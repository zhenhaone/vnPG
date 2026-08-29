using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NoVerity.GameScene
{
    public class GameSceneManager : MonoBehaviour
    {
        [Serializable]
        public class EvidenceSpriteBindings
        {
            [Header("Arthur Evidence")]
            public Sprite A_W1;
            public Sprite A_W2;
            public Sprite A_W3;
            public Sprite A_W4;
            public Sprite A_S1;
            public Sprite A_S2;
            public Sprite A_S3;
            public Sprite A_C1;
            public Sprite A_C2;

            [Header("Charles Evidence")]
            public Sprite C_W1;
            public Sprite C_W2;
            public Sprite C_W3;
            public Sprite C_W4;
            public Sprite C_S1;
            public Sprite C_S2;
            public Sprite C_S3;
            public Sprite C_C1;
            public Sprite C_C2;

            [Header("Beatrice Evidence")]
            public Sprite B_W1;
            public Sprite B_W2;
            public Sprite B_W3;
            public Sprite B_W4;
            public Sprite B_S1;
            public Sprite B_S2;
            public Sprite B_S3;
            public Sprite B_C1;
            public Sprite B_C2;

            public Sprite Get(string evidenceId)
            {
                switch (evidenceId)
                {
                    case "A_W1": return A_W1;
                    case "A_W2": return A_W2;
                    case "A_W3": return A_W3;
                    case "A_W4": return A_W4;
                    case "A_S1": return A_S1;
                    case "A_S2": return A_S2;
                    case "A_S3": return A_S3;
                    case "A_C1": return A_C1;
                    case "A_C2": return A_C2;
                    case "C_W1": return C_W1;
                    case "C_W2": return C_W2;
                    case "C_W3": return C_W3;
                    case "C_W4": return C_W4;
                    case "C_S1": return C_S1;
                    case "C_S2": return C_S2;
                    case "C_S3": return C_S3;
                    case "C_C1": return C_C1;
                    case "C_C2": return C_C2;
                    case "B_W1": return B_W1;
                    case "B_W2": return B_W2;
                    case "B_W3": return B_W3;
                    case "B_W4": return B_W4;
                    case "B_S1": return B_S1;
                    case "B_S2": return B_S2;
                    case "B_S3": return B_S3;
                    case "B_C1": return B_C1;
                    case "B_C2": return B_C2;
                    default: return null;
                }
            }
        }

        [Serializable]
        public class CharacterVariantSet
        {
            [Tooltip("角色进入审讯时使用的默认立绘")]
            public Sprite defaultSprite;
            [Tooltip("张力过低，得到无效证词时的差分")]
            public Sprite uselessStatementSprite;
            [Tooltip("得到真实证词时的差分")]
            public Sprite trueStatementSprite;
            [Tooltip("张力过高，得到虚假证词时的差分")]
            public Sprite falseStatementSprite;
            [Tooltip("使用安抚证据或发生缓和事件时的差分")]
            public Sprite calmSprite;
            [Tooltip("使用左轮手枪时的差分")]
            public Sprite revolverSprite;
            [Tooltip("张力超过上限时的崩溃差分")]
            public Sprite breakdownSprite;

            public Sprite GetStatementSprite(TestimonyTruth truth)
            {
                Sprite result = null;
                if (truth == TestimonyTruth.Useless) result = uselessStatementSprite;
                else if (truth == TestimonyTruth.True) result = trueStatementSprite;
                else if (truth == TestimonyTruth.False) result = falseStatementSprite;
                return result != null ? result : defaultSprite;
            }
        }

        [Serializable]
        public class CharacterVariantBindings
        {
            public CharacterVariantSet arthur = new CharacterVariantSet();
            public CharacterVariantSet charles = new CharacterVariantSet();
            public CharacterVariantSet beatrice = new CharacterVariantSet();

            public CharacterVariantSet Get(SuspectId id)
            {
                if (id == SuspectId.Arthur) return arthur;
                if (id == SuspectId.Charles) return charles;
                return beatrice;
            }
        }

        [Header("Optional Art Resources")]
        public Sprite backgroundSprite;
        public Sprite arthurSprite;
        public Sprite charlesSprite;
        public Sprite beatriceSprite;

        [Header("Character Variant Art Resources")]
        public CharacterVariantBindings characterVariants = new CharacterVariantBindings();

        [Header("Evidence Art Resources (27)")]
        public EvidenceSpriteBindings evidenceSprites = new EvidenceSpriteBindings();

        [Header("UI Art Resources (Sprite 2D and UI)")]
        public Sprite dialoguePanelUISprite;
        public Sprite buttonUISprite;
        public Sprite recordPanelUISprite;
        public Sprite resultPanelUISprite;

        [Header("Evidence Tooltip Art Resource")]
        [Tooltip("仅用于鼠标悬停证据时出现的介绍栏，不与背景或对话框共用")]
        public Sprite evidenceTooltipUISprite;

        [Header("Gameplay Art Resources")]
        [Tooltip("Displayed behind the tension number.")]
        public Sprite tensionBarSprite;
        [Tooltip("Used as the visual for both revolver buttons.")]
        public Sprite gunSprite;
        [Tooltip("Displayed like a guilty stamp after a successful accusation.")]
        public Sprite guiltSprite;

        [Header("Evidence Selection Effect")]
        [Tooltip("选中证据原图时使用的颜色。较低的RGB会使图片变暗。")]
        public Color selectedEvidenceTint = new Color(0.58f, 0.58f, 0.58f, 1f);

        [Header("Optional Audio")]
        public AudioClip interrogationMusic;
        public AudioClip clickSound;
        public AudioClip emptyGunSound;
        public AudioClip liveGunSound;

        [Header("Random Event Audio")]
        public AudioClip clockSound;
        public AudioClip thunderSound;
        public AudioClip fileSearchSound;
        public AudioClip tableSlamSound;
        public AudioClip chairScrapeSound;

        [Header("Heartbeat Audio")]
        [Tooltip("Looped while tension is from 0 to 19.")]
        public AudioClip heartbeat0;
        [Tooltip("Looped while tension is from 20 to 69.")]
        public AudioClip heartbeat20;
        [Tooltip("Looped while tension is from 70 to 89.")]
        public AudioClip heartbeat70;
        [Tooltip("Looped while tension is 90 or higher.")]
        public AudioClip heartbeat90;

        [Header("Audio Volume")]
        [Range(0f, 1f)] public float musicVolume = 1f;
        [Range(0f, 1f)] public float soundEffectVolume = 1f;
        [Range(0f, 1f)] public float heartbeatVolume = 1f;

        [Header("Ending Scene Settings")]
        public string successSceneName = "SuccessScene";
        public string failureSceneName = "FailureScene";

        private readonly Color ink = new Color32(236, 228, 211, 255);
        private readonly Color dark = new Color32(31, 24, 24, 245);
        private readonly Color brown = new Color32(91, 58, 42, 255);
        private readonly Color selected = new Color32(128, 82, 48, 255);

        private List<SuspectDefinition> suspects;
        private List<RandomEventDefinition> randomEvents;
        private readonly Dictionary<SuspectId, int> tension = new Dictionary<SuspectId, int>();
        private readonly List<TestimonyRecord> records = new List<TestimonyRecord>();
        private readonly List<EvidenceDefinition> remaining = new List<EvidenceDefinition>();
        private readonly List<EvidenceDefinition> drawn = new List<EvidenceDefinition>();
        private readonly List<EvidenceDefinition> selectedEvidence = new List<EvidenceDefinition>();
        private readonly List<Button> evidenceButtons = new List<Button>();
        private readonly Dictionary<Button, Image> evidenceButtonImages = new Dictionary<Button, Image>();

        private int suspectIndex;
        private int round;
        private bool revolverUsed;
        private AudioSource musicSource;
        private AudioSource soundEffectSource;
        private AudioSource heartbeatSource;
        private TMP_FontAsset uiFont;

        private TMP_Text titleText, tensionText, roundText, dialogueText, hintText, recordText;
        private Image portraitImage, backgroundImage, tensionBarImage, guiltImage;
        private Transform evidenceRoot;
        private ScrollRect dialogueScrollRect;
        private Scrollbar dialogueScrollbar;
        private GameObject evidenceTooltipPanel;
        private TMP_Text evidenceTooltipTitleText;
        private TMP_Text evidenceTooltipText;
        private TMP_Text evidenceTooltipRightText;
        private Button hoveredEvidenceButton;
        private Button confirmButton, emptyGunButton, liveGunButton;
        private GameObject interrogationPanel, recordPanel, resultPanel;
        private TMP_Text resultText;
        private Button accuseA, accuseC, accuseB, continueButton;
        private string pendingSceneName;

        private SuspectDefinition Current => suspects[suspectIndex];

        private void Awake()
        {
            LoadUIFont();
            suspects = NoVerityContent.CreateSuspects();
            randomEvents = NoVerityContent.CreateEvents();
            foreach (var s in suspects) tension[s.id] = s.initialTension;
            musicSource = CreateAudioSource("InterrogationMusicSource", true, musicVolume);
            soundEffectSource = CreateAudioSource("SoundEffectSource", false, soundEffectVolume);
            heartbeatSource = CreateAudioSource("HeartbeatSource", true, heartbeatVolume);
            BuildUI();
        }

        private void LoadUIFont()
        {
            const string resourcePath = "fonts/NotoSansSC-Medium";
            uiFont = Resources.Load<TMP_FontAsset>(resourcePath);

            // If the same Resources path contains a TTF/OTF instead of a TMP asset,
            // create a TMP font asset at runtime as a fallback.
            if (uiFont == null)
            {
                Font sourceFont = Resources.Load<Font>(resourcePath);
                if (sourceFont != null)
                    uiFont = TMP_FontAsset.CreateFontAsset(sourceFont);
            }

            if (uiFont == null)
                Debug.LogError("Font not found: Assets/Resources/fonts/NotoSansSC-Medium.");
        }

        private void Start()
        {
            if (interrogationMusic != null)
            {
                musicSource.clip = interrogationMusic;
                musicSource.Play();
            }
            BeginSuspect(0);

        }

        private void BeginSuspect(int index)
        {
            suspectIndex = index;
            round = 1;
            remaining.Clear();
            remaining.AddRange(Current.evidence);
            titleText.text = Current.displayName + "\n<size=55%>" + Current.label + "</size>";
            SetPortrait(GetDefaultPortrait(Current.id));
            SetDialogueText(Current.opening);
            recordPanel.SetActive(false);
            resultPanel.SetActive(false);
            interrogationPanel.SetActive(true);
            UpdateHeartbeat();
            PrepareRound();
        }

        private void PrepareRound()
        {
            ClearEvidenceButtons();
            selectedEvidence.Clear();
            drawn.Clear();
            var candidates = new List<EvidenceDefinition>(remaining);
            for (int i = 0; i < 3; i++)
            {
                int pick = UnityEngine.Random.Range(0, candidates.Count);
                drawn.Add(candidates[pick]);
                candidates.RemoveAt(pick);
            }
            foreach (var evidence in drawn) CreateEvidenceButton(evidence);
            hintText.text = "Select at least two pieces of evidence. Selection order is presentation order.";
            UpdateHeader();
            UpdateControls();
        }

        private void ToggleEvidence(EvidenceDefinition evidence, Button button)
        {
            Play(clickSound);
            if (selectedEvidence.Contains(evidence))
            {
                selectedEvidence.Remove(evidence);
                SetEvidenceCardVisual(button, false);
            }
            else
            {
                selectedEvidence.Add(evidence);
                SetEvidenceCardVisual(button, true);
            }
            UpdateControls();
        }

        private void ConfirmEvidence()
        {
            if (selectedEvidence.Count < 2) return;
            var log = new List<string>();
            foreach (var evidence in selectedEvidence)
            {
                ApplyEvidence(evidence, log);
                if (CheckBreakdown()) return;
            }

            foreach (var unasked in drawn.Where(x => !selectedEvidence.Contains(x)))
            {
                if (unasked.power == EvidencePower.Calm) continue;
                records.Add(new TestimonyRecord {
                    suspect=Current.id, evidenceId=unasked.id, evidenceTitle=unasked.title,
                    trait=unasked.trait, truth=TestimonyTruth.Unquestioned,
                    tension=tension[Current.id], response="Not questioned", questioned=false
                });
            }
            foreach (var e in drawn) remaining.Remove(e);

            var evt = DrawRandomEvent(Current.id);
            tension[Current.id] = Mathf.Clamp(tension[Current.id] + evt.tensionChange, 0, 999);
            SetPortraitForRandomEvent(evt);
            PlayRandomEventSound(evt);
            UpdateHeartbeat();
            log.Add($"\n[Random Event] {evt.text} (Tension {Signed(evt.tensionChange)})");
            SetDialogueText(string.Join("\n\n", log));
            UpdateHeader();
            if (CheckBreakdown()) return;

            if (round < 3)
            {
                round++;
                PrepareRound();
                SetDialogueText(string.Join("\n\n", log) + "\n\n-- Next Round --");
            }
            else
            {
                FinishSuspect();
            }
        }

        private void ApplyEvidence(EvidenceDefinition e, List<string> log)
        {
            if (e.power == EvidencePower.Calm)
            {
                tension[Current.id] = Mathf.Max(0, tension[Current.id] - 10);
                SetPortrait(GetVariantOrDefault(Current.id,
                    characterVariants.Get(Current.id).calmSprite));
                UpdateHeartbeat();
                log.Add($"[{e.title}]\n{Current.displayName}: {e.calmResponse}\nTension -10");
                return;
            }

            int baseValue = e.power == EvidencePower.Weak ? 15 : 25;
            int change = baseValue + Current.pressureModifier;
            tension[Current.id] += change;
            UpdateHeartbeat();
            var truth = TruthAt(tension[Current.id]);
            SetPortrait(GetVariantOrDefault(Current.id,
                characterVariants.Get(Current.id).GetStatementSprite(truth)));
            string response = truth == TestimonyTruth.Useless ? e.lowResponse
                : truth == TestimonyTruth.True ? e.trueResponse : e.highResponse;
            records.Add(new TestimonyRecord {
                suspect=Current.id, evidenceId=e.id, evidenceTitle=e.title, trait=e.trait,
                truth=truth, tension=tension[Current.id], response=response, questioned=true
            });
            log.Add($"[{e.title}] (Tension {Signed(change)} -> {tension[Current.id]})\n{Current.displayName}: {response}\nStatement: {TruthLabel(truth)}");
        }

        private void UseRevolver(bool live)
        {
            if (revolverUsed) return;
            revolverUsed = true;
            int change = live ? 10 : -10;
            tension[Current.id] = Mathf.Max(0, tension[Current.id] + change);
            SetPortrait(GetVariantOrDefault(Current.id,
                characterVariants.Get(Current.id).revolverSprite));
            Play(live ? liveGunSound : emptyGunSound);
            UpdateHeartbeat();
            SetDialogueText(live
                ? "[Live Round] The gunshot tears through the room.\nDetective: Keep lying, and I may fire again."
                : "[Blank Round] The trigger falls with an empty click.\nDetective: Now you understand my resolve.");
            UpdateHeader();
            UpdateControls();
            CheckBreakdown();
        }

        private void FinishSuspect()
        {
            ShowRecordBook();
            string buttonLabel = suspectIndex < suspects.Count - 1 ? "Question Next Suspect" : "Proceed to Accusation";
            hintText.text = buttonLabel;
            confirmButton.GetComponentInChildren<TMP_Text>().text = buttonLabel;
            confirmButton.interactable = true;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(ConfirmEvidence);
                confirmButton.GetComponentInChildren<TMP_Text>().text = "Present Evidence";
                if (suspectIndex < suspects.Count - 1) BeginSuspect(suspectIndex + 1);
                else ShowFinalAccusation();
            });
        }

        private void ShowRecordBook()
        {
            recordPanel.SetActive(true);
            var lines = records.Where(x => x.suspect == Current.id)
                .Select(x => $"[{TraitLabel(x.trait)}] {x.evidenceTitle} | {TruthLabel(x.truth)} | Tension {x.tension}\n{x.response}");
            recordText.text = "<b>Interrogation Record: " + Current.displayName + "</b>\n\n" + string.Join("\n\n", lines);
            ClearEvidenceButtons();
        }

        private void ShowFinalAccusation()
        {
            interrogationPanel.SetActive(false);
            recordPanel.SetActive(false);
            resultPanel.SetActive(true);
            guiltImage.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
            resultText.text = "<size=130%><b>Final Accusation</b></size>\n\nAn accusation requires a true motive statement, a true method statement, and one questioned case supplement.";
            accuseA.interactable = CanAccuse(SuspectId.Arthur);
            accuseC.interactable = CanAccuse(SuspectId.Charles);
            accuseB.interactable = CanAccuse(SuspectId.Beatrice);
            if (!accuseA.interactable && !accuseC.interactable && !accuseB.interactable)
                EndGame("Cold Case", "The evidence is insufficient and the statements cannot be trusted. You let them leave.", false);
        }

        private bool CanAccuse(SuspectId id)
        {
            var r = records.Where(x => x.suspect == id && x.questioned).ToList();
            bool motive = r.Any(x => x.trait == EvidenceTrait.Motive && x.truth == TestimonyTruth.True);
            bool method = r.Any(x => x.trait == EvidenceTrait.Method && x.truth == TestimonyTruth.True);
            bool supplement = r.Any(x => x.trait == EvidenceTrait.Supplement);
            return motive && method && supplement;
        }

        private void Accuse(SuspectId id)
        {
            if (id == SuspectId.Arthur)
                EndGame("The Assailant", "Arthur pushed Clara and caused severe bleeding, but he did not deliver the fatal blow.", true);
            else if (id == SuspectId.Charles)
                EndGame("The Poisoner", "Charles admits to drugging Clara, but the dose was not fatal. The truth remains incomplete.", true);
            else
                EndGame("The Gravedigger", "Beatrice admits to moving and burying Clara, never knowing that Clara was still alive.", true);
        }

        private void EndGame(string ending, string body, bool success)
        {
            interrogationPanel.SetActive(false);
            recordPanel.SetActive(false);
            resultPanel.SetActive(true);
            resultText.text = $"<size=150%><b>{ending}</b></size>\n\n{body}\n\nThe rain was just as heavy that night. Clara crawled out of the pit and begged you for help.\nYou did not save her. You struck the final blow and planted evidence against the other three.\n\n<b>No Verity -- there is no truth, because you constructed it from the beginning.</b>";
            SetAccusationButtons(false);
            guiltImage.gameObject.SetActive(success && guiltSprite != null);
            pendingSceneName = success ? successSceneName : failureSceneName;
            continueButton.gameObject.SetActive(true);
        }

        private bool CheckBreakdown()
        {
            if (tension[Current.id] <= 90) return false;
            SetPortrait(GetVariantOrDefault(Current.id,
                characterVariants.Get(Current.id).breakdownSprite));
            string text = Current.id == SuspectId.Arthur ? "Arthur seizes the revolver: You think you can judge me?"
                : Current.id == SuspectId.Charles ? "Charles loses all composure and lunges at you."
                : "Beatrice erupts in terror: Stop pushing me!";
            EndGame("Breakdown", "Your interrogation went too far. " + text + "\nThe files scatter across the floor. The truth is buried forever.", false);
            return true;
        }

        private TestimonyTruth TruthAt(int value)
        {
            if (value <= 20) return TestimonyTruth.Useless;
            if (value <= 70) return TestimonyTruth.True;
            return TestimonyTruth.False;
        }

        private RandomEventDefinition DrawRandomEvent(SuspectId id)
        {
            int roll = UnityEngine.Random.Range(0, 100);
            int sign = roll < 30 ? -1 : roll < 70 ? 0 : 1;
            var pool = randomEvents.Where(x => (!x.suspect.HasValue || x.suspect.Value == id)
                && Math.Sign(x.tensionChange) == sign).ToList();
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }

        private void UpdateHeader()
        {
            tensionText.text = $"Tension  {tension[Current.id]}/100";
            roundText.text = $"Round {round}/3";
        }

        private void SetDialogueText(string text)
        {
            if (dialogueText == null) return;

            dialogueText.text = text ?? string.Empty;
            dialogueText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueText.rectTransform);

            // 每次出现新的描写或突发事件时从文本顶部开始查看。
            if (dialogueScrollRect != null)
                dialogueScrollRect.verticalNormalizedPosition = 1f;
            if (dialogueScrollbar != null)
                dialogueScrollbar.value = 1f;
        }

        private void UpdateControls()
        {
            confirmButton.interactable = selectedEvidence.Count >= 2;
            emptyGunButton.interactable = !revolverUsed;
            liveGunButton.interactable = !revolverUsed;
        }

        private void ClearEvidenceButtons()
        {
            HideEvidenceTooltip();
            foreach (var b in evidenceButtons) if (b != null) Destroy(b.gameObject);
            evidenceButtons.Clear();
            evidenceButtonImages.Clear();
        }

        private void CreateEvidenceButton(EvidenceDefinition evidence)
        {
            Sprite evidenceSprite = GetEvidenceSprite(evidence);
            var buttonObject = new GameObject("Evidence_" + evidence.id,
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(evidenceRoot, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 220);

            LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = 250;
            layoutElement.preferredHeight = 220;

            // 证据原图本身就是按钮，不显示统一底图和下方文字。
            Image evidenceImage = buttonObject.GetComponent<Image>();
            evidenceImage.sprite = evidenceSprite;
            evidenceImage.preserveAspect = true;
            evidenceImage.type = Image.Type.Simple;
            evidenceImage.color = Color.white;
            evidenceImage.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = evidenceImage;
            button.transition = Selectable.Transition.None;
            evidenceButtonImages[button] = evidenceImage;
            SetEvidenceCardVisual(button, false);
            button.onClick.AddListener(() => ToggleEvidence(evidence, button));
            AddEvidenceHoverEvents(button, evidence);
            evidenceButtons.Add(button);
        }

        private Sprite GetEvidenceSprite(EvidenceDefinition evidence)
        {
            if (evidence == null || evidenceSprites == null) return null;
            return evidenceSprites.Get(evidence.id);
        }

        private void SetEvidenceCardVisual(Button button, bool isSelected)
        {
            if (button == null) return;
            if (!evidenceButtonImages.TryGetValue(button, out Image evidenceImage) || evidenceImage == null)
                return;

            // 不替换统一卡片UI，只在证据原图上应用选中颜色，注释保持清晰。
            evidenceImage.color = isSelected ? selectedEvidenceTint : Color.white;
        }

        private void AddEvidenceHoverEvents(Button button, EvidenceDefinition evidence)
        {
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            enterEntry.callback.AddListener(_ => ShowEvidenceTooltip(button, evidence));
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            exitEntry.callback.AddListener(_ =>
                StartCoroutine(HideEvidenceTooltipWhenPointerLeaves(button)));
            trigger.triggers.Add(exitEntry);
        }

        private void ShowEvidenceTooltip(Button button, EvidenceDefinition evidence)
        {
            if (evidenceTooltipPanel == null || evidenceTooltipText == null) return;

            hoveredEvidenceButton = button;
            string description = string.IsNullOrWhiteSpace(evidence.description)
                ? evidence.title : evidence.description;

            if (evidenceTooltipTitleText != null)
                evidenceTooltipTitleText.text = evidence.title;
            SplitDescriptionForPages(description, out string leftDescription, out string rightDescription);
            evidenceTooltipText.text = leftDescription;
            if (evidenceTooltipRightText != null)
                evidenceTooltipRightText.text = rightDescription;
            evidenceTooltipPanel.SetActive(true);
            evidenceTooltipPanel.transform.SetAsLastSibling();
        }

        private IEnumerator HideEvidenceTooltipWhenPointerLeaves(Button sourceButton)
        {
            // 给鼠标留出从证据图移动到介绍框的时间。
            yield return new WaitForSecondsRealtime(.1f);

            bool overEvidence = sourceButton != null &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    sourceButton.GetComponent<RectTransform>(), Input.mousePosition, null);
            bool overTooltip = evidenceTooltipPanel != null && evidenceTooltipPanel.activeSelf &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    evidenceTooltipPanel.GetComponent<RectTransform>(), Input.mousePosition, null);

            if (!overEvidence && !overTooltip && hoveredEvidenceButton == sourceButton)
                HideEvidenceTooltip();
        }

        private void HideEvidenceTooltip()
        {
            hoveredEvidenceButton = null;
            if (evidenceTooltipPanel != null)
                evidenceTooltipPanel.SetActive(false);
        }

        private void SplitDescriptionForPages(string description, out string leftPage, out string rightPage)
        {
            leftPage = description ?? string.Empty;
            rightPage = string.Empty;
            if (string.IsNullOrWhiteSpace(description)) return;

            string[] words = description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2) return;

            // 左页还要容纳标题，因此放约45%的正文，其余内容放在右页。
            int targetLength = Mathf.RoundToInt(description.Length * .45f);
            int currentLength = 0;
            int splitIndex = 1;
            for (int i = 0; i < words.Length; i++)
            {
                currentLength += words[i].Length + 1;
                if (currentLength >= targetLength)
                {
                    splitIndex = Mathf.Clamp(i + 1, 1, words.Length - 1);
                    break;
                }
            }

            leftPage = string.Join(" ", words.Take(splitIndex));
            rightPage = string.Join(" ", words.Skip(splitIndex));
        }

        private void BuildUI()
        {
            var canvasGO = new GameObject("GameSceneCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

            backgroundImage = MakeImage(canvasGO.transform, "Background", Vector2.zero, Vector2.one);
            backgroundImage.sprite = backgroundSprite; backgroundImage.color = backgroundSprite ? Color.white : new Color32(25,20,20,255);
            interrogationPanel = MakePanel(canvasGO.transform, "InterrogationPanel", Vector2.zero, Vector2.one, Color.clear);
            titleText = MakeText(interrogationPanel.transform,"SuspectTitle",new Vector2(.03f,.88f),new Vector2(.37f,.98f),30,TextAlignmentOptions.Left);
            // 紧张条放在左侧人物立绘正上方。
            tensionBarImage = MakeImage(interrogationPanel.transform,"TensionBar",new Vector2(.03f,.80f),new Vector2(.37f,.87f));
            tensionBarImage.sprite = tensionBarSprite;
            tensionBarImage.color = tensionBarSprite != null ? Color.white : Color.clear;
            tensionBarImage.type = Image.Type.Simple;
            tensionBarImage.raycastTarget = false;
            tensionText = MakeText(interrogationPanel.transform,"Tension",new Vector2(.05f,.805f),new Vector2(.35f,.865f),26,TextAlignmentOptions.Right);
            roundText = MakeText(interrogationPanel.transform,"Round",new Vector2(.03f,.755f),new Vector2(.37f,.80f),20,TextAlignmentOptions.Right);
            portraitImage = MakeImage(interrogationPanel.transform,"Portrait",new Vector2(-0.04f,.03f),new Vector2(.38f,.755f)); portraitImage.preserveAspect=true;
            // 适配“通用背景.png”的约1.69:1宽高比。
            var dialoguePanel = MakePanel(interrogationPanel.transform,"DialoguePanel",new Vector2(.38f,.29f),new Vector2(.98f,.922f),new Color32(20,16,16,210));
            ApplyUISprite(dialoguePanel.GetComponent<Image>(), dialoguePanelUISprite, new Color32(20,16,16,210));
            // 上方描写与突发事件共用可滚动的对话区域。
            dialogueScrollRect = dialoguePanel.AddComponent<ScrollRect>();
            dialogueScrollRect.horizontal = false;
            dialogueScrollRect.vertical = true;
            dialogueScrollRect.movementType = ScrollRect.MovementType.Clamped;
            dialogueScrollRect.scrollSensitivity = 24f;

            // 独立安全视口确保正文在任何滚动位置都不会进入四周装饰条纹。
            GameObject dialogueViewportObject = new GameObject("DialogueViewport",
                typeof(RectTransform), typeof(RectMask2D));
            dialogueViewportObject.transform.SetParent(dialoguePanel.transform, false);
            RectTransform dialogueViewport = dialogueViewportObject.GetComponent<RectTransform>();
            Anchor(dialogueViewport, new Vector2(.15f,.15f), new Vector2(.83f,.85f));
            dialogueScrollRect.viewport = dialogueViewport;

            dialogueText = MakeText(dialogueViewport,"Dialogue",new Vector2(0f,1f),new Vector2(1f,1f),21,TextAlignmentOptions.TopLeft);
            RectTransform dialogueContent = dialogueText.rectTransform;
            dialogueContent.pivot = new Vector2(.5f, 1f);
            dialogueContent.anchoredPosition = Vector2.zero;
            dialogueContent.sizeDelta = new Vector2(0f, 0f);
            dialogueText.color = dialoguePanelUISprite != null ? dark : ink;
            dialogueText.enableAutoSizing = false;
            dialogueText.overflowMode = TextOverflowModes.Overflow;
            ContentSizeFitter dialogueFitter = dialogueText.gameObject.AddComponent<ContentSizeFitter>();
            dialogueFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            dialogueFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            dialogueScrollRect.content = dialogueContent;

            GameObject scrollbarObject = MakePanel(dialoguePanel.transform,"DialogueScrollbar",
                new Vector2(.85f,.15f),new Vector2(.88f,.85f),new Color32(45,35,31,100));
            dialogueScrollbar = scrollbarObject.AddComponent<Scrollbar>();
            dialogueScrollbar.direction = Scrollbar.Direction.BottomToTop;

            GameObject slidingArea = new GameObject("SlidingArea", typeof(RectTransform));
            slidingArea.transform.SetParent(scrollbarObject.transform, false);
            Anchor(slidingArea.GetComponent<RectTransform>(), new Vector2(.15f,.02f), new Vector2(.85f,.98f));

            Image scrollbarHandle = MakeImage(slidingArea.transform,"Handle",Vector2.zero,Vector2.one);
            scrollbarHandle.color = dialoguePanelUISprite != null ? brown : ink;
            dialogueScrollbar.handleRect = scrollbarHandle.rectTransform;
            dialogueScrollbar.targetGraphic = scrollbarHandle;
            dialogueScrollbar.size = .35f;

            dialogueScrollRect.verticalScrollbar = dialogueScrollbar;
            dialogueScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            evidenceRoot = MakePanel(interrogationPanel.transform,"EvidenceCards",new Vector2(.38f,.075f),new Vector2(.98f,.27f),Color.clear).transform;
            var layout=evidenceRoot.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing=20; layout.childAlignment=TextAnchor.MiddleCenter; layout.childForceExpandWidth=true; layout.childForceExpandHeight=true;

            // 适配“笔记本.png”的约1.59:1宽高比，左页标题、右页介绍。
            evidenceTooltipPanel = MakePanel(interrogationPanel.transform,"EvidenceTooltip",
                new Vector2(.34f,.22f),new Vector2(.76f,.69f),new Color32(24,19,18,248));
            ApplyUISprite(evidenceTooltipPanel.GetComponent<Image>(), evidenceTooltipUISprite,
                new Color32(24,19,18,248));

            evidenceTooltipTitleText = MakeText(evidenceTooltipPanel.transform,"EvidenceTooltipTitle",
                new Vector2(.07f,.67f),new Vector2(.44f,.85f),28,TextAlignmentOptions.Center);
            evidenceTooltipTitleText.enableAutoSizing = true;
            evidenceTooltipTitleText.fontSizeMin = 21;
            evidenceTooltipTitleText.fontSizeMax = 28;
            evidenceTooltipTitleText.fontStyle = FontStyles.Bold;
            evidenceTooltipTitleText.color = evidenceTooltipUISprite != null ? brown : ink;
            evidenceTooltipTitleText.raycastTarget = false;

            evidenceTooltipText = MakeText(evidenceTooltipPanel.transform,"EvidenceTooltipText",
                new Vector2(.08f,.15f),new Vector2(.44f,.64f),22,TextAlignmentOptions.TopLeft);
            evidenceTooltipText.enableAutoSizing = false;
            evidenceTooltipText.fontSize = 22;
            evidenceTooltipText.color = evidenceTooltipUISprite != null ? dark : ink;
            evidenceTooltipText.raycastTarget = false;

            evidenceTooltipRightText = MakeText(evidenceTooltipPanel.transform,"EvidenceTooltipRightText",
                new Vector2(.53f,.15f),new Vector2(.87f,.85f),22,TextAlignmentOptions.TopLeft);
            evidenceTooltipRightText.enableAutoSizing = false;
            evidenceTooltipRightText.fontSize = 22;
            evidenceTooltipRightText.color = evidenceTooltipUISprite != null ? dark : ink;
            evidenceTooltipRightText.raycastTarget = false;

            EventTrigger tooltipTrigger = evidenceTooltipPanel.AddComponent<EventTrigger>();
            EventTrigger.Entry tooltipExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            tooltipExit.callback.AddListener(_ => {
                if (hoveredEvidenceButton != null)
                    StartCoroutine(HideEvidenceTooltipWhenPointerLeaves(hoveredEvidenceButton));
            });
            tooltipTrigger.triggers.Add(tooltipExit);
            evidenceTooltipPanel.SetActive(false);

            hintText = MakeText(interrogationPanel.transform,"Hint",new Vector2(.38f,.27f),new Vector2(.98f,.29f),14,TextAlignmentOptions.Center);
            confirmButton=MakeButton(interrogationPanel.transform,"Present Evidence",new Vector2(260,60)); Anchor(confirmButton.GetComponent<RectTransform>(),new Vector2(.84f,.01f),new Vector2(.98f,.065f)); confirmButton.onClick.AddListener(ConfirmEvidence);
            emptyGunButton=MakeButton(interrogationPanel.transform,"Revolver: Blank (-10)",new Vector2(220,55)); Anchor(emptyGunButton.GetComponent<RectTransform>(),new Vector2(.38f,.01f),new Vector2(.51f,.065f)); emptyGunButton.onClick.AddListener(()=>UseRevolver(false));
            liveGunButton=MakeButton(interrogationPanel.transform,"Revolver: Live (+10)",new Vector2(220,55)); Anchor(liveGunButton.GetComponent<RectTransform>(),new Vector2(.53f,.01f),new Vector2(.66f,.065f)); liveGunButton.onClick.AddListener(()=>UseRevolver(true));
            ApplyGunSprite(emptyGunButton);
            ApplyGunSprite(liveGunButton);

            recordPanel=MakePanel(canvasGO.transform,"RecordPanel",new Vector2(.03f,.05f),new Vector2(.34f,.78f),new Color32(35,28,25,245));
            ApplyUISprite(recordPanel.GetComponent<Image>(), recordPanelUISprite, new Color32(35,28,25,245));
            var scroll=recordPanel.AddComponent<ScrollRect>();
            recordText=MakeText(recordPanel.transform,"RecordText",new Vector2(.04f,.03f),new Vector2(.96f,.97f),17,TextAlignmentOptions.TopLeft); scroll.content=recordText.rectTransform; scroll.vertical=true;

            resultPanel=MakePanel(canvasGO.transform,"ResultPanel",new Vector2(.2f,.14f),new Vector2(.8f,.86f),dark);
            ApplyUISprite(resultPanel.GetComponent<Image>(), resultPanelUISprite, dark);
            resultText=MakeText(resultPanel.transform,"ResultText",new Vector2(.08f,.34f),new Vector2(.92f,.92f),26,TextAlignmentOptions.TopLeft);
            guiltImage=MakeImage(resultPanel.transform,"GuiltStamp",new Vector2(.58f,.48f),new Vector2(.90f,.82f));
            guiltImage.sprite=guiltSprite;
            guiltImage.color=guiltSprite!=null?Color.white:Color.clear;
            guiltImage.type=Image.Type.Simple;
            guiltImage.preserveAspect=true;
            guiltImage.raycastTarget=false;
            guiltImage.gameObject.SetActive(false);
            accuseA=MakeButton(resultPanel.transform,"Accuse Arthur",new Vector2(220,60)); Anchor(accuseA.GetComponent<RectTransform>(),new Vector2(.08f,.16f),new Vector2(.31f,.27f)); accuseA.onClick.AddListener(()=>Accuse(SuspectId.Arthur));
            accuseC=MakeButton(resultPanel.transform,"Accuse Charles",new Vector2(220,60)); Anchor(accuseC.GetComponent<RectTransform>(),new Vector2(.385f,.16f),new Vector2(.615f,.27f)); accuseC.onClick.AddListener(()=>Accuse(SuspectId.Charles));
            accuseB=MakeButton(resultPanel.transform,"Accuse Beatrice",new Vector2(220,60)); Anchor(accuseB.GetComponent<RectTransform>(),new Vector2(.69f,.16f),new Vector2(.92f,.27f)); accuseB.onClick.AddListener(()=>Accuse(SuspectId.Beatrice));
            continueButton=MakeButton(resultPanel.transform,"Continue",new Vector2(180,52)); Anchor(continueButton.GetComponent<RectTransform>(),new Vector2(.39f,.04f),new Vector2(.61f,.12f)); continueButton.onClick.AddListener(LoadEndingScene); continueButton.gameObject.SetActive(false);
        }

        private GameObject MakePanel(Transform parent,string name,Vector2 min,Vector2 max,Color color)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false); Anchor(go.GetComponent<RectTransform>(),min,max); go.GetComponent<Image>().color=color; return go; }
        private Image MakeImage(Transform parent,string name,Vector2 min,Vector2 max)
        { return MakePanel(parent,name,min,max,Color.white).GetComponent<Image>(); }
        private TMP_Text MakeText(Transform parent,string name,Vector2 min,Vector2 max,float size,TextAlignmentOptions align)
        { var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); go.transform.SetParent(parent,false); Anchor(go.GetComponent<RectTransform>(),min,max); var t=go.GetComponent<TMP_Text>(); if(uiFont!=null)t.font=uiFont; t.fontSize=size; t.color=ink; t.alignment=align; t.enableWordWrapping=true; return t; }
        private Button MakeButton(Transform parent,string text,Vector2 size)
        { var go=new GameObject("Button",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement)); go.transform.SetParent(parent,false); ApplyUISprite(go.GetComponent<Image>(),buttonUISprite,brown); go.GetComponent<RectTransform>().sizeDelta=size; var le=go.GetComponent<LayoutElement>(); le.preferredWidth=size.x; le.preferredHeight=size.y; var label=MakeText(go.transform,"Label",new Vector2(.06f,.08f),new Vector2(.94f,.92f),19,TextAlignmentOptions.Center); label.text=text; label.raycastTarget=false; return go.GetComponent<Button>(); }
        private void ApplyUISprite(Image image,Sprite sprite,Color fallback)
        { image.sprite=sprite; image.type=sprite!=null?Image.Type.Sliced:Image.Type.Simple; image.color=sprite!=null?Color.white:fallback; }
        private void ApplyGunSprite(Button button)
        {
            if(gunSprite==null) return;
            button.image.sprite=gunSprite;
            button.image.type=Image.Type.Simple;
            button.image.preserveAspect=true;
            button.image.color=Color.white;
        }
        private void Anchor(RectTransform r,Vector2 min,Vector2 max)
        { r.anchorMin=min; r.anchorMax=max; r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero; }
        private Sprite GetLegacyPortrait(SuspectId id)
            => id==SuspectId.Arthur ? arthurSprite
            : id==SuspectId.Charles ? charlesSprite : beatriceSprite;

        private Sprite GetDefaultPortrait(SuspectId id)
        {
            CharacterVariantSet variants = characterVariants != null
                ? characterVariants.Get(id) : null;
            if (variants != null && variants.defaultSprite != null)
                return variants.defaultSprite;
            return GetLegacyPortrait(id);
        }

        private Sprite GetVariantOrDefault(SuspectId id, Sprite variant)
            => variant != null ? variant : GetDefaultPortrait(id);

        private void SetPortrait(Sprite sprite)
        {
            if (portraitImage == null) return;
            portraitImage.sprite = sprite;
            portraitImage.enabled = sprite != null;
        }

        private void SetPortraitForRandomEvent(RandomEventDefinition evt)
        {
            if (evt == null || characterVariants == null)
            {
                SetPortrait(GetDefaultPortrait(Current.id));
                return;
            }

            CharacterVariantSet variants = characterVariants.Get(Current.id);
            Sprite target = variants.defaultSprite;
            if (evt.tensionChange < 0) target = variants.calmSprite;
            else if (evt.tensionChange > 0) target = variants.falseStatementSprite;
            SetPortrait(GetVariantOrDefault(Current.id, target));
        }
        private AudioSource CreateAudioSource(string sourceName, bool loop, float volume)
        {
            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = volume;
            return source;
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                soundEffectSource.volume = soundEffectVolume;
                soundEffectSource.PlayOneShot(clip);
            }
        }

        private void PlayRandomEventSound(RandomEventDefinition evt)
        {
            AudioClip clip = null;
            switch (evt.id)
            {
                case "NEUT_01":
                    clip = clockSound;
                    break;
                case "NEUT_02":
                case "BAD_01":
                    clip = thunderSound;
                    break;
                case "NEUT_03":
                    clip = fileSearchSound;
                    break;
                case "BAD_02":
                    clip = tableSlamSound;
                    break;
                case "BAD_B":
                    clip = chairScrapeSound;
                    break;
            }

            Play(clip);
        }

        private void UpdateHeartbeat()
        {
            int value = tension[Current.id];
            AudioClip targetClip;

            if (value >= 90)
                targetClip = heartbeat90;
            else if (value >= 70)
                targetClip = heartbeat70;
            else if (value >= 20)
                targetClip = heartbeat20;
            else
                targetClip = heartbeat0;

            heartbeatSource.volume = heartbeatVolume;

            if (targetClip == null)
            {
                heartbeatSource.Stop();
                heartbeatSource.clip = null;
                return;
            }

            if (heartbeatSource.clip == targetClip)
            {
                if (!heartbeatSource.isPlaying)
                    heartbeatSource.Play();
                return;
            }

            heartbeatSource.Stop();
            heartbeatSource.clip = targetClip;
            heartbeatSource.Play();
        }
        private void SetAccusationButtons(bool active) { accuseA.gameObject.SetActive(active); accuseC.gameObject.SetActive(active); accuseB.gameObject.SetActive(active); }
        private void LoadEndingScene() { if(!string.IsNullOrWhiteSpace(pendingSceneName)) SceneManager.LoadScene(pendingSceneName); }
        private string Signed(int n) => n>0?"+"+n:n.ToString();
        private string PowerLabel(EvidencePower p) => p==EvidencePower.Weak?"Weak":p==EvidencePower.Strong?"Strong":"Calming";
        private string TraitLabel(EvidenceTrait t) => t==EvidenceTrait.Motive?"Motive":t==EvidenceTrait.Method?"Method":t==EvidenceTrait.Supplement?"Supplement":"Relief";
        private string TruthLabel(TestimonyTruth t) => t==TestimonyTruth.True?"True Statement":t==TestimonyTruth.False?"False Statement":t==TestimonyTruth.Useless?"Useless Statement":t==TestimonyTruth.Unquestioned?"Not Questioned":"None";
    }
}
