using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryManager : MonoBehaviour
{
    public Transform historyContent;
    public GameObject historyItemPrefab;
    public GameObject historyScrollView;
    public Button closeButton;
    public GameObject historyPanel;

    private LinkedList<string> historyRecords;

    public static HistoryManager Instance { get; private set; }

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

    private void Start()
    {
        historyPanel.SetActive(false);
        closeButton.onClick.AddListener(CloseHistory);
    }

    public void ShowHistory(LinkedList<string> records)
    {
        foreach(Transform child in historyContent)
        {
            Destroy(child.gameObject);
        }
        historyRecords = records;
        LinkedListNode<string> currentNode = historyRecords.Last;
        while(currentNode!=null)
        {
            AddHistoryItem(currentNode.Value);
            currentNode = currentNode.Previous;
        }

        historyContent.GetComponent<RectTransform>().localPosition = Vector3.zero;
        historyPanel.SetActive(true);

    }

    public void CloseHistory()
    {
        historyPanel?.SetActive(false);
    }

    private void AddHistoryItem(string text)
    {
        GameObject historyItem = Instantiate(historyItemPrefab,historyContent);
        historyItem.GetComponentInChildren<TextMeshProUGUI>().text = text;
        historyItem.transform.SetAsFirstSibling();
    }
}
