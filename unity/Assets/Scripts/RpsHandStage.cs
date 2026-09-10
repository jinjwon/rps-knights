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
            camera.transform.position = new Vector3(0f, 0.25f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.04f, 0.08f);
            camera.fieldOfView = 42f;

            playerBase = new Vector3(-2.25f, -0.25f, 0f);
            opponentBase = new Vector3(2.25f, -0.25f, 0f);
            playerHand = new HandModel("Player 3D Hand", transform, new Color(0.25f, 0.65f, 1f));
            opponentHand = new HandModel("Opponent 3D Hand", transform, new Color(1f, 0.36f, 0.28f));
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
        }

        private sealed class HandModel
        {
            public Transform Root { get; }
            private readonly Color color;
            private readonly Shader shader;

            public HandModel(string name, Transform parent, Color handColor)
            {
                Root = new GameObject(name).transform;
                Root.SetParent(parent, false);
                color = handColor;
                shader = Resources.Load<Shader>("RPSUnlit");
                if (shader == null) throw new InvalidOperationException("RPSUnlit shader is missing.");
            }

            public void SetGesture(Sign sign)
            {
                for (int index = Root.childCount - 1; index >= 0; index--)
                    UnityEngine.Object.Destroy(Root.GetChild(index).gameObject);

                Part("Palm", "Cube.fbx", new Vector3(0f, -0.15f, 0f), new Vector3(1.25f, 1.25f, 0.5f), Vector3.zero);
                switch (sign)
                {
                    case Sign.Paper: BuildPaper(); break;
                    case Sign.Scissors: BuildScissors(); break;
                    default: BuildRock(); break;
                }
            }

            private void BuildPaper()
            {
                float[] x = { -0.48f, -0.16f, 0.16f, 0.48f };
                float[] lengths = { 0.82f, 1.02f, 0.94f, 0.72f };
                for (int i = 0; i < x.Length; i++)
                    Part("Finger", "Cube.fbx", new Vector3(x[i], 0.85f + lengths[i] * 0.42f, -0.02f),
                        new Vector3(0.24f, lengths[i], 0.3f), Vector3.zero);
                Part("Thumb", "Cube.fbx", new Vector3(-0.82f, 0.1f, -0.08f),
                    new Vector3(0.28f, 0.72f, 0.32f), new Vector3(0f, 0f, 58f));
            }

            private void BuildScissors()
            {
                Part("Index", "Cube.fbx", new Vector3(-0.3f, 1.15f, -0.02f),
                    new Vector3(0.25f, 1.15f, 0.3f), new Vector3(0f, 0f, -14f));
                Part("Middle", "Cube.fbx", new Vector3(0.32f, 1.15f, -0.02f),
                    new Vector3(0.25f, 1.15f, 0.3f), new Vector3(0f, 0f, 14f));
                Part("Folded Ring", "Sphere.fbx", new Vector3(0.2f, 0.5f, -0.28f),
                    new Vector3(0.34f, 0.34f, 0.3f), Vector3.zero);
                Part("Folded Pinky", "Sphere.fbx", new Vector3(0.5f, 0.4f, -0.28f),
                    new Vector3(0.3f, 0.3f, 0.28f), Vector3.zero);
                Part("Thumb", "Cube.fbx", new Vector3(-0.5f, 0.1f, -0.34f),
                    new Vector3(0.28f, 0.7f, 0.32f), new Vector3(0f, 0f, 64f));
            }

            private void BuildRock()
            {
                float[] x = { -0.46f, -0.15f, 0.16f, 0.47f };
                for (int i = 0; i < x.Length; i++)
                    Part("Knuckle", "Sphere.fbx", new Vector3(x[i], 0.48f, -0.28f),
                        new Vector3(0.34f, 0.38f, 0.3f), Vector3.zero);
                Part("Thumb", "Cube.fbx", new Vector3(-0.05f, 0.02f, -0.42f),
                    new Vector3(0.32f, 0.88f, 0.32f), new Vector3(0f, 0f, 88f));
            }

            private void Part(string name, string meshName, Vector3 position, Vector3 scale, Vector3 rotation)
            {
                var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
                item.transform.SetParent(Root, false);
                item.transform.localPosition = position;
                item.transform.localScale = scale;
                item.transform.localEulerAngles = rotation;
                item.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>(meshName);
                item.GetComponent<MeshRenderer>().material = new Material(shader) { color = color };
            }
        }
    }
}
