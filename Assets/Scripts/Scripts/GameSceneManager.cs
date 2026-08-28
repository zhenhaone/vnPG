using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NoVerity.GameScene
{
    public class GameSceneManager : MonoBehaviour
    {
        [Header("Optional Art Resources")]
        public Sprite backgroundSprite;
        public Sprite arthurSprite;
        public Sprite charlesSprite;
        public Sprite beatriceSprite;
        public List<Sprite> evidenceSprites = new List<Sprite>(); // Arthur 9, Charles 9, Beatrice 9

        [Header("UI Art Resources (Sprite 2D and UI)")]
        public Sprite dialoguePanelUISprite;
        public Sprite evidenceCardUISprite;
        public Sprite selectedEvidenceCardUISprite;
        public Sprite buttonUISprite;
        public Sprite recordPanelUISprite;
        public Sprite resultPanelUISprite;

        [Header("Gameplay Art Resources")]
        [Tooltip("Displayed behind the tension number.")]
        public Sprite tensionBarSprite;
        [Tooltip("Used as the visual for both revolver buttons.")]
        public Sprite gunSprite;
        [Tooltip("Displayed like a guilty stamp after a successful accusation.")]
        public Sprite guiltSprite;

        [Header("Evidence Selection Effect")]
        [Tooltip("Tint applied to a selected evidence card. Lower RGB values produce a darker card.")]
        public Color selectedEvidenceTint = new Color(0.58f, 0.58f, 0.58f, 1f);

        [Header("Optional Audio")]
        public AudioClip interrogationMusic;
        public AudioClip clickSound;
        public AudioClip emptyGunSound;
        public AudioClip liveGunSound;

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

        private int suspectIndex;
        private int round;
        private bool revolverUsed;
        private AudioSource audioSource;
        private TMP_FontAsset uiFont;

        private TMP_Text titleText, tensionText, roundText, dialogueText, hintText, recordText;
        private Image portraitImage, backgroundImage, tensionBarImage, guiltImage;
        private Transform evidenceRoot;
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
            audioSource = gameObject.AddComponent<AudioSource>();
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
                audioSource.clip = interrogationMusic;
                audioSource.loop = true;
                audioSource.Play();
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
            portraitImage.sprite = GetPortrait(Current.id);
            portraitImage.enabled = portraitImage.sprite != null;
            dialogueText.text = Current.opening;
            recordPanel.SetActive(false);
            resultPanel.SetActive(false);
            interrogationPanel.SetActive(true);
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
            log.Add($"\n[Random Event] {evt.text} (Tension {Signed(evt.tensionChange)})");
            dialogueText.text = string.Join("\n\n", log);
            UpdateHeader();
            if (CheckBreakdown()) return;

            if (round < 3)
            {
                round++;
                PrepareRound();
                dialogueText.text = string.Join("\n\n", log) + "\n\n-- Next Round --";
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
                log.Add($"[{e.title}]\n{Current.displayName}: {e.calmResponse}\nTension -10");
                return;
            }

            int baseValue = e.power == EvidencePower.Weak ? 15 : 25;
            int change = baseValue + Current.pressureModifier;
            tension[Current.id] += change;
            var truth = TruthAt(tension[Current.id]);
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
            Play(live ? liveGunSound : emptyGunSound);
            dialogueText.text = live
                ? "[Live Round] The gunshot tears through the room.\nDetective: Keep lying, and I may fire again."
                : "[Blank Round] The trigger falls with an empty click.\nDetective: Now you understand my resolve.";
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

        private void UpdateControls()
        {
            confirmButton.interactable = selectedEvidence.Count >= 2;
            emptyGunButton.interactable = !revolverUsed;
            liveGunButton.interactable = !revolverUsed;
        }

        private void ClearEvidenceButtons()
        {
            foreach (var b in evidenceButtons) if (b != null) Destroy(b.gameObject);
            evidenceButtons.Clear();
        }

        private void CreateEvidenceButton(EvidenceDefinition evidence)
        {
            var button = MakeButton(evidenceRoot, evidence.title + "\n<size=65%>" + PowerLabel(evidence.power) + "／" + TraitLabel(evidence.trait) + "</size>", new Vector2(250, 220));
            SetEvidenceCardVisual(button, false);

            Sprite evidenceSprite = GetEvidenceSprite(evidence);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (evidenceSprite != null)
            {
                Image evidenceImage = MakeImage(button.transform, "EvidenceImage", new Vector2(.05f,.38f), new Vector2(.95f,.95f));
                evidenceImage.sprite = evidenceSprite;
                evidenceImage.preserveAspect = true;
                evidenceImage.raycastTarget = false;
                Anchor(label.rectTransform, new Vector2(.05f,.03f), new Vector2(.95f,.36f));
                label.fontSize = 16;
            }
            button.onClick.AddListener(() => ToggleEvidence(evidence, button));
            evidenceButtons.Add(button);
        }

        private Sprite GetEvidenceSprite(EvidenceDefinition evidence)
        {
            int offset = Current.id == SuspectId.Arthur ? 0 : Current.id == SuspectId.Charles ? 9 : 18;
            int localIndex = Current.evidence.FindIndex(item => item.id == evidence.id);
            int spriteIndex = offset + localIndex;
            if (localIndex < 0 || spriteIndex < 0 || spriteIndex >= evidenceSprites.Count) return null;
            return evidenceSprites[spriteIndex];
        }

        private void SetEvidenceCardVisual(Button button, bool isSelected)
        {
            Sprite sprite = isSelected && selectedEvidenceCardUISprite != null
                ? selectedEvidenceCardUISprite : evidenceCardUISprite;
            ApplyUISprite(button.image, sprite, isSelected ? selected : brown);

            // Darken the selected card even when a separate selected sprite is assigned.
            if (sprite != null)
                button.image.color = isSelected ? selectedEvidenceTint : Color.white;
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
            titleText = MakeText(interrogationPanel.transform,"SuspectTitle",new Vector2(.03f,.82f),new Vector2(.35f,.97f),30,TextAlignmentOptions.Left);
            tensionBarImage = MakeImage(interrogationPanel.transform,"TensionBar",new Vector2(.67f,.87f),new Vector2(.96f,.97f));
            tensionBarImage.sprite = tensionBarSprite;
            tensionBarImage.color = tensionBarSprite != null ? Color.white : Color.clear;
            tensionBarImage.type = Image.Type.Simple;
            tensionBarImage.raycastTarget = false;
            tensionText = MakeText(interrogationPanel.transform,"Tension",new Vector2(.68f,.88f),new Vector2(.95f,.96f),28,TextAlignmentOptions.Right);
            roundText = MakeText(interrogationPanel.transform,"Round",new Vector2(.68f,.82f),new Vector2(.95f,.88f),22,TextAlignmentOptions.Right);
            portraitImage = MakeImage(interrogationPanel.transform,"Portrait",new Vector2(-0.08f,0.03f),new Vector2(0.43f,0.92f)); portraitImage.preserveAspect=true;
            var dialoguePanel = MakePanel(interrogationPanel.transform,"DialoguePanel",new Vector2(.37f,.50f),new Vector2(.96f,.81f),new Color32(20,16,16,210));
            ApplyUISprite(dialoguePanel.GetComponent<Image>(), dialoguePanelUISprite, new Color32(20,16,16,210));
            dialogueText = MakeText(dialoguePanel.transform,"Dialogue",new Vector2(.03f,.06f),new Vector2(.97f,.94f),24,TextAlignmentOptions.TopLeft);
            evidenceRoot = MakePanel(interrogationPanel.transform,"EvidenceCards",new Vector2(.36f,.22f),new Vector2(.96f,.48f),Color.clear).transform;
            var layout=evidenceRoot.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing=20; layout.childAlignment=TextAnchor.MiddleCenter; layout.childForceExpandWidth=true; layout.childForceExpandHeight=true;
            hintText = MakeText(interrogationPanel.transform,"Hint",new Vector2(.37f,.14f),new Vector2(.96f,.21f),18,TextAlignmentOptions.Center);
            confirmButton=MakeButton(interrogationPanel.transform,"Present Evidence",new Vector2(260,60)); Anchor(confirmButton.GetComponent<RectTransform>(),new Vector2(.78f,.04f),new Vector2(.94f,.12f)); confirmButton.onClick.AddListener(ConfirmEvidence);
            emptyGunButton=MakeButton(interrogationPanel.transform,"Revolver: Blank (-10)",new Vector2(220,55)); Anchor(emptyGunButton.GetComponent<RectTransform>(),new Vector2(.37f,.04f),new Vector2(.50f,.12f)); emptyGunButton.onClick.AddListener(()=>UseRevolver(false));
            liveGunButton=MakeButton(interrogationPanel.transform,"Revolver: Live (+10)",new Vector2(220,55)); Anchor(liveGunButton.GetComponent<RectTransform>(),new Vector2(.52f,.04f),new Vector2(.65f,.12f)); liveGunButton.onClick.AddListener(()=>UseRevolver(true));
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
        private Sprite GetPortrait(SuspectId id) => id==SuspectId.Arthur?arthurSprite:id==SuspectId.Charles?charlesSprite:beatriceSprite;
        private void Play(AudioClip clip) { if(clip!=null) audioSource.PlayOneShot(clip); }
        private void SetAccusationButtons(bool active) { accuseA.gameObject.SetActive(active); accuseC.gameObject.SetActive(active); accuseB.gameObject.SetActive(active); }
        private void LoadEndingScene() { if(!string.IsNullOrWhiteSpace(pendingSceneName)) SceneManager.LoadScene(pendingSceneName); }
        private string Signed(int n) => n>0?"+"+n:n.ToString();
        private string PowerLabel(EvidencePower p) => p==EvidencePower.Weak?"Weak":p==EvidencePower.Strong?"Strong":"Calming";
        private string TraitLabel(EvidenceTrait t) => t==EvidenceTrait.Motive?"Motive":t==EvidenceTrait.Method?"Method":t==EvidenceTrait.Supplement?"Supplement":"Relief";
        private string TruthLabel(TestimonyTruth t) => t==TestimonyTruth.True?"True Statement":t==TestimonyTruth.False?"False Statement":t==TestimonyTruth.Useless?"Useless Statement":t==TestimonyTruth.Unquestioned?"Not Questioned":"None";
    }
}
