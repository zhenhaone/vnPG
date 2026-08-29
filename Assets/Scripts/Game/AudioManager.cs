using System.IO;
using UnityEngine;

/// <summary>
/// 全局音频管理器。
/// 音频文件应放在：
/// Assets/Resources/audio/music/
/// Assets/Resources/audio/vocal/
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float soundEffectVolume = 1f;

    private AudioSource musicSource;
    private AudioSource soundEffectSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = CreateAudioSource("MusicSource", true);
        soundEffectSource = CreateAudioSource("SoundEffectSource", false);
    }

    private AudioSource CreateAudioSource(string objectName, bool loop)
    {
        GameObject sourceObject = new GameObject(objectName);
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        return source;
    }

    /// <summary>
    /// 播放背景音乐。fileName可以是"rain"或"rain.wav"。
    /// </summary>
    public void PlayMusic(string fileName, bool loop = true)
    {
        AudioClip clip = LoadAudioClip(Constants.MUSIC_PATH, fileName);
        if (clip == null)
        {
            Debug.LogError(Constants.MUSIC_LOAD_FAILED + ": " + fileName);
            return;
        }

        // 同一首音乐正在播放时不重新开始。
        if (musicSource.isPlaying && musicSource.clip == clip)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    /// <summary>
    /// 播放音效或语音。多次调用可以让多个音效同时播放。
    /// 默认从Constants.VOCAL_PATH指定的目录加载。
    /// </summary>
    public void PlaySoundEffect(string fileName)
    {
        AudioClip clip = LoadAudioClip(Constants.VOCAL_PATH, fileName);
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + fileName);
            return;
        }

        soundEffectSource.PlayOneShot(clip, soundEffectVolume);
    }

    public void StopAllSoundEffects()
    {
        soundEffectSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSoundEffectVolume(float volume)
    {
        soundEffectVolume = Mathf.Clamp01(volume);
        soundEffectSource.volume = soundEffectVolume;
    }

    private AudioClip LoadAudioClip(string folderPath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        // Resources.Load路径不能包含.wav、.mp3等扩展名。
        string nameWithoutExtension = Path.ChangeExtension(fileName.Trim(), null);
        string resourcePath = (folderPath + nameWithoutExtension).Replace("\\", "/");
        return Resources.Load<AudioClip>(resourcePath);
    }
}
