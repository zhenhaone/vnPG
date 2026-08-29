using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TypewritterEffect : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;

    public float typingSpeed = 0.05f;

    [Header("打字机音效")]
    [Tooltip("在Inspector中挂载打字机音频")]
    public AudioClip typewriterAudioClip;

    [Range(0f, 1f)]
    public float typewriterAudioVolume = 0.6f;

    [Tooltip("文字显示时间超过音频长度时循环播放")]
    public bool loopTypewriterAudio = true;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private AudioSource typewriterAudioSource;

    private void Awake()
    {
        typewriterAudioSource = GetComponent<AudioSource>();
        typewriterAudioSource.playOnAwake = false;
        typewriterAudioSource.loop = loopTypewriterAudio;
        typewriterAudioSource.volume = typewriterAudioVolume;
    }

    public void StartTyping(string text, float speed)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingSpeed = speed;
        PlayTypewriterAudio();
        typingCoroutine = StartCoroutine(TypeLine(text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;

        textDisplay.text = text;
        textDisplay.maxVisibleCharacters = 0;

        // Á¢¼´¸üÐÂTMPÎÄ±¾ÐÅÏ¢
        textDisplay.ForceMeshUpdate();

        // Ê¹ÓÃTMPÊµ¼Ê×Ö·ûÊýÁ¿£¬¼æÈÝ¸»ÎÄ±¾±êÇ©
        int characterCount = textDisplay.textInfo.characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            textDisplay.maxVisibleCharacters = i + 1;

            yield return new WaitForSeconds(typingSpeed);
        }

        // È·±£×îºóÈ«²¿ÏÔÊ¾
        textDisplay.maxVisibleCharacters = characterCount;

        isTyping = false;
        typingCoroutine = null;
        StopTypewriterAudio();
    }

    public void CompleteLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textDisplay.ForceMeshUpdate();
        textDisplay.maxVisibleCharacters =
            textDisplay.textInfo.characterCount;

        isTyping = false;
        StopTypewriterAudio();
    }

    public bool IsTyping()
    {
        return isTyping;
    }

    private void PlayTypewriterAudio()
    {
        if(typewriterAudioSource == null)
            typewriterAudioSource = GetComponent<AudioSource>();

        typewriterAudioSource.Stop();

        if(typewriterAudioClip == null)
            return;

        typewriterAudioSource.clip = typewriterAudioClip;
        typewriterAudioSource.volume = typewriterAudioVolume;
        typewriterAudioSource.loop = loopTypewriterAudio;
        typewriterAudioSource.Play();
    }

    private void StopTypewriterAudio()
    {
        if(typewriterAudioSource != null)
            typewriterAudioSource.Stop();
    }

    private void OnDisable()
    {
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        StopTypewriterAudio();
    }
}
