using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public Button startButton;
    public Button continueButton;
    public Button loadButton;
    public Button settingButton;
    public Button quitButton;

    private bool hasStarted = false;
    public static MenuManager Instance  { get; private set; }

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
        MenuButtonAddListener();
        startButton.image.alphaHitTestMinimumThreshold = 0.1f;
        continueButton.image.alphaHitTestMinimumThreshold = 0.1f;
        quitButton.image.alphaHitTestMinimumThreshold = 0.1f;
    }

    void MenuButtonAddListener()
    {
        startButton.onClick.AddListener(StartGame);
        continueButton.onClick.AddListener(ContinueGame);
        loadButton.onClick.AddListener(LoadGame);
        settingButton.onClick.AddListener(ShowSettingPanel);
        quitButton.onClick.AddListener(QuitGame);
    }

    void StartGame()
    {
        hasStarted = true;
        VNManager.Instance.StartGame(Constants.NEW_STORY_FILE_NAME,Constants.DEFAULT_START_LINE);
        //menuPanel.SetActive(false);
        //VNManager.Instance.gamePanel.SetActive(true);
        ShowGamePanel();
    }

    void ContinueGame()
    {
        if(hasStarted)
        {
            //menuPanel.SetActive(false);
            //VNManager.Instance.gamePanel.SetActive(true);
            ShowGamePanel();
        }
    }

    private void LoadGame()
    {
        VNManager.Instance.ShowLoadPanel(ShowGamePanel);
    }

    private void ShowGamePanel()
    {
        menuPanel.SetActive(false);
        VNManager.Instance.gamePanel.SetActive(true);
    }

    private void ShowSettingPanel()
    {
        SettingManager.Instance.ShowSettingPanel();
    }

    private void QuitGame()
    {
        UnityEngine.Application.Quit();
    }
}
