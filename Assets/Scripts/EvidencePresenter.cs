using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 在视觉小说剧情中展示证据图片，并等待玩家点击后继续剧情。
/// </summary>
public class EvidencePresenter : MonoBehaviour
{
    [Serializable]
    public class EvidenceImage
    {
        [Tooltip("剧情脚本中使用的证据编号，例如 A_W1。")]
        public string id;

        public Sprite sprite;
    }

    [Header("UI References")]
    [Tooltip("证据展示面板。面板应位于对话界面上方。")]
    public GameObject evidencePanel;

    [Tooltip("用于显示证据图片。")]
    public Image evidenceImage;

    [Tooltip("覆盖整个屏幕的透明按钮，用于接收关闭证据的点击。")]
    public Button screenClickButton;

    [Header("Evidence Images")]
    public List<EvidenceImage> evidenceImages = new List<EvidenceImage>();

    public bool IsShowing { get; private set; }

    private readonly Dictionary<string, Sprite> spriteById =
        new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    private bool closeRequested;
    private bool canClose;

    private void Awake()
    {
        BuildEvidenceDictionary();

        if (screenClickButton != null)
            screenClickButton.onClick.AddListener(RequestClose);

        HideImmediately();
    }

    private void OnDestroy()
    {
        if (screenClickButton != null)
            screenClickButton.onClick.RemoveListener(RequestClose);
    }

    private void BuildEvidenceDictionary()
    {
        spriteById.Clear();

        foreach (EvidenceImage evidence in evidenceImages)
        {
            if (evidence == null || string.IsNullOrWhiteSpace(evidence.id))
                continue;

            spriteById[evidence.id.Trim()] = evidence.sprite;
        }
    }

    /// <summary>
    /// 显示证据并暂停调用者的剧情协程，点击屏幕后才会结束。
    /// 使用方式：yield return evidencePresenter.ShowEvidenceAndWait("A_W1");
    /// </summary>
    public IEnumerator ShowEvidenceAndWait(string evidenceId)
    {
        if (IsShowing)
        {
            Debug.LogWarning("当前已有证据正在展示，不能重复打开证据界面。");
            yield break;
        }

        if (!spriteById.TryGetValue(evidenceId, out Sprite sprite) || sprite == null)
        {
            Debug.LogError("没有找到证据图片：" + evidenceId);
            yield break;
        }

        if (evidencePanel == null || evidenceImage == null || screenClickButton == null)
        {
            Debug.LogError("EvidencePresenter 的 UI 引用没有绑定完整。");
            yield break;
        }

        IsShowing = true;
        closeRequested = false;
        canClose = false;

        evidenceImage.sprite = sprite;
        evidenceImage.preserveAspect = true;
        evidencePanel.SetActive(true);
        evidencePanel.transform.SetAsLastSibling();

        // 防止用于打开证据的同一次点击立刻把证据关闭。
        yield return null;
        canClose = true;

        yield return new WaitUntil(() => closeRequested);

        evidencePanel.SetActive(false);
        evidenceImage.sprite = null;
        IsShowing = false;
        canClose = false;

        // 等一帧再恢复剧情，防止关闭点击继续传递给对话系统。
        yield return null;
    }

    private void RequestClose()
    {
        if (IsShowing && canClose)
            closeRequested = true;
    }

    private void HideImmediately()
    {
        IsShowing = false;
        closeRequested = false;
        canClose = false;

        if (evidenceImage != null)
            evidenceImage.sprite = null;

        if (evidencePanel != null)
            evidencePanel.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildEvidenceDictionary();
    }
#endif
}
