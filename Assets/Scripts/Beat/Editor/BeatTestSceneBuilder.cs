using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BeatTiming.EditorTools
{
    /// <summary>Menu: Tools > Beat Timing > Create Test Scene. Builds BeatTest.unity from scratch.</summary>
    public static class BeatTestSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/BeatTest.unity";

        [MenuItem("Tools/Beat Timing/Create Test Scene")]
        public static void CreateTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            var system = new GameObject("Beat System");
            system.AddComponent<BeatConductor>();
            system.AddComponent<TimingJudge>();
            system.AddComponent<BeatMetronome>();

            new GameObject("Beat Test Harness").AddComponent<BeatTestHarness>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"Beat Timing: created {ScenePath}. Press Play and use Space on the beat.");
        }
    }
}
