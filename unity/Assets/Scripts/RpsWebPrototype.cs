using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RpsKnights
{
    public sealed class RpsWebPrototype : MonoBehaviour
    {
        private readonly Color background = new(0.035f, 0.055f, 0.10f);
        private readonly Color panel = new(0.075f, 0.11f, 0.19f);
        private Font font;
        private Font handFont;
        private Text title;
        private Text chant;
        private Text playerHand;
        private Text opponentHand;
        private Text result;
        private GameObject choicePanel;
        private GameObject selectionRoot;
        private Button[] buttons;
        private bool revealing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartPrototype()
        {
            if (FindAnyObjectByType<RpsWebPrototype>() == null)
                new GameObject("RPS Web Prototype").AddComponent<RpsWebPrototype>();
        }

        private void Awake()
        {
            font = Resources.Load<Font>("Fonts/NotoSansKR");
            handFont = Resources.Load<Font>("Fonts/NotoEmoji");
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (handFont == null) handFont = font;
            BuildUi();
        }

        private void Update()
        {
            if (revealing) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) Choose(Sign.Scissors);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Choose(Sign.Rock);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Choose(Sign.Paper);
        }

        private void BuildUi()
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            Image root = Image("Background", canvas.transform, background, Vector2.zero, Vector2.one);
            selectionRoot = canvasObject;
            title = Label("RPS KNIGHTS", root.transform, 42, new Vector2(0.15f, 0.82f), new Vector2(0.85f, 0.94f));
            title.fontStyle = FontStyle.Bold;
            Label("손을 고르면 실제 가위바위보처럼 흔든 뒤 동시에 공개합니다", root.transform, 20,
                new Vector2(0.15f, 0.75f), new Vector2(0.85f, 0.82f));

            playerHand = Label("✊", root.transform, 150, new Vector2(0.12f, 0.34f), new Vector2(0.42f, 0.70f));
            opponentHand = Label("✊", root.transform, 150, new Vector2(0.58f, 0.34f), new Vector2(0.88f, 0.70f));
            playerHand.font = handFont;
            opponentHand.font = handFont;
            Label("나", root.transform, 22, new Vector2(0.12f, 0.66f), new Vector2(0.42f, 0.72f));
            Label("상대", root.transform, 22, new Vector2(0.58f, 0.66f), new Vector2(0.88f, 0.72f));
            Label("VS", root.transform, 30, new Vector2(0.45f, 0.48f), new Vector2(0.55f, 0.58f));
            chant = Label("무엇을 낼까요?", root.transform, 34, new Vector2(0.25f, 0.71f), new Vector2(0.75f, 0.78f));
            result = Label("", root.transform, 24, new Vector2(0.15f, 0.24f), new Vector2(0.85f, 0.34f));

            choicePanel = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            choicePanel.transform.SetParent(root.transform, false);
            var choiceRect = choicePanel.GetComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0.18f, 0.07f);
            choiceRect.anchorMax = new Vector2(0.82f, 0.20f);
            choiceRect.offsetMin = choiceRect.offsetMax = Vector2.zero;
            var layout = choicePanel.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 18;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;
            buttons = new[] {
                ChoiceButton("1  가위", Sign.Scissors),
                ChoiceButton("2  바위", Sign.Rock),
                ChoiceButton("3  보", Sign.Paper)
            };
        }

        private Button ChoiceButton(string caption, Sign sign)
        {
            Image image = Image(caption, choicePanel.transform, panel, Vector2.zero, Vector2.one);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => Choose(sign));
            Text label = Label(caption, image.transform, 25, Vector2.zero, Vector2.one);
            label.fontStyle = FontStyle.Bold;
            return button;
        }

        private void Choose(Sign player)
        {
            if (revealing) return;
            StartCoroutine(Reveal(player));
        }

        private IEnumerator Reveal(Sign player)
        {
            revealing = true;
            SetButtons(false);
            result.text = "";
            playerHand.text = opponentHand.text = "✊";
            string[] calls = { "가위…", "바위…", "보!" };
            foreach (string call in calls)
            {
                chant.text = call;
                yield return ShakeHands();
            }

            Sign opponent = (Sign)Random.Range(0, 3);
            RoundOutcome outcome = RpsRules.Resolve(player, opponent);
            playerHand.text = RpsRules.Hand(player);
            opponentHand.text = RpsRules.Hand(opponent);
            chant.text = "결과 공개!";
            string verdict = outcome == RoundOutcome.Win ? "승리 — 상대에게 짧은 패널티" :
                outcome == RoundOutcome.Loss ? "패배 — 나에게 짧은 패널티" : "무승부 — 패널티 없음";
            result.text = $"{RpsRules.KoreanName(player)}의 {RpsRules.Job(player)}  VS  " +
                          $"{RpsRules.KoreanName(opponent)}의 {RpsRules.Job(opponent)}\n{verdict}\n잠시 후 결투 시작";
            yield return new WaitForSecondsRealtime(1.8f);
            selectionRoot.SetActive(false);
            var duel = new GameObject("Duel Arena").AddComponent<DuelArena>();
            duel.Begin(player, opponent, outcome, ReturnFromDuel);
        }

        private void ReturnFromDuel()
        {
            selectionRoot.SetActive(true);
            playerHand.text = opponentHand.text = "✊";
            chant.text = "무엇을 낼까요?";
            result.text = "";
            SetButtons(true);
            revealing = false;
        }

        private IEnumerator ShakeHands()
        {
            RectTransform left = playerHand.rectTransform;
            RectTransform right = opponentHand.rectTransform;
            for (int step = 0; step < 10; step++)
            {
                float y = Mathf.Sin(step / 9f * Mathf.PI) * 38f;
                left.anchoredPosition = new Vector2(0, y);
                right.anchoredPosition = new Vector2(0, y);
                yield return new WaitForSecondsRealtime(0.035f);
            }
            left.anchoredPosition = right.anchoredPosition = Vector2.zero;
        }

        private void SetButtons(bool enabled)
        {
            foreach (Button button in buttons) button.interactable = enabled;
        }

        private Image Image(string name, Transform parentTransform, Color color, Vector2 min, Vector2 max)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parentTransform, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Text Label(string value, Transform parentTransform, int size, Vector2 min, Vector2 max)
        {
            var gameObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            gameObject.transform.SetParent(parentTransform, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = gameObject.GetComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            return text;
        }
    }
}
