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
    [Range(0f, 1f)] public float backgroundMusicVolume = 1f;
    public Image CharactorImage1;
    public Image CharactorImage2;
    public Image CharactorImage3;

    [Header("证据演出")]
    [Tooltip("绑定场景中挂有 EvidencePresenter 脚本的对象")]
    public EvidencePresenter evidencePresenter;
    private bool isPlayingEvidence;

    [Header("角色立绘明暗")]
    [Tooltip("当前说话角色的颜色，白色表示保持原始亮度")]
    public Color speakingCharacterColor = Color.white;
    [Tooltip("当前未说话角色的颜色，RGB越低立绘越暗")]
    public Color silentCharacterColor = new Color(0.45f, 0.45f, 0.45f, 1f);

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
    private readonly List<AudioSource> backgroundMusicSources = new List<AudioSource>();
    private readonly Dictionary<string, AudioClip> backgroundMusicCache = new Dictionary<string, AudioClip>();

    public static VNManager Instance { get; private set; }
    #endregion
    #region 生命周期


    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            if(backgroundMusic != null)
            {
                backgroundMusic.playOnAwake = false;
                backgroundMusicSources.Add(backgroundMusic);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
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
        if (isPlayingEvidence ||
           (evidencePresenter != null && evidencePresenter.IsShowing))
        {
            return;
        }

        if (MenuManager.Instance.menuPanel.activeSelf ||
           SLManager.Instance.saveLoadPanel.activeSelf ||
           HistoryManager.Instance.historyPanel.activeSelf ||
           SettingManager.Instance.settingPanel.activeSelf ||
           ChoiceManager.Instance.choicePanel.activeSelf ||
           !gamePanel.activeSelf)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (dialoguePanBox.activeSelf)
                CloseUI();
            else
                OpenUI();

            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) ||
           Input.GetKeyDown(KeyCode.RightControl))
        {
            CtrlSkip();
        }

        bool advancePressed =
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space);

        if (!advancePressed)
            return;

        if (!dialoguePanBox.activeSelf)
        {
            OpenUI();
            return;
        }

        if (!IsHittingBottomButtons())
        {
            // 一次点击只调用一次
            DisplayNextLine();
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

        StopAllBackgroundMusic();
        backgroundImage.gameObject.SetActive(false);

        avatarImage.gameObject.SetActive(false);
        vocalAudio.gameObject.SetActive(false);

        CharactorImage1.gameObject.SetActive(false);
        CharactorImage2.gameObject.SetActive(false);
        CharactorImage3.gameObject.SetActive(false);

        //choicePanel.SetActive(false);
        
        //autoButton.onClick.AddListener(OnAutoButtonClick);
    }

    void LoadStoryFromFile(string fileName)
    {
        currentStoryFileName = fileName;
        //Debug.Log(Application.streamingAssetsPath);
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
        //Debug.Log("1");
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

        // Excel特殊演出行：
        // speakerName填写 EVIDENCE 或“证据”，speakingContent填写证据ID。
        if(IsEvidenceCommand(data.speakerName))
        {
            string evidenceId = data.speakingContent == null
                ? string.Empty
                : data.speakingContent.Trim();

            // 先推进索引，避免证据关闭后再次执行同一行。
            currentLine++;
            StartCoroutine(PlayEvidencePerformance(evidenceId));
            return;
        }

        speakerName.text = data.speakerName;
        UpdateSpeakingCharacterBrightness(data.speakerName);
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
            //Debug.Log("NULL");
            avatarImage.gameObject.SetActive(false);
        }
        if(NotNullNorEmpty(data.vocalAudioFileName))
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
            Debug.Log(data.charactor1ImageFileName +" "+ data.charactor1Action+" "+ data.CoordinateX1);
            UpdateCharactorImage(data.charactor1Action,data.charactor1ImageFileName,CharactorImage1,data.CoordinateX1);
        }
        if(NotNullNorEmpty(data.charactor2Action))
        {
            Debug.Log(data.charactor2ImageFileName + " " + data.charactor2Action + " " + data.CoordinateX2);
            UpdateCharactorImage(data.charactor2Action, data.charactor2ImageFileName, CharactorImage2, data.CoordinateX2);
        }
        if (NotNullNorEmpty(data.charactor3Action))
        {
            Debug.Log(data.charactor3ImageFileName + " " + data.charactor3Action + " " + data.CoordinateX3);
            UpdateCharactorImage(data.charactor3Action, data.charactor3ImageFileName, CharactorImage3, data.CoordinateX3);
        }
        currentLine++;
    }

    bool IsEvidenceCommand(string command)
    {
        if(string.IsNullOrWhiteSpace(command))
            return false;

        command = command.Trim();
        return command.Equals("EVIDENCE", StringComparison.OrdinalIgnoreCase)
            || command == "证据";
    }

    IEnumerator PlayEvidencePerformance(string evidenceId)
    {
        if(isPlayingEvidence)
            yield break;

        isPlayingEvidence = true;

        // 演出期间停止自动播放和跳过，防止后台继续读取剧情行。
        if(isAutoPlay)
        {
            isAutoPlay = false;
            UpdateButtonImage(Constants.AUTO_OFF, autoButton);
        }

        if(isSkip)
            EndSkip();

        if(evidencePresenter == null)
        {
            Debug.LogError("VNManager没有绑定EvidencePresenter，无法展示证据：" + evidenceId);
        }
        else if(string.IsNullOrWhiteSpace(evidenceId))
        {
            Debug.LogError("证据演出行没有填写证据ID。");
        }
        else
        {
            yield return evidencePresenter.ShowEvidenceAndWait(evidenceId);
        }

        isPlayingEvidence = false;

        // 玩家关闭证据后，自动显示下一行视觉小说文本。
        DisplayNextLine();
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
        //Debug.Log(audioPath);
        AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.gameObject.SetActive(true);
            audioSource.loop = isLoop;
            audioSource.Play();
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

    public void PlayVocalAudio(string audioFileName)
    {
        string audioPath = Constants.VOCAL_PATH + audioFileName;

        PlayAudio(audioPath,vocalAudio,false);
    }

    

    void PlayBackgroundMusic(string musicFileName)
    {
        if(string.IsNullOrEmpty(musicFileName)) return;

        string musicPath = Constants.MUSIC_PATH + musicFileName;

        if(!backgroundMusicCache.TryGetValue(musicPath, out AudioClip musicClip))
        {
            musicClip = Resources.Load<AudioClip>(musicPath);
            if(musicClip == null)
            {
                Debug.LogError(Constants.MUSIC_LOAD_FAILED + musicPath);
                return;
            }
            backgroundMusicCache.Add(musicPath, musicClip);
        }

        // 相同BGM已经播放时不重复创建声源，也不从头播放。
        foreach(AudioSource source in backgroundMusicSources)
        {
            if(source != null && source.isPlaying && source.clip == musicClip)
                return;
        }

        AudioSource musicSource = GetAvailableBackgroundMusicSource();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.volume = backgroundMusicVolume;
        musicSource.gameObject.SetActive(true);
        musicSource.Play();
    }

    AudioSource GetAvailableBackgroundMusicSource()
    {
        foreach(AudioSource source in backgroundMusicSources)
        {
            if(source != null && !source.isPlaying)
                return source;
        }

        GameObject sourceObject = new GameObject("BackgroundMusic_" + backgroundMusicSources.Count);
        sourceObject.transform.SetParent(transform, false);
        AudioSource newSource = sourceObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = true;

        // 让动态创建的声源使用与原BGM声源相同的混音器和基础设置。
        if(backgroundMusic != null)
        {
            newSource.outputAudioMixerGroup = backgroundMusic.outputAudioMixerGroup;
            newSource.mute = backgroundMusic.mute;
            newSource.priority = backgroundMusic.priority;
            newSource.pitch = backgroundMusic.pitch;
            newSource.panStereo = backgroundMusic.panStereo;
            newSource.spatialBlend = backgroundMusic.spatialBlend;
        }

        backgroundMusicSources.Add(newSource);
        return newSource;
    }

    public void StopBackgroundMusic(string musicFileName)
    {
        string musicPath = Constants.MUSIC_PATH + musicFileName;
        if(!backgroundMusicCache.TryGetValue(musicPath, out AudioClip musicClip)) return;

        foreach(AudioSource source in backgroundMusicSources)
        {
            if(source != null && source.clip == musicClip)
            {
                source.Stop();
                source.clip = null;
                source.gameObject.SetActive(false);
            }
        }
    }

    public void StopAllBackgroundMusic()
    {
        foreach(AudioSource source in backgroundMusicSources)
        {
            if(source == null) continue;
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
        }
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        backgroundMusicVolume = Mathf.Clamp01(volume);
        foreach(AudioSource source in backgroundMusicSources)
        {
            if(source != null)
                source.volume = backgroundMusicVolume;
        }
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
                if(charactorImage.rectTransform.anchoredPosition.x!=newPosition.x)
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
                string imagePath = Constants.CHARACTOR_PATH + imageFileName;
                //Sprite sprite = Resources.Load<Sprite>(imagePath);
                //if (sprite != null && sprite != charactorImage.sprite) UpdateImage(imagePath, charactorImage);
                UpdateImage(imagePath, charactorImage);
                //var newPosition = new Vector2(float.Parse(x), charactorImage.rectTransform.anchoredPosition.y);
                //charactorImage.rectTransform.anchoredPosition = newPosition;
                //charactorImage.DOFade(1, ((isLoad || action == Constants.APPEAR_AT_INSTANTLY) ? 0 : Constants.DURATION_TIME)).From(0);

                charactorImage.rectTransform.DOAnchorPosX(float.Parse(x),Constants.DURATION_TIME);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }
        }
    }

    void UpdateSpeakingCharacterBrightness(string currentSpeaker)
    {
        if(string.IsNullOrEmpty(currentSpeaker)) return;

        string speaker = currentSpeaker.Trim().ToLowerInvariant();

        // 只有角色自身说话时恢复亮度；u、narrator及其他说话者会让三人全部变暗。
        SetCharacterBrightness(CharactorImage1, speaker == "a");
        SetCharacterBrightness(CharactorImage2, speaker == "b");
        SetCharacterBrightness(CharactorImage3, speaker == "c");
    }

    void SetCharacterBrightness(Image characterImage, bool isSpeaking)
    {
        if(characterImage == null) return;

        Color targetColor = isSpeaking ? speakingCharacterColor : silentCharacterColor;

        // 只改变RGB，保留淡入、淡出动画当前使用的透明度。
        targetColor.a = characterImage.color.a;
        characterImage.color = targetColor;
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
