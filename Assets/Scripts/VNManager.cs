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

    private string storyPath = Constants.STORY_PATH;
    private string defaultStoryFileName = Constants.DEFAULT_STORY_FILE_NAME;
    private List<ExcelReader.ExcelData> storyData;
    private int currentLine = Constants.DEFAULT_START_LINE;
    

    // Start is called before the first frame update
    void Start()
    {
        LoadStoryFromFile(storyPath+defaultStoryFileName);
        DisplayNextLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            DisplayNextLine(); 
        }

    }

    void LoadStoryFromFile(string path)
    {
        storyData = ExcelReader.ReadExcel(path);
        if(storyData==null||storyData.Count==0)
        {
            Debug.LogError("No data found in the file");
        }
    }

    void DisplayNextLine()
    {
        if(currentLine>=storyData.Count)
        {
            Debug.Log("End of story");
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
            currentLine++;
    }

    bool NotNullNorEmpty(string str)
    {
        return !string.IsNullOrEmpty(str);
    }

    void UpdateAvatarImage(string imageFileName)
    {
        string imagePath = Constants.AVATAR_PATH + imageFileName;
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if(sprite!=null)
        {
            avatarImage.sprite = sprite;
            avatarImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAD_FAILED+imagePath);
        }
    }

    void PlayVocalAudio(string audioFileName)
    {
        string audioPath = Constants.VOCAL_PATH + audioFileName;
        AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
        if(audioClip!=null)
        {
            vocalAudio.clip = audioClip;
            vocalAudio.Play();
        }
        else
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED+audioPath);
        }

    }

    void UpdateBackgroundImage(string imageFileName)
    {
        string imagePath = Constants.BACKGROUND_PATH + imageFileName;
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if(sprite!=null)
        {
            backgroundImage.sprite = sprite;
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAD_FAILED+imagePath);
        }

    }

    void PlayBackgroundMusic(string musicFielName)
    {
        string musicPath = Constants.MUSIC_PATH + musicFielName;
        AudioClip audioClip=Resources.Load<AudioClip>(musicPath);
        if(audioClip!=null)
        {
            backgroundMusic.clip = audioClip;
            backgroundMusic.Play();
            backgroundMusic.loop = true;
        }
        else
        {
            Debug.LogError(Constants.MUSIC_LOAD_FAILED+musicPath);
        }
    }
}
