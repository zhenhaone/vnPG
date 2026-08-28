using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameMgr : MonoBehaviour
{
    //public TextMeshProUGUI name;
    public Sprite[] sprites;
    public Image image;
    Color color;
    // Start is called before the first frame update
    void Start()
    {
        color = image.color;
        //name=this.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

        switch(this.GetComponent<TextMeshProUGUI>().text)
        {
            case "a":
                color.a = 1f; // 0完全透明，1完全不透明
                image.color = color;
                image.sprite = sprites[0];
            break;
            case "b":
                color.a = 1f; // 0完全透明，1完全不透明
                image.color = color;
                image.sprite = sprites[1];
                break;
            case "c":
                color.a = 1f; // 0完全透明，1完全不透明
                image.color = color;
                image.sprite = sprites[2];
                break;
            case "u":
                color.a = 1f; // 0完全透明，1完全不透明
                image.color = color;
                image.sprite = sprites[3];
                break;
            default:
                color.a = 0f; // 0完全透明，1完全不透明
                image.color = color;
                break;
        }
    }
}
