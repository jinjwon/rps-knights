using System;
using UnityEngine;
using UnityEngine.UI;

namespace RpsKnights
{
    public sealed class DuelArena : MonoBehaviour
    {
        private const float RoundSeconds = 45f;
        private const float MaxHealth = 100f;
        private Transform player;
        private Transform opponent;
        private float playerHealth = MaxHealth;
        private float opponentHealth = MaxHealth;
        private float remaining = RoundSeconds;
        private float playerAttackCooldown;
        private float opponentAttackCooldown;
        private float playerPenaltyRemaining;
        private float opponentPenaltyRemaining;
        private Text status;
        private Text playerHud;
        private Text opponentHud;
        private Button attackButton;
        private Action onFinished;
        private bool finished;

        public void Begin(Sign playerSign, Sign opponentSign, RoundOutcome rpsOutcome, Action finishedCallback)
        {
            onFinished = finishedCallback;
            if (rpsOutcome == RoundOutcome.Loss) playerPenaltyRemaining = 4f;
            if (rpsOutcome == RoundOutcome.Win) opponentPenaltyRemaining = 4f;
            BuildWorld(playerSign, opponentSign);
            BuildHud();
        }

        private void Update()
        {
            if (finished) return;
            float dt = Time.deltaTime;
            remaining = Mathf.Max(0f, remaining - dt);
            playerAttackCooldown = Mathf.Max(0f, playerAttackCooldown - dt);
            opponentAttackCooldown = Mathf.Max(0f, opponentAttackCooldown - dt);
            playerPenaltyRemaining = Mathf.Max(0f, playerPenaltyRemaining - dt);
            opponentPenaltyRemaining = Mathf.Max(0f, opponentPenaltyRemaining - dt);

            Vector3 input = new(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            float playerSpeed = playerPenaltyRemaining > 0f ? 2.6f : 3.6f;
            player.position += Vector3.ClampMagnitude(input, 1f) * (playerSpeed * dt);
            player.position = ClampToArena(player.position);
            player.LookAt(new Vector3(opponent.position.x, player.position.y, opponent.position.z));

            Vector3 toPlayer = player.position - opponent.position;
            float distance = toPlayer.magnitude;
            if (distance > 1.7f)
            {
                float botSpeed = opponentPenaltyRemaining > 0f ? 2.0f : 2.8f;
                opponent.position += toPlayer.normalized * (botSpeed * dt);
                opponent.position = ClampToArena(opponent.position);
            }
            opponent.LookAt(new Vector3(player.position.x, opponent.position.y, player.position.z));

            if (Input.GetKeyDown(KeyCode.Space)) PlayerAttack();
            if (distance <= 1.9f && opponentAttackCooldown <= 0f)
            {
                playerHealth = Mathf.Max(0f, playerHealth - 11f);
                opponentAttackCooldown = 0.9f;
                Pulse(opponent);
            }

            RefreshHud();
            if (playerHealth <= 0f || opponentHealth <= 0f || remaining <= 0f) FinishRound();
        }

        private void PlayerAttack()
        {
            if (finished || playerAttackCooldown > 0f) return;
            playerAttackCooldown = 0.65f;
            Pulse(player);
            if (Vector3.Distance(player.position, opponent.position) <= 2.3f)
                opponentHealth = Mathf.Max(0f, opponentHealth - 14f);
        }

        private void FinishRound()
        {
            finished = true;
            attackButton.interactable = false;
            string verdict = playerHealth > opponentHealth ? "결투 승리!" :
                playerHealth < opponentHealth ? "결투 패배" : "결투 무승부";
            status.text = verdict + "\n다음 가위바위보로 돌아갑니다";
            Invoke(nameof(ReturnToSelection), 2.5f);
        }

        private void ReturnToSelection()
        {
            onFinished?.Invoke();
            Destroy(gameObject);
        }

        private void BuildWorld(Sign playerSign, Sign opponentSign)
        {
            Camera camera = new GameObject("Duel Camera", typeof(Camera)).GetComponent<Camera>();
            camera.transform.SetParent(transform);
            camera.transform.position = new Vector3(0f, 8.5f, -9.5f);
            camera.transform.rotation = Quaternion.Euler(32f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.10f);

            var light = new GameObject("Arena Light", typeof(Light)).GetComponent<Light>();
            light.transform.SetParent(transform);
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Transform floor = Primitive("Arena", PrimitiveType.Cylinder, new Vector3(0f, -0.3f, 1f),
                new Vector3(10f, 0.3f, 10f), new Color(0.10f, 0.14f, 0.22f));
            floor.SetParent(transform);
            player = Knight("Player Knight", new Vector3(0f, 0.8f, -3f), playerSign, new Color(0.25f, 0.65f, 1f));
            opponent = Knight("Opponent Knight", new Vector3(0f, 0.8f, 3f), opponentSign, new Color(1f, 0.35f, 0.28f));
        }

        private Transform Knight(string objectName, Vector3 position, Sign sign, Color color)
        {
            Transform body = Primitive(objectName, PrimitiveType.Capsule, position, Vector3.one, color);
            body.SetParent(transform);
            Transform weapon = Primitive("Weapon", sign == Sign.Paper ? PrimitiveType.Cube : PrimitiveType.Cylinder,
                new Vector3(0.75f, 0.2f, 0.2f), sign == Sign.Paper ? new Vector3(0.18f, 0.8f, 0.65f) : new Vector3(0.12f, 0.8f, 0.12f),
                sign == Sign.Rock ? new Color(0.8f, 0.65f, 0.25f) : new Color(0.75f, 0.8f, 0.9f));
            weapon.SetParent(body, false);
            return body;
        }

        private Transform Primitive(string objectName, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            var item = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.position = position;
            item.transform.localScale = scale;
            string meshName = type switch
            {
                PrimitiveType.Cube => "Cube.fbx",
                PrimitiveType.Cylinder => "Cylinder.fbx",
                PrimitiveType.Sphere => "Sphere.fbx",
                _ => "Capsule.fbx"
            };
            item.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>(meshName);
            Shader shader = Resources.Load<Shader>("RPSUnlit");
            if (shader == null) throw new InvalidOperationException("RPSUnlit shader is missing from Resources.");
            var material = new Material(shader);
            material.color = color;
            item.GetComponent<Renderer>().material = material;
            return item.transform;
        }

        private void BuildHud()
        {
            Font font = Resources.Load<Font>("Fonts/NotoSansKR") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Duel HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            playerHud = Label(canvas.transform, font, new Vector2(0.04f, 0.86f), new Vector2(0.35f, 0.97f), TextAnchor.MiddleLeft, 25);
            opponentHud = Label(canvas.transform, font, new Vector2(0.65f, 0.86f), new Vector2(0.96f, 0.97f), TextAnchor.MiddleRight, 25);
            status = Label(canvas.transform, font, new Vector2(0.32f, 0.84f), new Vector2(0.68f, 0.98f), TextAnchor.MiddleCenter, 28);
            status.text = "결투 시작!";

            var attackImage = new GameObject("Attack", typeof(RectTransform), typeof(Image), typeof(Button));
            attackImage.transform.SetParent(canvas.transform, false);
            var rect = attackImage.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.78f, 0.06f);
            rect.anchorMax = new Vector2(0.95f, 0.18f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            attackImage.GetComponent<Image>().color = new Color(0.72f, 0.18f, 0.15f, 0.95f);
            attackButton = attackImage.GetComponent<Button>();
            attackButton.onClick.AddListener(PlayerAttack);
            Text attackText = Label(attackImage.transform, font, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 25);
            attackText.text = "공격  SPACE";
            Label(canvas.transform, font, new Vector2(0.04f, 0.04f), new Vector2(0.42f, 0.13f), TextAnchor.MiddleLeft, 19).text =
                "이동 W·A·S·D   가까이 가서 공격";
            RefreshHud();
        }

        private Text Label(Transform parent, Font font, Vector2 min, Vector2 max, TextAnchor anchor, int size)
        {
            var item = new GameObject("Text", typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = item.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = anchor;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            return text;
        }

        private void RefreshHud()
        {
            string penalty = playerPenaltyRemaining > 0f ? $"  이속 감소 {playerPenaltyRemaining:0.0}초" : "";
            string botPenalty = opponentPenaltyRemaining > 0f ? $"  이속 감소 {opponentPenaltyRemaining:0.0}초" : "";
            playerHud.text = $"나  HP {playerHealth:0}/{MaxHealth}{penalty}";
            opponentHud.text = $"상대  HP {opponentHealth:0}/{MaxHealth}{botPenalty}";
            if (!finished) status.text = $"남은 시간 {remaining:0.0}초";
        }

        private static Vector3 ClampToArena(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, -4.4f, 4.4f);
            position.z = Mathf.Clamp(position.z, -3.9f, 5.2f);
            return position;
        }

        private void Pulse(Transform target)
        {
            target.localScale = Vector3.one * 1.18f;
            LeanBack(target);
        }

        private async void LeanBack(Transform target)
        {
            await Awaitable.WaitForSecondsAsync(0.12f);
            if (target != null) target.localScale = Vector3.one;
        }
    }
}
