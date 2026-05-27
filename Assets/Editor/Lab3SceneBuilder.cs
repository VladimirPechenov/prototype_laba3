using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Lab3SceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string AutoBuildMarkerPath = "Assets/Editor/Lab3SceneBuilt.marker";

    [InitializeOnLoadMethod]
    private static void AutoBuildOnceAfterImport()
    {
        if (Application.isBatchMode || File.Exists(AutoBuildMarkerPath))
            return;

        EditorApplication.delayCall += () =>
        {
            if (File.Exists(AutoBuildMarkerPath))
                return;

            RebuildPrototypeScene();
            File.WriteAllText(AutoBuildMarkerPath, "Lab 3 scene was generated automatically.");
            AssetDatabase.ImportAsset(AutoBuildMarkerPath);
        };
    }

    [MenuItem("Tools/Lab 3/Rebuild Prototype Scene")]
    public static void RebuildPrototypeScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "SampleScene";

        CreateLighting();
        Material groundMaterial = CreateMaterial("Lab3_Ground", new Color(0.35f, 0.42f, 0.38f));
        Material platformMaterial = CreateMaterial("Lab3_Platform", new Color(0.45f, 0.5f, 0.58f));
        Material playerMaterial = CreateMaterial("Lab3_Player", new Color(0.1f, 0.42f, 0.72f));
        Material collectibleMaterial = CreateMaterial("Lab3_Collectible", new Color(1f, 0.78f, 0.18f));
        Material doorMaterial = CreateMaterial("Lab3_Door", new Color(0.65f, 0.18f, 0.18f));
        Material switchMaterial = CreateMaterial("Lab3_Switch", new Color(0.1f, 0.6f, 0.42f));

        CreateBox("Ground", new Vector3(0f, -0.05f, 0f), new Vector3(16f, 0.1f, 16f), groundMaterial);
        CreateBox("Raised Platform A", new Vector3(3.8f, 0.65f, 1.5f), new Vector3(3.2f, 0.3f, 3f), platformMaterial);
        CreateBox("Raised Platform B", new Vector3(-3.8f, 1.1f, 3.6f), new Vector3(2.8f, 0.3f, 2.4f), platformMaterial);
        CreateBox("Goal Platform", new Vector3(0f, 0.4f, 7.1f), new Vector3(4f, 0.25f, 2f), platformMaterial);

        GameObject player = CreatePlayer(playerMaterial);
        Camera camera = CreateCamera(player.transform);
        CreateUi();
        CreateCollectibles(collectibleMaterial);
        CreateDoorAndSwitch(doorMaterial, switchMaterial);

        Selection.activeGameObject = player;
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Lab 3 prototype scene rebuilt at {ScenePath}. Main camera: {camera.name}");
    }

    private static void CreateLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.85f;

        GameObject lightObject = new("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static GameObject CreatePlayer(Material material)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.05f, -4f);
        player.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.35f;
        controller.center = Vector3.zero;
        controller.slopeLimit = 50f;
        controller.stepOffset = 0.35f;

        GameObject groundCheck = new("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.96f, 0f);

        PlayerController playerController = player.AddComponent<PlayerController>();
        SetSerialized(playerController, "groundCheck", groundCheck.transform);
        SetSerialized(playerController, "walkSpeed", 5f);
        SetSerialized(playerController, "runSpeed", 8f);
        SetSerialized(playerController, "jumpHeight", 1.65f);
        SetSerialized(playerController, "airControl", 0.65f);
        SetSerialized(playerController, "coyoteTime", 0.14f);
        SetSerialized(playerController, "jumpBufferTime", 0.14f);

        return player;
    }

    private static Camera CreateCamera(Transform player)
    {
        GameObject cameraObject = new("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 3.2f, -9f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 68f;
        camera.clearFlags = CameraClearFlags.Skybox;
        cameraObject.AddComponent<AudioListener>();

        CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
        follow.SetTarget(player);
        return camera;
    }

    private static void CreateUi()
    {
        GameObject managerObject = new("GameManager");
        GameManager manager = managerObject.AddComponent<GameManager>();

        Canvas canvas = new GameObject("HUD").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        Text scoreText = CreateText(canvas.transform, "ScoreText", "Score: 0", TextAnchor.UpperLeft, new Vector2(18f, -18f), new Vector2(280f, 42f), 24);
        Text promptText = CreateText(canvas.transform, "PromptText", string.Empty, TextAnchor.LowerCenter, new Vector2(0f, 54f), new Vector2(340f, 46f), 24);
        promptText.enabled = false;

        SetSerialized(manager, "scoreText", scoreText);
        SetSerialized(manager, "promptText", promptText);
    }

    private static Text CreateText(Transform parent, string name, string value, TextAnchor anchor, Vector2 anchoredPosition, Vector2 size, int fontSize)
    {
        GameObject textObject = new(name);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        RectTransform rect = text.rectTransform;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        if (anchor == TextAnchor.UpperLeft)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
        }

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
        outline.effectDistance = new Vector2(2f, -2f);
        return text;
    }

    private static void CreateCollectibles(Material material)
    {
        Vector3[] positions =
        {
            new(-2f, 0.65f, -1f),
            new(2.5f, 0.65f, -0.4f),
            new(3.8f, 1.15f, 1.5f),
            new(-3.8f, 1.6f, 3.6f),
            new(0f, 0.9f, 7.1f),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            collectible.name = $"Collectible Coin {i + 1}";
            collectible.transform.position = positions[i];
            collectible.transform.localScale = Vector3.one * 0.45f;
            collectible.GetComponent<Renderer>().sharedMaterial = material;
            collectible.GetComponent<SphereCollider>().isTrigger = true;
            collectible.AddComponent<Collectible>();

            GameObject particles = new("Collect Particles");
            particles.transform.SetParent(collectible.transform);
            particles.transform.localPosition = Vector3.zero;
            ParticleSystem particleSystem = particles.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.startColor = new Color(1f, 0.82f, 0.18f);
            main.startLifetime = 0.35f;
            main.startSpeed = 2.2f;
            main.maxParticles = 32;
            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;
            SetSerialized(collectible.GetComponent<Collectible>(), "collectParticles", particleSystem);
        }
    }

    private static void CreateDoorAndSwitch(Material doorMaterial, Material switchMaterial)
    {
        GameObject door = CreateBox("Sliding Door", new Vector3(0f, 1.5f, 5.8f), new Vector3(2.2f, 3f, 0.35f), doorMaterial);
        SlidingDoor slidingDoor = door.AddComponent<SlidingDoor>();
        SetSerialized(slidingDoor, "openOffset", new Vector3(0f, 3.2f, 0f));
        SetSerialized(slidingDoor, "moveSpeed", 4.2f);

        GameObject switchBase = CreateBox("Door Switch", new Vector3(-2.2f, 0.25f, 4.7f), new Vector3(0.9f, 0.5f, 0.9f), switchMaterial);
        BoxCollider trigger = switchBase.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(3f, 2f, 3f);
        trigger.center = new Vector3(0f, 0.8f, 0f);

        Interactable interactable = switchBase.AddComponent<Interactable>();
        SetSerialized(interactable, "promptText", "Press E to open the door");
        UnityEventTools.AddPersistentListener(interactable.OnInteract, slidingDoor.Toggle);
    }

    private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.position = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        return box;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        const string directory = "Assets/Materials";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string path = $"{directory}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetSerialized(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new(target);
        serializedObject.FindProperty(propertyName).objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerialized(Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new(target);
        serializedObject.FindProperty(propertyName).floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerialized(Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new(target);
        serializedObject.FindProperty(propertyName).stringValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerialized(Object target, string propertyName, Vector3 value)
    {
        SerializedObject serializedObject = new(target);
        serializedObject.FindProperty(propertyName).vector3Value = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
