#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// 关卡白模一键配置：Ground 层、Tilemap 碰撞、玩家组件。
/// </summary>
public static class LevelSetupEditor
{
    const string GroundLayerName = "Ground";
    const int GroundLayerIndex = 8;

    static readonly string[] LevelScenePaths =
    {
        "Assets/Scenes/第一关.unity",
        "Assets/Scenes/第二关.unity",
        "Assets/Scenes/第三关.unity",
        "Assets/Scenes/第四关.unity",
        "Assets/Scenes/第五关.unity",
        "Assets/Scenes/第六关.unity"
    };

    [InitializeOnLoadMethod]
    static void AutoSetupOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorPrefs.GetBool("Shanhaijing_LevelSetup_v2", false))
                return;

            EnsureGroundLayer();
            SetupAllLevelScenes();
            EditorPrefs.SetBool("Shanhaijing_LevelSetup_v2", true);
            Debug.Log("[山海经2026] 已自动配置 Ground 层、Tilemap 碰撞与玩家组件。");
        };
    }

    [MenuItem("山海经2026/Setup All Level Scenes")]
    public static void SetupAllLevelScenesMenu()
    {
        EnsureGroundLayer();
        SetupAllLevelScenes();
        Debug.Log("[山海经2026] 关卡场景配置完成。");
    }

    static void SetupAllLevelScenes()
    {
        var activeScene = SceneManager.GetActiveScene();
        string activePath = activeScene.path;

        foreach (string scenePath in LevelScenePaths)
        {
            if (!System.IO.File.Exists(scenePath))
                continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            SetupGroundInScene(scene);
            SetupPlayerInScene();
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(activePath))
            EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);
    }

    static void EnsureGroundLayer()
    {
        if (LayerMask.NameToLayer(GroundLayerName) >= 0)
            return;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        SerializedProperty groundSlot = layers.GetArrayElementAtIndex(GroundLayerIndex);
        groundSlot.stringValue = GroundLayerName;
        tagManager.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    static void SetupGroundInScene(Scene scene)
    {
        int groundLayer = LayerMask.NameToLayer(GroundLayerName);
        if (groundLayer < 0)
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Tilemap tilemap in root.GetComponentsInChildren<Tilemap>(true))
            {
                tilemap.gameObject.layer = groundLayer;

                var tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
                if (tilemapCollider == null)
                    tilemapCollider = tilemap.gameObject.AddComponent<TilemapCollider2D>();

                tilemapCollider.usedByComposite = true;

                Transform gridTransform = tilemap.transform.parent;
                if (gridTransform == null)
                    continue;

                var rigidbody = gridTransform.GetComponent<Rigidbody2D>();
                if (rigidbody == null)
                    rigidbody = gridTransform.gameObject.AddComponent<Rigidbody2D>();
                rigidbody.bodyType = RigidbodyType2D.Static;

                var composite = gridTransform.GetComponent<CompositeCollider2D>();
                if (composite == null)
                    composite = gridTransform.gameObject.AddComponent<CompositeCollider2D>();
                composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            }
        }
    }

    static void SetupPlayerInScene()
    {
        GameObject player = GameObject.Find("Capsule");
        if (player == null)
            player = GameObject.Find("Player");

        if (player == null)
            return;

        var rigidbody = player.GetComponent<Rigidbody2D>();
        if (rigidbody == null)
            rigidbody = player.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 3f;
        rigidbody.freezeRotation = true;
        rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = player.GetComponent<CapsuleCollider2D>();
        if (collider == null)
            collider = player.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.6f, 1.6f);

        if (player.GetComponent<InputManager>() == null)
            player.AddComponent<InputManager>();

        if (player.GetComponent<PlayerController>() == null)
            player.AddComponent<PlayerController>();

        Transform groundCheck = player.transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            var groundCheckObject = new GameObject("GroundCheck");
            groundCheckObject.transform.SetParent(player.transform);
            groundCheckObject.transform.localPosition = new Vector3(0f, -0.8f, 0f);
        }
    }
}
#endif
