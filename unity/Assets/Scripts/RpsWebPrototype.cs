using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RpsKnights
{
    public sealed class RpsWebPrototype : MonoBehaviour
    {
        private readonly Color panel = new(0.08f, 0.13f, 0.23f, 0.96f);
        private Font font;
        private Text chant;
        private Text result;
        private GameObject selectionRoot;
        private RpsHandStage handStage;
        private Button playButton;
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
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildHandStage();
            BuildUi();
        }

        private void Update()
        {
            if (!revealing && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
                StartRandomRound();
        }

        private void BuildHandStage()
        {
            handStage = new GameObject("3D RPS Hand Stage").AddComponent<RpsHandStage>();
            handStage.Build();
        }

        private void BuildUi()
        {
            var canvasObject = new GameObject("Selection UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            selectionRoot = canvasObject;
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            Image overlay = Image("Overlay", canvas.transform, new Color(0.02f, 0.04f, 0.08f, 0.18f), Vector2.zero, Vector2.one);
            Text title = Label("RPS KNIGHTS", overlay.transform, 42, new Vector2(0.15f, 0.84f), new Vector2(0.85f, 0.95f));
            title.fontStyle = FontStyle.Bold;
            Label("버튼을 누르면 두 손의 결과가 무작위로 정해집니다", overlay.transform, 20,
                new Vector2(0.15f, 0.77f), new Vector2(0.85f, 0.84f));
            Label("나", overlay.transform, 23, new Vector2(0.12f, 0.68f), new Vector2(0.42f, 0.75f));
            Label("상대", overlay.transform, 23, new Vector2(0.58f, 0.68f), new Vector2(0.88f, 0.75f));
            Label("VS", overlay.transform, 31, new Vector2(0.45f, 0.47f), new Vector2(0.55f, 0.57f));
            chant = Label("준비되면 시작하세요", overlay.transform, 34, new Vector2(0.25f, 0.7f), new Vector2(0.75f, 0.79f));
            result = Label("", overlay.transform, 23, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.34f));

            Image buttonImage = Image("Random RPS", overlay.transform, panel, new Vector2(0.32f, 0.06f), new Vector2(0.68f, 0.18f));
            playButton = buttonImage.gameObject.AddComponent<Button>();
            playButton.targetGraphic = buttonImage;
            playButton.onClick.AddListener(StartRandomRound);
            Text buttonLabel = Label("가위바위보!", buttonImage.transform, 29, Vector2.zero, Vector2.one);
            buttonLabel.fontStyle = FontStyle.Bold;
        }

        private void StartRandomRound()
        {
            if (revealing) return;
            Sign player = (Sign)Random.Range(0, 3);
            Sign opponent = (Sign)Random.Range(0, 3);
            StartCoroutine(Reveal(player, opponent));
        }

        private IEnumerator Reveal(Sign player, Sign opponent)
        {
            revealing = true;
            playButton.interactable = false;
            result.text = "";
            handStage.ShowFists();
            string[] calls = { "가위…", "바위…", "보!" };
            foreach (string call in calls)
            {
                chant.text = call;
                for (int step = 0; step < 12; step++)
                {
                    float height = Mathf.Sin(step / 11f * Mathf.PI) * 0.55f;
                    handStage.SetShakeOffset(height);
                    yield return new WaitForSecondsRealtime(0.035f);
                }
            }

            handStage.ShowResults(player, opponent);
            RoundOutcome outcome = RpsRules.Resolve(player, opponent);
            chant.text = "결과 공개!";
            string verdict = outcome == RoundOutcome.Win ? "승리 — 상대에게 짧은 패널티" :
                outcome == RoundOutcome.Loss ? "패배 — 나에게 짧은 패널티" : "무승부 — 패널티 없음";
            result.text = $"나: {RpsRules.KoreanName(player)} · {RpsRules.Job(player)}    " +
                          $"상대: {RpsRules.KoreanName(opponent)} · {RpsRules.Job(opponent)}\n{verdict}\n잠시 후 결투 시작";
            yield return new WaitForSecondsRealtime(2.1f);
            selectionRoot.SetActive(false);
            handStage.gameObject.SetActive(false);
            var duel = new GameObject("Duel Arena").AddComponent<DuelArena>();
            duel.Begin(player, opponent, outcome, ReturnFromDuel);
        }

        private void ReturnFromDuel()
        {
            handStage.gameObject.SetActive(true);
            handStage.ShowFists();
            selectionRoot.SetActive(true);
            chant.text = "준비되면 시작하세요";
            result.text = "";
            playButton.interactable = true;
            revealing = false;
        }

        private Image Image(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = item.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Text Label(string value, Transform parent, int size, Vector2 min, Vector2 max)
        {
            var item = new GameObject("Text", typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = item.GetComponent<Text>();
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
