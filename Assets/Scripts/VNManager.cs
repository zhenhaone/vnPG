using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VNManager : MonoBehaviour
{
    public TextMeshProUGUI speakerName;
    public TextMeshProUGUI speakingContent;
    public TypewritterEffect typewritterEffect;
    public Image avatarImage;
    public AudioSource vocalAudio;
    public Image backgroundImage;
    public AudioSource backgroundMusic;
    public Image CharactorImage1;
    public Image CharactorImage2;

    public GameObject choicePanel;
    public Button choiceButton1;
    public Button choiceButton2;

    public GameObject bottomButtons;
    public Button autoButton;
    public Button skipButton;
    public Button saveButton;
    public Button loadButton;

    private string storyPath = Constants.STORY_PATH;
    private string defaultStoryFileName = Constants.DEFAULT_STORY_FILE_NAME;
    private string excelFileExtension=Constants.EXCEL_FILE_EXTENSION;
    private List<ExcelReader.ExcelData> storyData;
    private int currentLine;
    private string currentStoryFileName;

    private bool isAutoPlay = false;
    private bool isSkip = false;
    private int maxReachedLineIndex = 0;
    private Dictionary<string, int> globalMaxReachedLineIndices = new Dictionary<string, int>();
    
    

    // Start is called before the first frame update
    void Start()
    {
        InitializeAndLoadStory(defaultStoryFileName);
        bottomButtonAddListener();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            if (!IsHittingBottomButtons())
            {
                Debug.Log("Hit");
                DisplayNextLine();
            }
            
        }

    }

    void bottomButtonAddListener()
    {
        autoButton.onClick.AddListener(OnAutoButtonClick);
        skipButton.onClick.AddListener(OnSkipButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        loadButton.onClick.AddListener(OnLoadButtonClick);
    }

    void InitializeAndLoadStory(string fileName)
    {
        //Debug.Log(fileName);
        Initialize();
        LoadStoryFromFile(storyPath + fileName);
        DisplayNextLine();
    }

    private void Initialize()
    {
        currentLine = Constants.DEFAULT_START_LINE;
        avatarImage.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
        CharactorImage1.gameObject.SetActive(false);
        CharactorImage2.gameObject.SetActive(false);
        choicePanel.SetActive(false);
        //autoButton.onClick.AddListener(OnAutoButtonClick);
    }

    void LoadStoryFromFile(string fileName)
    {
        currentStoryFileName = fileName;
        var path = fileName + excelFileExtension;
        storyData = ExcelReader.ReadExcel(path);
        if(storyData==null||storyData.Count==0)
        {
            Debug.LogError("No data found in the file");
        }
        if(globalMaxReachedLineIndices.ContainsKey(currentStoryFileName))
        {
            maxReachedLineIndex=globalMaxReachedLineIndices[currentStoryFileName];
        }
        else
        {
            maxReachedLineIndex = 0;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
    }

    void DisplayNextLine()
    {
        if(currentLine>maxReachedLineIndex)
        {
            maxReachedLineIndex = currentLine;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
        if(currentLine>=storyData.Count-1)
        {
            if(isAutoPlay)
            {
                isAutoPlay = false;
                UpdateButtonImage(Constants.AUTO_OFF,autoButton);
            }

            if (storyData[currentLine].speakerName==Constants.END_OD_STORY)
            {
                Debug.Log(Constants.END_OD_STORY);
            }
            if (storyData[currentLine].speakerName==Constants.CHOICE)
            {
                ShowChoices();
                
            }
            return;
        }
        if(typewritterEffect.IsTyping())
        {
            typewritterEffect.CompleteLine();
        }
        else
        {
            DisplayThisLine(); 
        }
        
    }

    void DisplayThisLine()
    {
        var data = storyData[currentLine];
        speakerName.text = data.speakerName;
        speakingContent.text = data.speakingContent;
        typewritterEffect.StartTyping(speakingContent.text);
        if(NotNullNorEmpty(data.avatarImageFileName))
        {
            UpdateAvatarImage(data.avatarImageFileName);
        }
        else
        {
            avatarImage.gameObject.SetActive(false);
        }
        if(NotNullNorEmpty(data.avatarImageFileName))
        {
            PlayVocalAudio(data.vocalAudioFileName);
        }
        if(NotNullNorEmpty(data.backgroundImageFileName))
        {
            UpdateBackgroundImage(data.backgroundImageFileName);
        }
        if(NotNullNorEmpty(data.backgroundMusicFileName))
        {
            PlayBackgroundMusic(data.backgroundMusicFileName);
        }
        if(NotNullNorEmpty(data.charactor1Action))
        {
            UpdateCharactorImage(data.charactor1Action,data.charactor1ImageFileName,CharactorImage1,data.CoordinateX1);
        }
        if(NotNullNorEmpty(data.charactor2Action))
        {
            UpdateCharactorImage(data.charactor2Action, data.charactor2ImageFileName, CharactorImage2, data.CoordinateX2);
        }
            currentLine++;
    }

    bool NotNullNorEmpty(string str)
    {
        return !string.IsNullOrEmpty(str);
    }

    void ShowChoices()
    {
        var data = storyData[currentLine];
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();
        choicePanel.SetActive(true);
        choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = data.speakingContent;
        choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = data.vocalAudioFileName;
        choiceButton1.onClick.AddListener(() => InitializeAndLoadStory(data.avatarImageFileName));
        choiceButton2.onClick.AddListener(() => InitializeAndLoadStory(data.backgroundImageFileName));
    }

    void UpdateAvatarImage(string imageFileName)
    {
        string imagePath = Constants.AVATAR_PATH + imageFileName;
        UpdateImage(imagePath,avatarImage);
    }

    void PlayVocalAudio(string audioFileName)
    {
        string audioPath = Constants.VOCAL_PATH + audioFileName;

        PlayAudio(audioPath,vocalAudio,false);
    }

    void UpdateBackgroundImage(string imageFileName)
    {
        string imagePath = Constants.BACKGROUND_PATH + imageFileName;
        UpdateImage(imagePath, backgroundImage);

    }

    void PlayBackgroundMusic(string musicFielName)
    {
        string musicPath = Constants.MUSIC_PATH + musicFielName;
        PlayAudio(musicPath, backgroundMusic, false);
    }

    void UpdateCharactorImage(string action,string imageFileName,Image charactorImage,string x)
    {
        if (action.StartsWith(Constants.charactorActionAppearAt))
        {
            string imagePath=Constants.CHARACTOR_PATH+ imageFileName;
            if (NotNullNorEmpty(x))
            {
                UpdateImage(imagePath, charactorImage);
                var newPosition = new Vector2(float.Parse(x),charactorImage.rectTransform.anchoredPosition.y);
                charactorImage.rectTransform.anchoredPosition= newPosition;
                charactorImage.DOFade(1,Constants.DURATION_TIME).From(0);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }
        }
        else if(action==Constants.charactorActionDisappear)
        {
            charactorImage.DOFade(0,Constants.DURATION_TIME).OnComplete(()=>charactorImage.gameObject.SetActive(false));
            
        }
        else if(action.StartsWith(Constants.charactorActionMoveTo))
        {
            if(NotNullNorEmpty(x))
            {
                charactorImage.rectTransform.DOAnchorPosX(float.Parse(x),Constants.DURATION_TIME);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }
        }
    }

    void UpdateImage(string imagePath,Image image)
    {
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if(sprite!=null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAD_FAILED+imagePath);
        }
    }

    void PlayAudio(string audioPath,AudioSource audioSource,bool isLoop)
    {
        AudioClip audioClip=Resources.Load<AudioClip>(audioPath);
        if(audioClip!=null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
            audioSource.loop = isLoop;
        }
        else
        {
            if(audioSource==vocalAudio)
            {
                Debug.LogError(Constants.AUDIO_LOAD_FAILED+audioPath);
            }
            else if(audioSource==backgroundMusic)
            {
                Debug.LogError(Constants.MUSIC_LOAD_FAILED+audioPath);
            }
        }
    }

    bool IsHittingBottomButtons()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            bottomButtons.GetComponent<RectTransform>(),
            Input.mousePosition,
            null
            );
    }

    //private bool isAutoPlay = false;

    void OnAutoButtonClick()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay?Constants.AUTO_ON:Constants.AUTO_OFF),autoButton);
        if(isAutoPlay)
        {
            StartCoroutine(StartAutoPlay());
        }
    }

    void OnSkipButtonClick()
    {
        Debug.Log("Click");
        if(!isSkip&&CanSkip())
        {
            StartSkip();
        }
        else if(isSkip)
        {
            StopCoroutine(SkipToMaxReachedLine());
            EndSkip();
        }
    }

    void OnSaveButtonClick()
    {
        SLManager.Instance.ShowSaveLoadUI(true);
    }

    void OnLoadButtonClick()
    {
        SLManager.Instance.ShowSaveLoadUI(false);
    }

    bool CanSkip()
    {
        //Debug.Log(currentLine+" "+ maxReachedLineIndex);
        return currentLine < maxReachedLineIndex;
    }

    void StartSkip()
    {
        Debug.Log("StartSkip");
        isSkip = true;
        UpdateButtonImage(Constants.SKIP_ON,skipButton);
        typewritterEffect.typingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipToMaxReachedLine());
    }

    void UpdateButtonImage(string imageFileName,Button button)
    {
        string imagePath = Constants.BUTTON_PATH + imageFileName;
        UpdateImage(imagePath,button.image);
    }

    private IEnumerator StartAutoPlay()
    {
        while(isAutoPlay)
        {
            if(!typewritterEffect.IsTyping())
            {
                DisplayNextLine();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_AUTO_WAITING_SECONDS);
        }
    }

    private IEnumerator SkipToMaxReachedLine()
    {
        while(isSkip)
        {
            if (CanSkip())
            {
                DisplayThisLine();
            }
            else
            {
                EndSkip();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITTING_SECONDS);
        }
    }

    void EndSkip()
    {
        isSkip = false;
        typewritterEffect.typingSpeed = Constants.DEFAULT_TYPING_SPEED;
        UpdateButtonImage(Constants.SKIP_OFF,skipButton);
    }
}
