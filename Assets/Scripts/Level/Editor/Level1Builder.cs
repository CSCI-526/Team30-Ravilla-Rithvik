using System.Linq;
using BeatTiming;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Tools > Beat Boop > Build Level 1. Rebuilds Level1.unity ("First Beats", 90 BPM) from the layout below.
// Keeps the camera, lights, Player, BeatSystem and HUD; everything else is deleted and rebuilt under "Level",
// so it is safe to run again after changing a number here.
//
// Units: ground surface is y = 0. The player is 1 x 2, walks 8 u/s and a jump covers ~3.3 up and ~10 across,
// so every gap that has to be dashed sits under a low ceiling (0.25 of headroom) to rule out jumping.
// Good dash = 2.5, perfect dash = 4: a 3-wide pit takes any dash, a 4-wide pit only a perfect one.
public static class Level1Builder
{
    const string ScenePath = "Assets/Scenes/Level1.unity";
    const string Prefabs = "Assets/Prefabs/";

    const float GroundBottom = -4f;
    const float CeilingBottom = 2.25f;
    const float CeilingTop = 4.25f;
    const float Bpm = 90f;

    static readonly Color HintColor = new Color(0.78f, 0.66f, 1f);

    static Transform section;

    [MenuItem("Tools/Beat Boop/Build Level 1")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (!IsKept(root)) Object.DestroyImmediate(root);
        }

        GameObject player = EnsurePlayer();
        EnsureBeatSystem();
        EnsureHud();
        SetUpCamera(player.transform);

