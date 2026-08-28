using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class SLManager : MonoBehaviour
{
    public GameObject saveLoadPanel;
    public TextMeshProUGUI panelTitle;
    public Button[] saveLoadButtons;
    public Button prePageButton;
    public Button nextPageButton;
    public Button backButton;

    private bool isSave;
    private int currentPage = Constants.DEFAULT_START_INDEX;
    private readonly int slotsPerpage = Constants.SLOTS_PER_PAGE;
    private readonly int totalSlots = Constants.TOTAL_SLOTS;
    private System.Action<int> currentAction;
    private System.Action menuAction;

    public static SLManager Instance { get; private set; }

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
        prePageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(GoBack);
        saveLoadPanel.SetActive(false); 
    }

    public void ShowSavePanel(System.Action<int> action)
    {
        isSave = true;
        panelTitle.text = Constants.SAVE_GAME;
        currentAction = action;
        UpdateUI();
        saveLoadPanel.SetActive(true);
    }

    public void ShowLoadPanel(System.Action<int> action,System.Action menuAction)
    {
        isSave = false;
        panelTitle.text = Constants.LOAD_GAME;
        currentAction = action;
        this.menuAction= menuAction;
        UpdateUI();
        saveLoadPanel.SetActive(true);
    }

    private void UpdateUI()
    {
        for (int i=0; i < slotsPerpage;++i)
        {
            int slotIndex = currentPage * slotsPerpage + i;
            if(slotIndex<totalSlots)
            {
                UpdateSaveLoadButtons(saveLoadButtons[i],slotIndex);
                LoadStorylineAndScreenshots(saveLoadButtons[i],slotIndex);
            }
            else
            {
                //Debug.Log("no");
                saveLoadButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSaveLoadButtons(Button button,int index)
    {
        button.gameObject.SetActive(true);
        button.interactable= true;

        var savePath = GenerateDataPath(index);
        var fileExists = File.Exists(savePath);

        if(!isSave&&!fileExists)
        {
            button.interactable = false;
        }

        var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
        textComponents[0].text = null;
        textComponents[1].text = (index + 1) + Constants.COLON + Constants.EMPTY_SLOT;
        button.GetComponentInChildren<RawImage>().texture = null;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(()=>OnButtonClick(button,index));
    }

    private void OnButtonClick(Button button,int index)
    {
        Debug.Log("click");
        currentAction?.Invoke(index);
        menuAction?.Invoke();
        if (isSave)
        {
            LoadStorylineAndScreenshots(button,index);
        }
        else
        {
            GoBack();
        }
    }

    public void ShowSaveLoadUI(bool save)
    {
        isSave = save;
        panelTitle.text = isSave ? Constants.SAVE_GAME : Constants.LOAD_GAME;
        UpdateSaveLoadUI();
        saveLoadPanel.SetActive(true);
        //LoadStorylineAndScreenshots();
    }

    private void UpdateSaveLoadUI()
    {
        for(int i=0;i<slotsPerpage;i++)
        {
            int slotIndex = currentPage * slotsPerpage + i;
            if(slotIndex<totalSlots)
            {
                saveLoadButtons[i].gameObject.SetActive(true);
                saveLoadButtons[i].interactable = true;

                var slotText = (slotIndex+1)+Constants.COLON+Constants.EMPTY_SLOT;
                var textComponents = saveLoadButtons[i].GetComponentsInChildren<TextMeshProUGUI>();
                textComponents[0].text = null;
                textComponents[1].text = slotText;
                saveLoadButtons[i].GetComponentInChildren<RawImage>().texture = null;
                
            }
            else
            {
                saveLoadButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void PrevPage()
    {
        if(currentPage>0)
        {
            currentPage--;
            UpdateUI();
            //LoadStorylineAndScreenshots();
        }
    }

    private void NextPage()
    {
        if((currentPage+1)*slotsPerpage<totalSlots)
        {
            currentPage++;
            UpdateUI();
            //LoadStorylineAndScreenshots();
        }
    }

    private void GoBack()
    {
        saveLoadPanel.SetActive(false);
    }

    private void LoadStorylineAndScreenshots(Button button,int index)
    {
        var savePath = GenerateDataPath(index);
        if(File.Exists(savePath))
        {
            string json =File.ReadAllText(savePath);
            var saveData = JsonConvert.DeserializeObject<VNManager.SaveData>(json);
            if(saveData.savedScreenshotData!=null)
            {
                Debug.Log("Have");
                
                //Debug.Log(saveData.savedScreenshotData);
                Texture2D screenshot = new Texture2D(2,2);
                screenshot.LoadImage(saveData.savedScreenshotData);
                button.GetComponentInChildren<RawImage>().texture = screenshot;
                Color color = button.GetComponentInChildren<RawImage>().color;
                color.a = 1;
                button.GetComponentInChildren<RawImage>().color = color;
            }
            
            if (saveData.savedSpeakingContent != null)
            {
                var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
                //textComponents[0].text = saveData.savedSpeakingContent;
                textComponents[1].text = File.GetLastWriteTime(savePath).ToString("G");
            }
        }
        else
        {
            Color color = button.GetComponentInChildren<RawImage>().color;
            color.a = 0;
            button.GetComponentInChildren<RawImage>().color = color;
        }
    }

    private string GenerateDataPath(int index)
    {
        return Path.Combine(Application.persistentDataPath,Constants.SAVE_FILE_PATH,index+Constants.SAVE_FILE_EXTENSION);
    }
}
