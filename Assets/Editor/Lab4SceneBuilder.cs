using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class Lab4SceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Lab 4/Rebuild Cemetery Level")]
    public static void RebuildCemeteryLevel()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "SampleScene";

        Material ground = CreateMaterial("Lab4_MudGround", new Color(0.12f, 0.16f, 0.13f));
        Material wall = CreateMaterial("Lab4_StoneWall", new Color(0.22f, 0.24f, 0.25f));
        Material grave = CreateMaterial("Lab4_GraveStone", new Color(0.42f, 0.45f, 0.42f));
        Material interact = CreateMaterial("Lab4_InteractYellow", new Color(0.95f, 0.74f, 0.18f));
        Material danger = CreateMaterial("Lab4_DangerRed", new Color(0.55f, 0.06f, 0.06f));
        Material safe = CreateMaterial("Lab4_SafeMoss", new Color(0.18f, 0.36f, 0.2f));
        Material playerMaterial = CreateMaterial("Lab4_Player", new Color(0.18f, 0.24f, 0.3f));
        Material enemyMaterial = CreateMaterial("Lab4_Enemy", new Color(0.62f, 0.08f, 0.08f));
        Material completedGrave = CreateMaterial("Lab4_DugGrave", new Color(0.07f, 0.05f, 0.04f));
        Material ritual = CreateMaterial("Lab4_RitualWhite", new Color(0.86f, 0.9f, 0.82f));
        Material healing = CreateMaterial("Lab5_HealthPickup", new Color(0.1f, 0.75f, 0.35f));
        Material coin = CreateMaterial("Lab6_Coin", new Color(1f, 0.78f, 0.16f));
        Material keyFragment = CreateMaterial("Lab6_KeyFragment", new Color(0.55f, 0.85f, 1f));
        Material blessedRelic = CreateMaterial("Lab6_BlessedRelic", new Color(0.9f, 0.95f, 0.68f));

        CreateLightingAndFog();
        EnsureTag("Enemy");
        CreateReusablePrefabs(wall, grave, safe, interact);

        GameObject level = new("Level - Old Cemetery Greybox");
        BuildGroundAndBoundaries(level.transform, ground, wall);
        BuildZones(level.transform, ground, wall, grave, safe, interact, danger, completedGrave, ritual, healing, coin, keyFragment, blessedRelic);

        GameObject player = CreatePlayer(playerMaterial);
        CreateFirstPersonCamera(player.transform);
        CreateUi(player);
        CreateEnemies(player.transform, enemyMaterial);

        NavMeshSurface surface = level.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();

        Selection.activeGameObject = player;
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Lab 4 cemetery level rebuilt at {ScenePath}");
    }

    private static void CreateLightingAndFog()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.055f, 0.07f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.09f, 0.12f, 0.1f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.025f;

        GameObject moon = new("Moon Directional Light");
        Light light = moon.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.38f;
        light.color = new Color(0.62f, 0.72f, 0.86f);
        light.shadows = LightShadows.Soft;
        moon.transform.rotation = Quaternion.Euler(50f, -25f, 0f);
    }

    private static void CreateReusablePrefabs(Material wall, Material grave, Material safe, Material interact)
    {
        const string directory = "Assets/Prefabs/Lab4";
        Directory.CreateDirectory(directory);

        SaveCubePrefab($"{directory}/Wall_Block.prefab", "Wall_Block", wall, new Vector3(1f, 2.5f, 1f));
        SaveCubePrefab($"{directory}/GraveStone.prefab", "GraveStone", grave, new Vector3(0.45f, 1.2f, 0.2f));
        SaveCubePrefab($"{directory}/MossCover.prefab", "MossCover", safe, new Vector3(1f, 0.08f, 1f));
        SaveCubePrefab($"{directory}/InteractMarker.prefab", "InteractMarker", interact, new Vector3(0.45f, 0.45f, 0.45f));
    }

    private static void SaveCubePrefab(string path, string name, Material material, Vector3 scale)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        PrefabUtility.SaveAsPrefabAsset(cube, path);
        Object.DestroyImmediate(cube);
    }

    private static void BuildGroundAndBoundaries(Transform parent, Material ground, Material wall)
    {
        GameObject floor = CreateBox("Cemetery Ground", new Vector3(0f, -0.05f, 0f), new Vector3(24f, 0.1f, 64f), ground, parent);
        floor.isStatic = true;

        CreateBox("Left Boundary Wall", new Vector3(-12.2f, 1.35f, 0f), new Vector3(0.4f, 2.8f, 64f), wall, parent).isStatic = true;
        CreateBox("Right Boundary Wall", new Vector3(12.2f, 1.35f, 0f), new Vector3(0.4f, 2.8f, 64f), wall, parent).isStatic = true;
        CreateBox("Start Iron Gate", new Vector3(0f, 1.35f, -31.8f), new Vector3(24f, 2.8f, 0.4f), wall, parent).isStatic = true;
        CreateBox("Back Cemetery Wall", new Vector3(0f, 1.35f, 31.8f), new Vector3(24f, 2.8f, 0.4f), wall, parent).isStatic = true;
    }

    private static void BuildZones(Transform parent, Material ground, Material wall, Material grave, Material safe, Material interact, Material danger, Material completedGrave, Material ritual, Material healing, Material coin, Material keyFragment, Material blessedRelic)
    {
        CreateZoneLabel(parent, "Zone 1 - Entrance / Safe movement tutorial", new Vector3(0f, 0.04f, -24f), new Vector3(10f, 0.08f, 9f), safe);
        CreateZoneLabel(parent, "Zone 2 - Graves / digging tutorial", new Vector3(0f, 0.04f, -10f), new Vector3(13f, 0.08f, 12f), ground);
        CreateZoneLabel(parent, "Zone 3 - Patrol / stealth and flashlight risk", new Vector3(0f, 0.04f, 7f), new Vector3(16f, 0.08f, 16f), ground);
        CreateZoneLabel(parent, "Zone 4 - Chapel yard / combined challenge", new Vector3(0f, 0.04f, 24f), new Vector3(13f, 0.08f, 12f), ritual);

        CreateBox("Crouch Tutorial Low Arch", new Vector3(0f, 1.35f, -19f), new Vector3(5f, 0.55f, 0.6f), wall, parent);
        CreateBox("Left Low Arch Post", new Vector3(-2.7f, 0.9f, -19f), new Vector3(0.35f, 1.8f, 0.6f), wall, parent);
        CreateBox("Right Low Arch Post", new Vector3(2.7f, 0.9f, -19f), new Vector3(0.35f, 1.8f, 0.6f), wall, parent);

        CreateGateAndGenerator(parent, wall, interact);
        CreateGraveRows(parent, grave, completedGrave, interact);
        CreateStealthObstacles(parent, wall, grave, danger);
        CreateRitualAltar(parent, ritual, interact);
        CreateWayfindingCandles(parent, interact);
        CreateLab5ThreatsAndRecovery(parent, danger, interact, healing);
        CreateLab6ProgressionPickups(parent, coin, keyFragment, blessedRelic);
    }

    private static void CreateZoneLabel(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject zone = CreateBox(name, position, scale, material, parent);
        zone.GetComponent<Collider>().enabled = false;
    }

    private static void CreateGateAndGenerator(Transform parent, Material wall, Material interact)
    {
        GameObject gate = CreateBox("Locked Chapel Gate", new Vector3(0f, 1.35f, 15.5f), new Vector3(5f, 2.7f, 0.35f), wall, parent);
        SlidingDoor slidingDoor = gate.AddComponent<SlidingDoor>();
        SetSerialized(slidingDoor, "openOffset", new Vector3(0f, 3.2f, 0f));

        GameObject generator = CreateBox("Old Generator Switch", new Vector3(-7f, 0.45f, 9f), new Vector3(1.2f, 0.9f, 1.2f), interact, parent);
        BoxCollider trigger = generator.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(3.5f, 2f, 3.5f);
        trigger.center = new Vector3(0f, 0.75f, 0f);

        Interactable interactable = generator.AddComponent<Interactable>();
        SetSerialized(interactable, "promptText", "Press E to start generator");
        UnityEventTools.AddPersistentListener(interactable.OnInteract, slidingDoor.Toggle);
    }

    private static void CreateGraveRows(Transform parent, Material grave, Material completedGrave, Material interact)
    {
        Vector3[] gravePositions =
        {
            new(-4f, 0.6f, -12f),
            new(0f, 0.6f, -9f),
            new(4f, 0.6f, -6f),
            new(-5f, 0.6f, 3f),
            new(5.5f, 0.6f, 7.5f),
            new(-4f, 0.6f, 20f),
        };

        for (int i = 0; i < gravePositions.Length; i++)
        {
            CreateBox($"Greybox Tombstone {i + 1}", gravePositions[i] + Vector3.forward * 0.75f, new Vector3(0.45f, 1.2f, 0.22f), grave, parent);
            GameObject mound = CreateBox($"Diggable Grave {i + 1}", gravePositions[i] + Vector3.down * 0.35f, new Vector3(1.7f, 0.25f, 2.5f), i < 3 ? interact : grave, parent);

            if (i >= 3)
                continue;

            GameObject remains = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            remains.name = $"Unearthed Remains {i + 1}";
            remains.transform.SetParent(parent);
            remains.transform.position = gravePositions[i] + new Vector3(0f, 0.1f, 0f);
            remains.transform.localScale = new Vector3(0.35f, 0.16f, 0.35f);
            remains.GetComponent<Renderer>().sharedMaterial = completedGrave;
            remains.SetActive(false);

            GraveDigSpot digSpot = mound.AddComponent<GraveDigSpot>();
            SetSerialized(digSpot, "hiddenRemains", remains);
            SetSerialized(digSpot, "completedMaterial", completedGrave);

            BoxCollider trigger = mound.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.5f, 1.4f, 3.2f);
            trigger.center = Vector3.up * 0.7f;

            Interactable interactable = mound.AddComponent<Interactable>();
            SetSerialized(interactable, "promptText", "Press E to dig grave");
            UnityEventTools.AddPersistentListener(interactable.OnInteract, digSpot.Dig);
        }
    }

    private static void CreateStealthObstacles(Transform parent, Material wall, Material grave, Material danger)
    {
        for (int i = 0; i < 6; i++)
        {
            float x = i % 2 == 0 ? -5.5f : 5.5f;
            float z = -1f + i * 3.2f;
            CreateBox($"Broken Stone Cover {i + 1}", new Vector3(x, 0.75f, z), new Vector3(2.6f, 1.5f, 0.55f), wall, parent);
        }

        AddTrap(CreateBox("Trap - Thorn Patch Left", new Vector3(-8f, 0.06f, 13f), new Vector3(2.5f, 0.12f, 4f), danger, parent), 25, 1f);
        AddTrap(CreateBox("Trap - Thorn Patch Right", new Vector3(8f, 0.06f, 16f), new Vector3(2.5f, 0.12f, 4f), danger, parent), 25, 1f);

        CreateBox("Small Mausoleum Blocker", new Vector3(0f, 1.35f, 6f), new Vector3(4.5f, 2.7f, 3f), grave, parent);
        CreateBox("Mausoleum Left Passage Marker", new Vector3(-3.8f, 0.12f, 6f), new Vector3(0.5f, 0.25f, 2.8f), danger, parent).GetComponent<Collider>().enabled = false;
        CreateBox("Mausoleum Right Safer Passage Marker", new Vector3(3.8f, 0.12f, 6f), new Vector3(0.5f, 0.25f, 2.8f), grave, parent).GetComponent<Collider>().enabled = false;
    }

    private static void CreateLab5ThreatsAndRecovery(Transform parent, Material danger, Material checkpointMaterial, Material healing)
    {
        AddTrap(CreateBox("Trap - Ritual Fire Line", new Vector3(0f, 0.08f, 22f), new Vector3(5.2f, 0.16f, 0.7f), danger, parent), 35, 1.2f);
        AddTrap(CreateBox("Trap - Rusted Grave Spikes", new Vector3(3.2f, 0.22f, -2.5f), new Vector3(1.8f, 0.45f, 1.2f), danger, parent), 20, 0.9f);
        AddTrap(CreateBox("Trap - Cursed Mud", new Vector3(-3.6f, 0.08f, 14f), new Vector3(2.8f, 0.16f, 2.8f), danger, parent), 15, 0.8f);

        CreateCheckpoint("Checkpoint - Entrance Candle", new Vector3(0f, 0.45f, -21f), checkpointMaterial, parent);
        CreateCheckpoint("Checkpoint - Chapel Candle", new Vector3(0f, 0.45f, 17.2f), checkpointMaterial, parent);

        CreateHealthPickup("Health Pickup - Old Bandage", new Vector3(5.2f, 0.6f, -13.5f), healing, parent);
        CreateHealthPickup("Health Pickup - Graveyard Medkit", new Vector3(-6.5f, 0.6f, 18.5f), healing, parent);
    }

    private static void AddTrap(GameObject trapObject, int damage, float cooldown)
    {
        Trap trap = trapObject.AddComponent<Trap>();
        SetSerialized(trap, "damage", damage);
        SetSerialized(trap, "cooldown", cooldown);
    }

    private static void CreateCheckpoint(string name, Vector3 position, Material material, Transform parent)
    {
        GameObject checkpoint = CreateBox(name, position, new Vector3(0.7f, 0.9f, 0.7f), material, parent);
        BoxCollider trigger = checkpoint.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(3.5f, 2f, 3.5f);
        trigger.center = Vector3.up * 0.8f;
        Checkpoint checkpointScript = checkpoint.AddComponent<Checkpoint>();
        SetSerialized(checkpointScript, "activatedMaterial", material);
    }

    private static void CreateHealthPickup(string name, Vector3 position, Material material, Transform parent)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = name;
        pickup.transform.SetParent(parent);
        pickup.transform.position = position;
        pickup.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        pickup.GetComponent<Renderer>().sharedMaterial = material;
        pickup.AddComponent<HealthPickup>();
    }

    private static void CreateLab6ProgressionPickups(Transform parent, Material coinMaterial, Material keyMaterial, Material blessedRelicMaterial)
    {
        Vector3[] coinPositions =
        {
            new(-1.8f, 0.55f, -22.2f),
            new(1.8f, 0.55f, -22.2f),
            new(-4.2f, 0.55f, -14.8f),
            new(3.6f, 0.55f, -11.2f),
            new(-6.8f, 0.55f, -2.2f),
            new(6.6f, 0.55f, 1.8f),
            new(-5.4f, 0.55f, 8.8f),
            new(5.8f, 0.55f, 12.2f),
            new(-3.4f, 0.55f, 18.8f),
            new(3.4f, 0.55f, 20.4f),
            new(-1.5f, 0.55f, 25.4f),
            new(1.5f, 0.55f, 25.4f),
        };

        for (int i = 0; i < coinPositions.Length; i++)
            CreatePickup($"Memory Coin {i + 1}", coinPositions[i], Vector3.one * 0.38f, coinMaterial, parent, Pickup.PickupType.Coins, i % 4 == 0 ? 15 : 10, 0);

        CreatePickup("White Key Fragment - Grave Row", new Vector3(0f, 0.6f, -6.8f), new Vector3(0.36f, 0.55f, 0.36f), keyMaterial, parent, Pickup.PickupType.KeyFragment, 1, 0);
        CreatePickup("White Key Fragment - Mausoleum", new Vector3(0f, 0.6f, 6.4f), new Vector3(0.36f, 0.55f, 0.36f), keyMaterial, parent, Pickup.PickupType.KeyFragment, 1, 0);
        CreatePickup("White Key Fragment - Chapel Yard", new Vector3(0f, 0.6f, 23.8f), new Vector3(0.36f, 0.55f, 0.36f), keyMaterial, parent, Pickup.PickupType.KeyFragment, 1, 0);
        CreatePickup("Blessed Relic - Emergency Heal", new Vector3(7.2f, 0.6f, 23.5f), Vector3.one * 0.48f, blessedRelicMaterial, parent, Pickup.PickupType.Healing, 0, 25);
    }

    private static void CreatePickup(string name, Vector3 position, Vector3 scale, Material material, Transform parent, Pickup.PickupType pickupType, int value, int healAmount)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = name;
        pickup.transform.SetParent(parent);
        pickup.transform.position = position;
        pickup.transform.localScale = scale;
        pickup.GetComponent<Renderer>().sharedMaterial = material;

        Pickup pickupScript = pickup.AddComponent<Pickup>();
        SetSerialized(pickupScript, "pickupType", (int)pickupType);
        SetSerialized(pickupScript, "value", value);
        SetSerialized(pickupScript, "healAmount", healAmount);
    }

    private static void CreateRitualAltar(Transform parent, Material ritual, Material interact)
    {
        GameObject altar = CreateBox("White Ritual Altar", new Vector3(0f, 0.55f, 27f), new Vector3(2.4f, 1.1f, 2.4f), ritual, parent);
        RitualAltar ritualAltar = altar.AddComponent<RitualAltar>();

        Light light = new GameObject("Ritual Completion Light").AddComponent<Light>();
        light.transform.SetParent(altar.transform);
        light.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        light.type = LightType.Point;
        light.range = 10f;
        light.color = Color.white;
        light.enabled = false;
        SetSerialized(ritualAltar, "ritualLight", light);

        ParticleSystem particles = new GameObject("White Ritual Particles").AddComponent<ParticleSystem>();
        particles.transform.SetParent(altar.transform);
        particles.transform.localPosition = Vector3.up * 1.2f;
        ParticleSystem.MainModule main = particles.main;
        main.startColor = Color.white;
        main.startLifetime = 1.1f;
        main.startSpeed = 1.5f;
        particles.Stop();
        SetSerialized(ritualAltar, "ritualParticles", particles);

        BoxCollider trigger = altar.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(3.5f, 2.5f, 3.5f);
        trigger.center = Vector3.up * 1.2f;

        Interactable interactable = altar.AddComponent<Interactable>();
        SetSerialized(interactable, "promptText", "Press E to perform white ritual");
        UnityEventTools.AddPersistentListener(interactable.OnInteract, ritualAltar.TryCompleteRitual);

        CreateBox("Ritual Candle 1", new Vector3(-2.2f, 0.45f, 25.6f), new Vector3(0.25f, 0.9f, 0.25f), interact, parent);
        CreateBox("Ritual Candle 2", new Vector3(2.2f, 0.45f, 25.6f), new Vector3(0.25f, 0.9f, 0.25f), interact, parent);
    }

    private static void CreateWayfindingCandles(Transform parent, Material interact)
    {
        Vector3[] positions =
        {
            new(0f, 0.35f, -24f),
            new(-3f, 0.35f, -15f),
            new(4f, 0.35f, -7f),
            new(-7f, 0.35f, 8.5f),
            new(0f, 0.35f, 17f),
            new(0f, 0.35f, 24f),
        };

        foreach (Vector3 position in positions)
        {
            GameObject candle = CreateBox("Yellow Wayfinding Candle", position, new Vector3(0.25f, 0.7f, 0.25f), interact, parent);
            Light light = new GameObject("Candle Light").AddComponent<Light>();
            light.transform.SetParent(candle.transform);
            light.transform.localPosition = Vector3.up * 0.6f;
            light.type = LightType.Point;
            light.range = 4f;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.72f, 0.28f);
        }
    }

    private static GameObject CreatePlayer(Material material)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.05f, -26f);
        player.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.35f;
        controller.center = Vector3.zero;

        GameObject groundCheck = new("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.96f, 0f);

        PlayerController playerController = player.AddComponent<PlayerController>();
        SetSerialized(playerController, "groundCheck", groundCheck.transform);
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerStats>();

        Light flashlight = new GameObject("Player Flashlight").AddComponent<Light>();
        flashlight.transform.SetParent(player.transform);
        flashlight.transform.localPosition = new Vector3(0f, 0.7f, 0.45f);
        flashlight.transform.localRotation = Quaternion.identity;
        flashlight.type = LightType.Spot;
        flashlight.spotAngle = 55f;
        flashlight.range = 12f;
        flashlight.intensity = 4f;
        flashlight.enabled = true;
        SetSerialized(playerController, "flashlight", flashlight);

        return player;
    }

    private static void CreateFirstPersonCamera(Transform player)
    {
        GameObject cameraObject = new("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player);
        cameraObject.transform.localPosition = new Vector3(0f, 0.72f, 0.05f);
        cameraObject.transform.localRotation = Quaternion.identity;

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 72f;
        camera.clearFlags = CameraClearFlags.Skybox;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<FirstPersonCameraLook>();
    }

    private static void CreateUi(GameObject player)
    {
        GameObject managerObject = new("GameManager");
        GameManager manager = managerObject.AddComponent<GameManager>();
        ResourceManager resources = managerObject.AddComponent<ResourceManager>();
        Shop shop = managerObject.AddComponent<Shop>();
        SetSerialized(manager, "RequiredRemains", 3);

        Canvas canvas = new GameObject("Immersive HUD").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        Text scoreText = CreateText(canvas.transform, "ScoreText", "Score: 0", TextAnchor.UpperLeft, new Vector2(18f, -18f), new Vector2(260f, 38f), 20);
        Text coinText = CreateText(canvas.transform, "CoinText", "Coins: 0", TextAnchor.UpperLeft, new Vector2(18f, -48f), new Vector2(260f, 32f), 18);
        Text keyText = CreateText(canvas.transform, "KeyFragmentText", "Key fragments: 0/3", TextAnchor.UpperLeft, new Vector2(18f, -76f), new Vector2(270f, 32f), 18);
        CreateText(canvas.transform, "ShopHintText", "Upgrades: B", TextAnchor.UpperLeft, new Vector2(18f, -104f), new Vector2(270f, 32f), 17);
        Text statusText = CreateText(canvas.transform, "StatusText", "HP: 3/3", TextAnchor.LowerLeft, new Vector2(18f, 18f), new Vector2(260f, 130f), 18);
        Text statsText = CreateText(canvas.transform, "StatsText", "Ritual power: 10", TextAnchor.UpperRight, new Vector2(-18f, -96f), new Vector2(360f, 60f), 17);
        Text objectiveText = CreateText(canvas.transform, "ObjectiveText", "Collect remains: 0/3", TextAnchor.UpperRight, new Vector2(-18f, -18f), new Vector2(360f, 70f), 19);
        Text promptText = CreateText(canvas.transform, "PromptText", string.Empty, TextAnchor.LowerCenter, new Vector2(0f, 54f), new Vector2(420f, 46f), 23);
        Text feedbackText = CreateText(canvas.transform, "ResourceFeedbackText", string.Empty, TextAnchor.UpperLeft, new Vector2(18f, -134f), new Vector2(300f, 36f), 20);
        promptText.enabled = false;
        feedbackText.enabled = false;
        Slider healthSlider = CreateHealthSlider(canvas.transform);
        GameObject gameOverPanel = CreateGameOverPanel(canvas.transform, player.GetComponent<PlayerHealth>());
        GameObject shopPanel = CreateShopPanel(canvas.transform, shop);

        SetSerialized(manager, "scoreText", scoreText);
        SetSerialized(manager, "promptText", promptText);
        SetSerialized(manager, "statusText", statusText);
        SetSerialized(manager, "objectiveText", objectiveText);
        SetSerialized(resources, "coinText", coinText);
        SetSerialized(resources, "keyText", keyText);
        SetSerialized(resources, "feedbackText", feedbackText);

        Text[] shopTexts = shopPanel.GetComponentsInChildren<Text>(true);
        SetSerialized(shop, "shopPanel", shopPanel);
        SetSerialized(shop, "healthButtonText", FindText(shopTexts, "+20 max HP - 50"));
        SetSerialized(shop, "damageButtonText", FindText(shopTexts, "+5 ritual power - 40"));
        SetSerialized(shop, "speedButtonText", FindText(shopTexts, "+0.8 speed - 30"));
        SetSerialized(shop, "statusText", FindText(shopTexts, "Collect coins to buy upgrades"));

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        SetSerialized(playerHealth, "maxHealth", 100);
        SetSerialized(playerHealth, "invincibilityDuration", 1.5f);
        SetSerialized(playerHealth, "healthSlider", healthSlider);
        SetSerialized(playerHealth, "gameOverPanel", gameOverPanel);

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        SetSerialized(playerStats, "statsText", statsText);
    }

    private static Text FindText(Text[] texts, string value)
    {
        foreach (Text text in texts)
        {
            if (text.text == value)
                return text;
        }

        return null;
    }

    private static Slider CreateHealthSlider(Transform parent)
    {
        GameObject sliderObject = new("HealthSlider");
        sliderObject.transform.SetParent(parent, false);
        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        RectTransform rect = slider.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(18f, 156f);
        rect.sizeDelta = new Vector2(220f, 18f);

        GameObject background = new("Background");
        background.transform.SetParent(sliderObject.transform, false);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.12f, 0.02f, 0.02f, 0.85f);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;

        GameObject fillArea = new("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2f, 2f);
        fillAreaRect.offsetMax = new Vector2(-2f, -2f);

        GameObject fill = new("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.75f, 0.05f, 0.04f, 0.95f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        slider.fillRect = fillRect;
        return slider;
    }

    private static GameObject CreateGameOverPanel(Transform parent, PlayerHealth playerHealth)
    {
        GameObject panel = new("GameOverPanel");
        panel.transform.SetParent(parent, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.02f, 0f, 0f, 0.82f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Text title = CreateText(panel.transform, "GameOverTitle", "GAME OVER", TextAnchor.MiddleCenter, new Vector2(0f, 80f), new Vector2(420f, 60f), 38);
        title.color = new Color(0.95f, 0.08f, 0.06f);

        Button restartButton = CreateButton(panel.transform, "RestartButton", "Restart level", new Vector2(0f, 0f));
        Button checkpointButton = CreateButton(panel.transform, "CheckpointButton", "Respawn checkpoint", new Vector2(0f, -56f));
        UnityEventTools.AddPersistentListener(restartButton.onClick, playerHealth.RestartLevel);
        UnityEventTools.AddPersistentListener(checkpointButton.onClick, playerHealth.RespawnAtCheckpoint);

        panel.SetActive(false);
        return panel;
    }

    private static GameObject CreateShopPanel(Transform parent, Shop shop)
    {
        GameObject panel = new("ShopPanel");
        panel.transform.SetParent(parent, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.04f, 0.045f, 0.05f, 0.92f);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-18f, 0f);
        rect.sizeDelta = new Vector2(340f, 270f);

        Text title = CreateText(panel.transform, "ShopTitle", "Upgrade Shop", TextAnchor.UpperLeft, new Vector2(18f, -16f), new Vector2(300f, 34f), 24);
        title.color = new Color(1f, 0.82f, 0.32f);

        Button healthButton = CreateButton(panel.transform, "HealthUpgradeButton", "+20 max HP - 50", new Vector2(0f, 54f));
        Button damageButton = CreateButton(panel.transform, "DamageUpgradeButton", "+5 ritual power - 40", new Vector2(0f, 4f));
        Button speedButton = CreateButton(panel.transform, "SpeedUpgradeButton", "+0.8 speed - 30", new Vector2(0f, -46f));

        UnityEventTools.AddPersistentListener(healthButton.onClick, shop.BuyHealthUpgrade);
        UnityEventTools.AddPersistentListener(damageButton.onClick, shop.BuyDamageUpgrade);
        UnityEventTools.AddPersistentListener(speedButton.onClick, shop.BuySpeedUpgrade);

        Text status = CreateText(panel.transform, "ShopStatusText", "Collect coins to buy upgrades", TextAnchor.LowerCenter, new Vector2(0f, 14f), new Vector2(300f, 34f), 15);
        status.color = new Color(0.78f, 0.9f, 1f);

        return panel;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position)
    {
        GameObject buttonObject = new(name);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(230f, 42f);

        Text text = CreateText(buttonObject.transform, "Text", label, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(220f, 36f), 20);
        text.color = Color.white;
        return button;
    }

    private static void CreateEnemies(Transform player, Material enemyMaterial)
    {
        CreateEnemy("Cemetery Watchman", new Vector3(-6f, 1f, 1f), enemyMaterial, player, true, new[]
        {
            new Vector3(-6f, 0f, -2f),
            new Vector3(-6f, 0f, 9f),
            new Vector3(2f, 0f, 9f),
            new Vector3(5f, 0f, 1f),
        });

        CreateEnemy("Gravedigger Patrol", new Vector3(4f, 1f, 19f), enemyMaterial, player, false, new[]
        {
            new Vector3(4f, 0f, 18f),
            new Vector3(-4f, 0f, 20f),
            new Vector3(-3f, 0f, 27f),
            new Vector3(5f, 0f, 27f),
        });
    }

    private static void CreateEnemy(string name, Vector3 position, Material material, Transform player, bool useBasicEnemyAi, Vector3[] patrolPositions)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.tag = "Enemy";
        enemy.transform.position = position;
        enemy.GetComponent<Renderer>().sharedMaterial = material;
        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
        agent.height = 2f;
        agent.radius = 0.35f;

        Transform[] points = new Transform[patrolPositions.Length];
        for (int i = 0; i < patrolPositions.Length; i++)
        {
            GameObject point = new($"{name} Patrol Point {i + 1}");
            point.transform.position = patrolPositions[i];
            points[i] = point.transform;
        }

        if (useBasicEnemyAi)
        {
            EnemyAI ai = enemy.AddComponent<EnemyAI>();
            SetSerializedArray(ai, "waypoints", points);
            SetSerialized(ai, "chaseRange", 9f);
            SetSerialized(ai, "attackRange", 1.8f);
            SetSerialized(ai, "damage", 20);
            SetSerialized(ai, "attackCooldown", 1.5f);
        }
        else
        {
            EnemyPatrolAI ai = enemy.AddComponent<EnemyPatrolAI>();
            SetSerialized(ai, "player", player);
            SetSerializedArray(ai, "patrolPoints", points);
        }
    }

    private static void EnsureTag(string tag)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets.Length == 0)
            return;

        SerializedObject tagManager = new(assets[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedPropertiesWithoutUndo();
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
        else if (anchor == TextAnchor.UpperRight)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
        }
        else if (anchor == TextAnchor.LowerLeft)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
        }
        else if (anchor == TextAnchor.MiddleCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
        else if (anchor == TextAnchor.LowerCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);
        return text;
    }

    private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        box.transform.position = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        return box;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        const string directory = "Assets/Materials";
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

    private static void SetSerialized(Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.intValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerialized(Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerialized(Object target, string propertyName, Vector3 value)
    {
        SerializedObject serializedObject = new(target);
        serializedObject.FindProperty(propertyName).vector3Value = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerialized(Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new(target);
        serializedObject.FindProperty(propertyName).stringValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedArray(Object target, string propertyName, Transform[] values)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
