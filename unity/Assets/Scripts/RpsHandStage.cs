using System;
using UnityEngine;

namespace RpsKnights
{
    public sealed class RpsHandStage : MonoBehaviour
    {
        private HandModel playerHand;
        private HandModel opponentHand;
        private Vector3 playerBase;
        private Vector3 opponentBase;

        public void Build()
        {
            Camera camera = new GameObject("Hand Camera", typeof(Camera)).GetComponent<Camera>();
            camera.transform.SetParent(transform);
            camera.transform.position = new Vector3(0f, 0.15f, -11.5f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.012f, 0.022f, 0.055f);
            camera.fieldOfView = 38f;

            BuildBackdrop();
            playerBase = new Vector3(-2.35f, -0.65f, 0f);
            opponentBase = new Vector3(2.35f, -0.65f, 0f);
            playerHand = new HandModel("Player 3D Hand", transform, new Color(0.16f, 0.56f, 1f), false);
            opponentHand = new HandModel("Opponent 3D Hand", transform, new Color(1f, 0.27f, 0.22f), true);
            ShowFists();
        }

        public void ShowFists()
        {
            playerHand.SetGesture(Sign.Rock);
            opponentHand.SetGesture(Sign.Rock);
            SetShakeOffset(0f);
        }

        public void ShowResults(Sign player, Sign opponent)
        {
            playerHand.SetGesture(player);
            opponentHand.SetGesture(opponent);
            SetShakeOffset(0f);
        }

        public void SetShakeOffset(float height)
        {
            playerHand.Root.localPosition = playerBase + Vector3.up * height;
            opponentHand.Root.localPosition = opponentBase + Vector3.up * height;
            float tilt = height * 4f;
            playerHand.Root.localEulerAngles = new Vector3(0f, 0f, -tilt);
            opponentHand.Root.localEulerAngles = new Vector3(0f, 0f, tilt);
        }

        private void BuildBackdrop()
        {
            CreateDisc("Player Glow", new Vector3(-2.35f, -1.5f, 1.7f), new Vector3(3.6f, 0.12f, 3.6f),
                new Color(0.03f, 0.22f, 0.48f));
            CreateDisc("Opponent Glow", new Vector3(2.35f, -1.5f, 1.7f), new Vector3(3.6f, 0.12f, 3.6f),
                new Color(0.48f, 0.08f, 0.07f));

            GameObject divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider.name = "Center Light";
            divider.transform.SetParent(transform, false);
            divider.transform.localPosition = new Vector3(0f, -0.35f, 2.2f);
            divider.transform.localScale = new Vector3(0.025f, 3.2f, 0.025f);
            ApplyMaterial(divider, new Color(0.28f, 0.46f, 0.7f));
        }

        private void CreateDisc(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(transform, false);
            disc.transform.localPosition = position;
            disc.transform.localScale = scale;
            ApplyMaterial(disc, color);
        }

        private static void ApplyMaterial(GameObject target, Color color)
        {
            Shader shader = Resources.Load<Shader>("RPSUnlit");
            target.GetComponent<Renderer>().material = new Material(shader) { color = color };
            Collider collider = target.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.Destroy(collider);
        }

        private sealed class HandModel
        {
            public Transform Root { get; }

            private readonly Color color;
            private readonly bool mirrored;
            private readonly Shader shader;
            private GameObject visibleHand;

            public HandModel(string name, Transform parent, Color handColor, bool mirror)
            {
                Root = new GameObject(name).transform;
                Root.SetParent(parent, false);
                color = handColor;
                mirrored = mirror;
                shader = Resources.Load<Shader>("RPSUnlit");
                if (shader == null) throw new InvalidOperationException("RPSUnlit shader is missing.");
            }

            public void SetGesture(Sign sign)
            {
                if (visibleHand != null) UnityEngine.Object.Destroy(visibleHand);

                string side = mirrored ? "R_" : "L_";
                string assetName = side + (sign == Sign.Paper ? "Paper" :
                    sign == Sign.Scissors ? "Scissors" : "Rock");
                GameObject source = Resources.Load<GameObject>("Hands/" + assetName);
                if (source == null) throw new InvalidOperationException(assetName + " model is missing.");

                visibleHand = UnityEngine.Object.Instantiate(source, Root);
                visibleHand.name = assetName;
                visibleHand.transform.localPosition = Vector3.zero;
                visibleHand.transform.localRotation = Quaternion.Euler(0f, 0f, mirrored ? 90f : -90f);
                visibleHand.transform.localScale = Vector3.one * 11.5f;

                Material material = new Material(shader) { color = color };
                material.SetColor("_RimColor", Color.Lerp(color, Color.white, 0.65f));
                foreach (Renderer renderer in visibleHand.GetComponentsInChildren<Renderer>())
                    renderer.material = material;
            }
        }
    }
}