        var level = new GameObject("Level").transform;
        BuildLayout(level);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddToBuildSettings();
        Debug.Log("Level 1: built " + ScenePath + ". Press Play to test.");
    }

    static void BuildLayout(Transform level)
    {
        // ---- 1 Warm Up: move, jump, step, pit, spike ----
        Section(level, "1 Warm Up");
        Wall(-1f, 0f);
        Ground(0f, 16f);
        Ground(16f, 24f, 1.5f);
        Pit(24f, 27f);
        Ground(27f, 47f);
        Spike(31f);
        Checkpoint(35f);
        Hint(1f, 5f, "A / D to move    W to jump");
        Hint(21.5f, 4.5f, "Jump the pit");
        Hint(29f, 3f, "Jump the spike");

        // ---- 2 First Dash: good dash over a 3 gap, perfect dash over a 4 gap ----
        Section(level, "2 First Dash");
        Ceiling(42f, 56f);
        Pit(47f, 50f);
        Ground(50f, 63f);
        Ceiling(58f, 72f);
        Pit(63f, 67f);
        Ground(67f, 128f);
        Checkpoint(77f);
        Hint(37f, 6.2f, "DASH: press SPACE on a beat, then SPACE again on the next beat");
        Hint(37f, 5.4f, "Low ceiling, so you can't jump. Dash across!");
        Hint(57f, 4.8f, "Wider gap: only a PERFECT dash makes it. Dash from the edge.");

        // ---- 3 Break the Blue: blue blocks only break on a perfect dash ----
        Section(level, "3 Break the Blue");
        Ceiling(86f, 96f);
        BlueBox(91f);
        Ceiling(100f, 113f);
        BlueBox(104f);
        BlueBox(108.5f);
        Checkpoint(118f);
        Hint(81f, 5.4f, "Blue blocks only break on a PERFECT dash");
        Hint(99f, 4.8f, "Two in a row: dash, then dash again");

        // ---- 4 Ride the Beat: two moving platforms over lava, then a double spike ----
        Section(level, "4 Ride the Beat");
        Pit(128f, 160f);
        Ferry(132f, 8f);
        Ferry(148f, 8f);
        Ground(160f, 212f);
        Spike(164f);
        Spike(165f);
        Checkpoint(168f);
        Hint(121f, 4f, "Wait for the platform, then ride it across");

        // ---- 5 Saw Sampler: a still saw, then one that moves up and down on the beat ----
        Section(level, "5 Saw Sampler");
        Saw(178.5f, 0f);
        Saw(191f, 3.4f);
        Checkpoint(203f);
        Hint(175f, 3.5f, "Jump the saw");
        Hint(185f, 6f, "This saw moves on the beat. Go when it's up");

        // ---- 6 Encore: dash tunnel, spike, blue block, finish ----
        Section(level, "6 Encore");
        Ceiling(208f, 220f);
        Pit(212f, 215f);
        Ground(215f, 240f);
        Spike(223f);
        Ceiling(226f, 233f);
        BlueBox(229.5f);
        Finish(237f);
        Wall(240f, 241f);
        Hint(207f, 5.4f, "Encore: everything you've learned");
    }

    // ---------- Pieces ----------

    static void Section(Transform level, string name)
    {
        section = new GameObject(name).transform;
        section.SetParent(level, false);
    }

    static void Ground(float xMin, float xMax, float top = 0f) =>
        Box("Blocks/Block_Ground", "Ground", xMin, xMax, GroundBottom, top);

    static void Ceiling(float xMin, float xMax) =>
        Box("Blocks/Block_Ground", "Ceiling", xMin, xMax, CeilingBottom, CeilingTop);

    static void Wall(float xMin, float xMax) =>
        Box("Blocks/Block_Wall", "Wall", xMin, xMax, GroundBottom, 12f);

    // Bottomless pit: the lava is a trigger, so it hurts on the way down and the fall (below y -10) kills.
    // A solid lava floor would let the player stand in it and jump back out.
    static void Pit(float xMin, float xMax)
    {
        GameObject lava = Box("Hazard_Lava", "Lava", xMin, xMax, -1.5f, -0.5f);
        var col = lava.GetComponent<Collider2D>();
        col.isTrigger = true;
        Record(col);
    }

    static void Spike(float xMin) => Place("Hazard_Spike", "Spike", xMin + 0.5f, 0.3f);

    static void Checkpoint(float x) => Place("Checkpoint", "Checkpoint", x, 0.75f);

    static void BlueBox(float x) => Place("Enemy_BlueBox", "BlueBox", x, 1f);

    // rise = 0 gives a saw that stays put; otherwise it moves up by `rise` and back, one move every 2 beats
    static void Saw(float x, float rise)
    {
        GameObject saw = Place("Hazard_Saw", rise > 0f ? "Saw_Moving" : "Saw", x, 0.6f);
        var mover = saw.GetComponent<BeatMover>();
        if (mover == null) return;

        mover.waypoints = rise > 0f
            ? new[] { Vector2.zero, new Vector2(0f, rise) }
            : new[] { Vector2.zero };
        mover.beatsPerStep = 2;
        Record(mover);
    }

    // 4-wide platform whose top sits at ground level, sliding `travel` to the right and back
    static void Ferry(float startX, float travel)
    {
        GameObject platform = Place("Blocks/Platforms/Block_Moving_Platform", "MovingPlatform", startX, -0.25f);
        var mover = platform.GetComponent<MovingPlatform>();
        mover.travel = new Vector2(travel, 0f);
        mover.speed = 2f;
        mover.waitTime = 0.5f;
        Record(mover);
    }

    static void Finish(float x)
    {
        GameObject finish = Place("Finish_GameComplete", "Finish", x, 2f);
        var complete = finish.GetComponent<GameComplete>();
        complete.message = "LEVEL 1 COMPLETE";
        Record(complete);
    }

    // World-space tutorial prompt (plain TextMesh so it needs no extra packages)
    static void Hint(float x, float y, string text)
    {
        var go = new GameObject("Hint");
        go.transform.SetParent(section, false);
        go.transform.position = new Vector3(x, y, 0f);

        var mesh = go.AddComponent<TextMesh>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mesh.font = font;
        mesh.text = text;
        mesh.fontSize = 64;
        mesh.characterSize = 0.07f;
        mesh.anchor = TextAnchor.MiddleLeft;
        mesh.color = HintColor;

        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = font.material;
        renderer.sortingOrder = 20;
    }

    // ---------- Helpers ----------

    // Prefab scaled to fill the rectangle [xMin, xMax] x [yMin, yMax] (all block sprites are 1 x 1 units)
    static GameObject Box(string prefab, string name, float xMin, float xMax, float yMin, float yMax)
    {
        GameObject go = Place(prefab, name, (xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);
        go.transform.localScale = new Vector3(xMax - xMin, yMax - yMin, 1f);
        Record(go.transform);
        return go;
    }

    static GameObject Place(string prefab, string name, float x, float y)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        go.name = name;
        go.transform.SetParent(section, false);
        go.transform.position = new Vector3(x, y, 0f);
        Record(go.transform);
        return go;
    }

    static void Record(Object obj) => PrefabUtility.RecordPrefabInstancePropertyModifications(obj);

    // Scene-level objects that survive a rebuild
    static bool IsKept(GameObject root) =>
        root.GetComponentInChildren<Camera>(true) != null
        || root.GetComponentInChildren<Light>(true) != null
        || root.GetComponentInChildren<PlayerMovement>(true) != null
        || root.GetComponentInChildren<BeatConductor>(true) != null
        || root.GetComponentInChildren<PlayerHUD>(true) != null
        || root.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>(true) != null
        || root.name == "Global Volume";

    static GameObject EnsurePlayer()
    {
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        GameObject player = movement != null
            ? movement.gameObject
            : (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "Player.prefab"));

        player.name = "Player";
        player.transform.position = new Vector3(3f, 1f, 0f);
        Record(player.transform);
        return player;
    }

    static void EnsureBeatSystem()
    {
        var conductor = Object.FindFirstObjectByType<BeatConductor>();
        if (conductor == null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "BeatSystem.prefab"));
            conductor = go.GetComponent<BeatConductor>();
        }

        var so = new SerializedObject(conductor);
        so.FindProperty("bpm").floatValue = Bpm;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureHud()
    {
        if (Object.FindFirstObjectByType<PlayerHUD>() != null) return;
        PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "HUD.prefab"));
    }

    static void SetUpCamera(Transform player)
    {
        Camera cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        if (cam == null) return;

        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player;
        follow.useBounds = false;
        cam.transform.position = new Vector3(player.position.x, player.position.y + 1f, -10f);
        EditorUtility.SetDirty(follow);
    }

    static void AddToBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => !string.IsNullOrEmpty(s.path) && System.IO.File.Exists(s.path))
            .ToList();
        if (scenes.All(s => s.path != ScenePath))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
