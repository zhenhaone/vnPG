using System;
using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VNManager : MonoBehaviour
{
    #region 变量
    public GameObject gamePanel;
    public GameObject dialoguePanBox;
    public TextMeshProUGUI speakerName;
    //public TextMeshProUGUI speakingContent;
    public TypewritterEffect typewritterEffect;
    public ScreenShooter screenShooter;

    public Image avatarImage;
    public AudioSource vocalAudio;
    public Image backgroundImage;
    public AudioSource backgroundMusic;
    public Image CharactorImage1;
    public Image CharactorImage2;
    public Image CharactorImage3;

    //public GameObject choicePanel;
    //public Button choiceButton1;
    //public Button choiceButton2;

    public GameObject bottomButtons;
    public Button autoButton;
    public Button skipButton;
    public Button saveButton;
    public Button loadButton;
    public Button historyButton;
    public Button settingButton;
    public Button homeButton;
    public Button closeButton;

    private string storyPath = Constants.STORY_PATH;
    private readonly string defaultStoryFileName = Constants.DEFAULT_STORY_FILE_NAME;
    private int defaultStartLine = Constants.DEFAULT_START_LINE;
    private string excelFileExtension=Constants.EXCEL_FILE_EXTENSION;

    private string saveFolderPath;
    private byte[] screenshotData;
    private string currentSpeakingContent;

    private List<ExcelReader.ExcelData> storyData;
    private int currentLine;
    private string currentStoryFileName;
    private float currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;

    private bool isAutoPlay = false;
    private bool isSkip = false;
    private bool isLoad = false;
    private int maxReachedLineIndex = 0;
    private Dictionary<string, int> globalMaxReachedLineIndices = new Dictionary<string, int>();
    private LinkedList<string> historyRecords=new LinkedList<string>();

    public static VNManager Instance { get; private set; }
    #endregion
    #region 生命周期


    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        
        //InitializeAndLoadStory(defaultStoryFileName);
        InitializeSaveFilePath();
        bottomButtonAddListener();
        //InitializeImage();
        //gamePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(!MenuManager.Instance.menuPanel.activeSelf&&
            !SLManager.Instance.saveLoadPanel.activeSelf&&
            !HistoryManager.Instance.historyScrollView.activeSelf&&
            !SettingManager.Instance.settingPanel.activeSelf&&
            !ChoiceManager.Instance.choicePanel.activeSelf&&
            gamePanel.activeSelf&&Input.GetMouseButtonDown(0))
        {
            if(Input.GetMouseButtonDown(0)||Input.GetKeyDown(KeyCode.Space))
            {
                if(!dialoguePanBox.activeSelf)
                {
                    OpenUI();
                }
                else if(!IsHittingBottomButtons())
                {
                    Debug.Log("1");
                    
                    DisplayNextLine();
                }
            }    
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(dialoguePanBox.activeSelf)
                {
                    CloseUI();
                }
                else
                {
                    OpenUI();
                }
            }
            if(Input.GetKeyDown(KeyCode.LeftControl)||Input.GetKeyDown(KeyCode.RightControl))
            {
                Debug.Log("ctrl");
                CtrlSkip();
            }

            if(!dialoguePanBox.activeSelf)
            {
                OpenUI();
            }
            else if(!IsHittingBottomButtons())
            {  
                DisplayNextLine(); 
            }
        }
    }
    #endregion
    # region 初始化
    void InitializeSaveFilePath()
    {
        saveFolderPath = Path.Combine(Application.persistentDataPath,Constants.SAVE_FILE_PATH);
        if(!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }
    }

    void bottomButtonAddListener()
    {
        autoButton.onClick.AddListener(OnAutoButtonClick);
        skipButton.onClick.AddListener(OnSkipButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        loadButton.onClick.AddListener(OnLoadButtonClick);
        historyButton.onClick.AddListener(OnHistoryButtonClick);
        settingButton.onClick.AddListener(OnSettingButtonClick);
        homeButton.onClick.AddListener(OnHomeButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }

    public void StartGame(string filename,int startLine)
    {
        InitializeAndLoadStory(filename, startLine);
    }

    void InitializeAndLoadStory(string fileName,int lineNumber)
    {
        //Debug.Log(fileName);
        Initialize(lineNumber);
        LoadStoryFromFile(fileName);
        if(isLoad)
        {
            RecoverLastBackgroundAndCharactor();
            isLoad = false;
        }
        DisplayNextLine();
    }

    private void Initialize(int lineNUmber)
    {
        currentLine = lineNUmber;

        backgroundMusic.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);

        avatarImage.gameObject.SetActive(false);
        vocalAudio.gameObject.SetActive(false);

        CharactorImage1.gameObject.SetActive(false);
        CharactorImage2.gameObject.SetActive(false);

        //choicePanel.SetActive(false);
        
        //autoButton.onClick.AddListener(OnAutoButtonClick);
    }

    void LoadStoryFromFile(string fileName)
    {
        currentStoryFileName = fileName;
        Debug.Log(Application.streamingAssetsPath);
        string path = Path.Combine(Application.streamingAssetsPath, "story/", fileName+Constants.EXCEL_FILE_EXTENSION);// fileName + excelFileExtension;
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
    #endregion
    #region 展示
    void DisplayNextLine()
    {
        Debug.Log("1");
        if(currentLine>maxReachedLineIndex)
        {
            maxReachedLineIndex = currentLine;
            globalMaxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
        if(currentLine>=storyData.Count-1)
        {
            Debug.Log("2");
            if (isAutoPlay)
            {
                isAutoPlay = false;
                UpdateButtonImage(Constants.AUTO_OFF,autoButton);
            }
            Debug.Log(storyData[currentLine].speakerName);

            if (storyData[currentLine].speakerName==Constants.END_OD_STORY)
            {
                Debug.Log(Constants.END_OD_STORY);
                SceneManager.LoadScene("GameScene");
            }
            if (storyData[currentLine].speakerName==Constants.CHOICE)
            {
                ShowChoices();
                
            }
            if (storyData[currentLine].speakerName==Constants.GOTO)
            {
                InitializeAndLoadStory(storyData[currentLine].speakingContent,defaultStartLine);
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
        currentSpeakingContent = data.speakingContent;
        //speakingContent.text = data.speakingContent;
        typewritterEffect.StartTyping(currentSpeakingContent,currentTypingSpeed);

        RecordHistory(speakerName.text,currentSpeakingContent);

        if(NotNullNorEmpty(data.avatarImageFileName))
        {
            UpdateAvatarImage(data.avatarImageFileName);
        }
        else
        {
            Debug.Log("NULL");
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
        if (NotNullNorEmpty(data.charactor3Action))
        {
            UpdateCharactorImage(data.charactor3Action, data.charactor3ImageFileName, CharactorImage3, data.CoordinateX3);
        }
        currentLine++;
    }

    void RecordHistory(string speaker,string content)
    {
        string historyRecord = speaker + Constants.COLON + content;
        if(historyRecords.Count>=Constants.MAX_LENGTH)
        {
            historyRecords.RemoveFirst();
        }
        historyRecords.AddLast(historyRecord);
    }

    void RecoverLastBackgroundAndCharactor()
    {
        var data = storyData[currentLine];
        if(NotNullNorEmpty(data.lastBackgroundImage))
        {
            UpdateBackgroundImage(data.lastBackgroundImage);
        }
        if(NotNullNorEmpty(data.lastBackgroundMusic))
        {
            PlayBackgroundMusic(data.lastBackgroundMusic);
        }
        if (data.charactor1Action!=Constants.charactorActionAppearAt&& NotNullNorEmpty(data.lastCoordinate1))
        {
            UpdateCharactorImage(Constants.charactorActionAppearAt,data.charactor1ImageFileName,CharactorImage1, data.lastCoordinate1);
        }
        if (data.charactor2Action != Constants.charactorActionAppearAt&&NotNullNorEmpty(data.lastCoordinate2))
        {
            UpdateCharactorImage(Constants.charactorActionAppearAt, data.charactor2ImageFileName, CharactorImage2, data.lastCoordinate2);
        }
        if (data.charactor3Action != Constants.charactorActionAppearAt && NotNullNorEmpty(data.lastCoordinate3))
        {
            UpdateCharactorImage(Constants.charactorActionAppearAt, data.charactor3ImageFileName, CharactorImage3, data.lastCoordinate3);
        }
    }

    bool NotNullNorEmpty(string str)
    {
        return !string.IsNullOrEmpty(str);
    }
    #endregion
    #region 选择
    void ShowChoices()
    {
        var data = storyData[currentLine];
        var choices = data.speakingContent.Split(Constants.ChoiceDelimeter).Select(s => s.Trim()).ToList();
        var actions=data.avatarImageFileName.Split(Constants.ChoiceDelimeter).Select(s => s.Trim()).ToList();
        ChoiceManager.Instance.ShowChoices(choices,actions,HandleChoice);
        //choiceButton1.onClick.RemoveAllListeners();
        //choiceButton2.onClick.RemoveAllListeners();
        //choicePanel.SetActive(true);
        //choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = data.speakingContent;
        //choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = data.vocalAudioFileName;
        //choiceButton1.onClick.AddListener(() => InitializeAndLoadStory(storyPath+data.avatarImageFileName,defaultStartLine));
        //choiceButton2.onClick.AddListener(() => InitializeAndLoadStory(storyPath+data.backgroundImageFileName, defaultStartLine));
    }

    void HandleChoice(string selectedChoice)
    {
        Debug.Log("click");
        currentLine = Constants.DEFAULT_START_LINE;
        LoadStoryFromFile(selectedChoice);
        DisplayNextLine();
    }
    #endregion
    #region 音效

    void PlayAudio(string audioPath, AudioSource audioSource, bool isLoop)
    {
        AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
            audioSource.loop = isLoop;
        }
        else
        {
            if (audioSource == vocalAudio)
            {
                Debug.Log(Constants.AUDIO_LOAD_FAILED + audioPath);
            }
            else if (audioSource == backgroundMusic)
            {
                Debug.Log(Constants.MUSIC_LOAD_FAILED + audioPath);
            }
        }
    }

    void PlayVocalAudio(string audioFileName)
    {
        string audioPath = Constants.VOCAL_PATH + audioFileName;

        PlayAudio(audioPath,vocalAudio,false);
    }

    

    void PlayBackgroundMusic(string musicFielName)
    {
        string musicPath = Constants.MUSIC_PATH + musicFielName;
        PlayAudio(musicPath, backgroundMusic, false);
    }
    #endregion
    #region 图片
    void UpdateAvatarImage(string imageFileName)
    {
        string imagePath = Constants.AVATAR_PATH + imageFileName;
        UpdateImage(imagePath, avatarImage);
    }

    void UpdateBackgroundImage(string imageFileName)
    {
        string imagePath = Constants.BACKGROUND_PATH + imageFileName;
        UpdateImage(imagePath, backgroundImage);

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
                charactorImage.DOFade(1,((isLoad||action==Constants.APPEAR_AT_INSTANTLY)?0:Constants.DURATION_TIME)).From(0);
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
    #endregion
    #region 按钮
    #region bottom

    bool IsHittingBottomButtons()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            bottomButtons.GetComponent<RectTransform>(),
            Input.mousePosition,
            Camera.main
            );
    }
    #endregion
    #region auto
    //private bool isAutoPlay = false;

    void OnAutoButtonClick()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay?Constants.AUTO_OFF:Constants.AUTO_ON),autoButton);
        if(isAutoPlay)
        {
            StartCoroutine(StartAutoPlay());
        }
    }

    private IEnumerator StartAutoPlay()
    {
        while (isAutoPlay)
        {
            if (!typewritterEffect.IsTyping())
            {
                DisplayNextLine();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_AUTO_WAITING_SECONDS);
        }
    }
    #endregion
    #region skip
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

    void StartSkip()
    {
        Debug.Log("StartSkip");
        isSkip = true;
        UpdateButtonImage(Constants.SKIP_ON, skipButton);
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipToMaxReachedLine());
    }

    bool CanSkip()
    {
        //Debug.Log(currentLine+" "+ maxReachedLineIndex);
        return currentLine < maxReachedLineIndex;
    }

    private IEnumerator SkipToMaxReachedLine()
    {
        while (isSkip)
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
        currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;
        UpdateButtonImage(Constants.SKIP_OFF, skipButton);
    }

    void CtrlSkip()
    {
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED;
        StartCoroutine(SkipWhilePressingCtrl());
    }

    private IEnumerator SkipWhilePressingCtrl()
    {
        while(Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.RightControl))
        {
            DisplayNextLine();
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITTING_SECONDS);
        }
    }

    #endregion
    #region save

    void SaveGame(int slotIndex)
    {
        var saveData = new SaveData
        {
            savedStoryFileName = currentStoryFileName,
            savedLine = currentLine,
            savedSpeakingContent=currentSpeakingContent,
            savedScreenshotData=screenshotData,
            savedHistoryRecords=historyRecords
        };
        string savePath = Path.Combine(saveFolderPath,slotIndex+Constants.SAVE_FILE_EXTENSION);
        string json = JsonConvert.SerializeObject(saveData,Formatting.Indented);
        File.WriteAllText(savePath,json);
    }

    void OnSaveButtonClick()
    {
        CloseUI();
        Texture2D screenshot = screenShooter.CaptureScrennshot();
        screenshotData = screenshot.EncodeToPNG();
        SLManager.Instance.ShowSavePanel(SaveGame);
        //SLManager.Instance.ShowSaveLoadUI(true);
        OpenUI();
    }

    public class SaveData
    {
        public string savedStoryFileName;
        public int savedLine;
        public string savedSpeakingContent;
        public byte[] savedScreenshotData;
        public LinkedList<string> savedHistoryRecords;
    }
    #endregion
    #region load
    void OnLoadButtonClick()
    {
        //SLManager.Instance.ShowSaveLoadUI(false);
        
        ShowLoadPanel(null);
    }

    public void ShowLoadPanel(Action action)
    {
        SLManager.Instance.ShowLoadPanel(LoadGame,action);
    }

    void LoadGame(int slotIndex)
    {
        string savePath = Path.Combine(saveFolderPath, slotIndex + Constants.SAVE_FILE_EXTENSION);
        if(File.Exists(savePath))
        {
            isLoad = true;
            string json=File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<SaveData>(json);
            historyRecords = saveData.savedHistoryRecords;
            historyRecords.RemoveLast();
            var lineNumber = saveData.savedLine - 1;
            //Debug.Log(saveData.savedStoryFileName);
            InitializeAndLoadStory(saveData.savedStoryFileName,lineNumber);
        }
    }
    #endregion
    #region home
    void OnHomeButtonClick()
    {
        gamePanel.SetActive(false);
        MenuManager.Instance.menuPanel.SetActive(true);
    }
    #endregion
    #region history
    void OnHistoryButtonClick()
    {
        HistoryManager.Instance.ShowHistory(historyRecords);
    }
    #endregion
    #region setting
    private void OnSettingButtonClick()
    {
        SettingManager.Instance.ShowSettingPanel();
    }
    #endregion
    #region close
    void OnCloseButtonClick()
    {
        CloseUI();
    }
    #endregion


    void UpdateButtonImage(string imageFileName,Button button)
    {
        string imagePath = Constants.BUTTON_PATH + imageFileName;
        UpdateImage(imagePath,button.image);
    } 

    void OpenUI()
    {
        dialoguePanBox.SetActive(true);
        bottomButtons.SetActive(true);
    }

    void CloseUI()
    {
        dialoguePanBox.SetActive(false);
        bottomButtons.SetActive(false);
    }
    #endregion
}
