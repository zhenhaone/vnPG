using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExcelDataReader.Log;

public class TypewritterEffect : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public float waitingSeconds = Constants.DEFAULT_WAITING_SECONDS;

    private Coroutine typingCoroutine;
    private bool isTyping;

    public void StartTyping(string text)
    {
        if(typingCoroutine!=null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(Typeline(text));
    }
    
    private IEnumerator Typeline(string text)
    {
        isTyping = true;
        textDisplay.text = text;
        print(text);
        textDisplay.maxVisibleCharacters = 0;

        for(int i=0;i<text.Length;i++)
        {
            textDisplay.maxVisibleCharacters = i+1;
            yield return new WaitForSeconds(waitingSeconds);
        }

        isTyping = false;
    } 

    public void CompleteLine()
    {
        if(typingCoroutine!=null)
        {
            StopCoroutine(typingCoroutine);
        }

        textDisplay.maxVisibleCharacters=textDisplay.text.Length;
        print(textDisplay.maxVisibleCharacters);
        isTyping = false;
    }

    public bool IsTyping()
    {
        return isTyping;
    }
}
