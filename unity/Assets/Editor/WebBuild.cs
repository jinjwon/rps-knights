using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RpsKnights.Editor
{
    [InitializeOnLoad]
    public static class WebBuild
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        static WebBuild()
        {
            EditorApplication.delayCall += EnsureScene;
        }

        [MenuItem("RPS Knights/웹 빌드 만들기")]
        public static void BuildWeb()
        {
            EnsureScene();
            Directory.CreateDirectory("Builds/Web");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Web",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            BuildPipeline.BuildPlayer(options);
        }

        public static void BuildWebFromCommandLine()
        {
            BuildWeb();
        }

        private static void EnsureScene()
        {
            if (File.Exists(ScenePath)) return;
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("RPS Knights web prototype scene created.");
        }
    }
}
