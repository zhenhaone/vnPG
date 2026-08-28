using System.Collections;
using TMPro;
using UnityEngine;

public class TypewritterEffect : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;

    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;
    private bool isTyping;

    public void StartTyping(string text, float speed)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        typingSpeed = speed;
        typingCoroutine = StartCoroutine(TypeLine(text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;

        textDisplay.text = text;
        textDisplay.maxVisibleCharacters = 0;

        // 立即更新TMP文本信息
        textDisplay.ForceMeshUpdate();

        // 使用TMP实际字符数量，兼容富文本标签
        int characterCount = textDisplay.textInfo.characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            textDisplay.maxVisibleCharacters = i + 1;

            yield return new WaitForSeconds(typingSpeed);
        }

        // 确保最后全部显示
        textDisplay.maxVisibleCharacters = characterCount;

        isTyping = false;
        typingCoroutine = null;
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
    }

    public bool IsTyping()
    {
        return isTyping;
    }
}
