using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using MarbleRace.Data;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Marble;
using MarbleRace.Runtime.Track;
using MarbleRace.Runtime.Camera;
using MarbleRace.Runtime.UI;
using MarbleRace.Events;
using UnityEngine.InputSystem.UI;

public class GameSetupWizard : EditorWindow
{
    [MenuItem("MarbleRace/Setup Entire Game Scene")]
    public static void SetupScene()
    {
        if (!EditorUtility.DisplayDialog("MarbleRace Setup",
            "This will set up the entire game scene:\n\n" +
            "- Race track with ramps and obstacles\n" +
            "- 8 marble configs\n" +
            "- Marble prefab\n" +
            "- All managers\n" +
            "- UI canvas with all panels\n" +
            "- Camera\n\n" +
            "Continue?", "Build It!", "Cancel"))
            return;

        // Clean up existing scene objects to avoid duplicates
        CleanupExistingScene();

        CreateFolders();
        var economyConfig = CreateEconomyConfig();
        var raceSettings = CreateRaceSettings();
        var trackData = CreateTrackData();
        var marbleConfigs = CreateMarbleConfigs();
        var events = CreateEvents();
        var marblePrefab = CreateMarblePrefab(raceSettings);

        // Random track selection
        var trackTypes = System.Enum.GetValues(typeof(TrackType));
        var selectedTrack = (TrackType)trackTypes.GetValue(Random.Range(0, trackTypes.Length));
        trackData.trackType = selectedTrack;
        trackData.trackName = selectedTrack.ToString() + " Track";
        EditorUtility.SetDirty(trackData);

        var trackPhysMat = CreateTrackPhysicsMaterial();
        var track = TrackGenerator.GenerateTrack(selectedTrack, trackPhysMat);
        var spawnPoints = CreateSpawnPoints();
        var finishLine = CreateFinishLine(selectedTrack);
        var startGate = CreateStartGate();
        var managers = CreateManagers(economyConfig, raceSettings, trackData, marbleConfigs, marblePrefab, spawnPoints, events);
        var canvas = CreateUI(managers, economyConfig);
        SetupCamera(managers);
        SetupLightingAndAtmosphere();
        SetupPostProcessing();

        EditorUtility.DisplayDialog("Done!",
            "Game scene is ready!\n\n" +
            "Press PLAY to test.\n\n" +
            "Click 'Play' button in the game UI to start a race.",
            "Let's Go!");
    }

    static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Marbles"))
            AssetDatabase.CreateFolder("Assets/Data", "Marbles");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Events"))
            AssetDatabase.CreateFolder("Assets/Data", "Events");
    }

    static EconomyConfig CreateEconomyConfig()
    {
        var config = ScriptableObject.CreateInstance<EconomyConfig>();
        config.startingCoins = 1000;
        config.minimumBet = 10;
        config.maximumBet = 500;
        config.dailyLoginReward = 200;
        config.bailoutAmount = 100;
        config.bailoutCooldownHours = 1f;
        AssetDatabase.CreateAsset(config, "Assets/Data/EconomyConfig.asset");
        return config;
    }

    static RaceSettings CreateRaceSettings()
    {
        var settings = ScriptableObject.CreateInstance<RaceSettings>();
        settings.countdownDuration = 3f;
        settings.bettingDuration = 15f;
        settings.raceTimeout = 60f;
        settings.marbleCount = 8;
        settings.marbleMass = 1f;
        settings.marbleDrag = 0.1f;
        settings.marbleAngularDrag = 0.5f;
        settings.minNudgeInterval = 0.8f;
        settings.maxNudgeInterval = 2.0f;
        settings.minNudgeForce = 0.1f;
        settings.maxNudgeForce = 0.4f;
        settings.lateralNudgeStrength = 0.1f;
        settings.cameraFollowSpeed = 5f;
        settings.cameraLookAhead = 2f;
        AssetDatabase.CreateAsset(settings, "Assets/Data/RaceSettings.asset");
        return settings;
    }

    static TrackData CreateTrackData()
    {
        var data = ScriptableObject.CreateInstance<TrackData>();
        data.trackId = "track_01";
        data.trackName = "Marble Mountain";
        data.checkpointCount = 3;
        data.expectedRaceTime = 30f;
        AssetDatabase.CreateAsset(data, "Assets/Data/TrackData.asset");
        return data;
    }

    static MarbleData[] CreateMarbleConfigs()
    {
        string[] names = { "Crimson", "Cobalt", "Emerald", "Solar", "Violet", "Frost", "Blaze", "Shadow" };
        Color[] colors = {
            new Color(0.9f, 0.1f, 0.1f),  // Red
            new Color(0.1f, 0.2f, 0.9f),  // Blue
            new Color(0.1f, 0.8f, 0.2f),  // Green
            new Color(1f, 0.9f, 0.1f),    // Yellow
            new Color(0.6f, 0.1f, 0.9f),  // Purple
            new Color(0.1f, 0.9f, 0.9f),  // Cyan
            new Color(1f, 0.5f, 0f),      // Orange
            new Color(0.3f, 0.3f, 0.35f)  // Dark Gray
        };
        // Physics personalities: mass, drag, bounce, nudge strength
        // Balanced so no marble dominates — each has a tradeoff
        float[][] personalities = {
            new[] { 1.0f, 0.08f, 0.6f, 1.0f },  // Crimson: balanced all-rounder
            new[] { 1.1f, 0.06f, 0.5f, 1.0f },  // Cobalt: slightly heavy, low drag
            new[] { 0.9f, 0.10f, 0.7f, 1.1f },  // Emerald: light but more drag
            new[] { 1.0f, 0.07f, 0.6f, 1.05f }, // Solar: slight nudge edge
            new[] { 1.1f, 0.09f, 0.5f, 0.95f }, // Violet: heavy, moderate drag
            new[] { 0.9f, 0.09f, 0.7f, 1.0f },  // Frost: light but avg drag/nudge
            new[] { 1.1f, 0.07f, 0.55f, 1.05f }, // Blaze: heavy, low drag, slight nudge
            new[] { 1.2f, 0.06f, 0.5f, 0.95f }, // Shadow: heaviest, low drag compensates
        };

        var configs = new MarbleData[8];
        for (int i = 0; i < 8; i++)
        {
            string path = $"Assets/Data/Marbles/{names[i]}.asset";
            // Delete existing asset so stats are always fresh
            var existing = AssetDatabase.LoadAssetAtPath<MarbleData>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);

            var data = ScriptableObject.CreateInstance<MarbleData>();
            data.marbleId = names[i].ToLower();
            data.marbleName = names[i];
            data.marbleColor = colors[i];
            data.rarity = i < 4 ? MarbleRarity.Common : (i < 6 ? MarbleRarity.Rare : (i < 7 ? MarbleRarity.Epic : MarbleRarity.Legendary));
            data.massMultiplier = personalities[i][0];
            data.dragMultiplier = personalities[i][1];
            data.bounciness = personalities[i][2];
            data.nudgeStrength = personalities[i][3];
            AssetDatabase.CreateAsset(data, path);
            configs[i] = data;
        }
        return configs;
    }

    static EventAssets CreateEvents()
    {
        var assets = new EventAssets();

        assets.onStateChanged = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onStateChanged, "Assets/Data/Events/OnStateChanged.asset");

        assets.onRacePrepared = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onRacePrepared, "Assets/Data/Events/OnRacePrepared.asset");

        assets.onCountdownStarted = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onCountdownStarted, "Assets/Data/Events/OnCountdownStarted.asset");

        assets.onRaceStarted = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onRaceStarted, "Assets/Data/Events/OnRaceStarted.asset");

        assets.onRaceFinished = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onRaceFinished, "Assets/Data/Events/OnRaceFinished.asset");

        assets.onBettingOpened = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onBettingOpened, "Assets/Data/Events/OnBettingOpened.asset");

        assets.onBettingClosed = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onBettingClosed, "Assets/Data/Events/OnBettingClosed.asset");

        assets.onBetPlaced = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onBetPlaced, "Assets/Data/Events/OnBetPlaced.asset");

        assets.onBailoutUsed = ScriptableObject.CreateInstance<GameEvent>();
        AssetDatabase.CreateAsset(assets.onBailoutUsed, "Assets/Data/Events/OnBailoutUsed.asset");

        assets.onCoinsChanged = ScriptableObject.CreateInstance<IntEvent>();
        AssetDatabase.CreateAsset(assets.onCoinsChanged, "Assets/Data/Events/OnCoinsChanged.asset");

        assets.onCountdownTick = ScriptableObject.CreateInstance<IntEvent>();
        AssetDatabase.CreateAsset(assets.onCountdownTick, "Assets/Data/Events/OnCountdownTick.asset");

        assets.onPayoutReceived = ScriptableObject.CreateInstance<IntEvent>();
        AssetDatabase.CreateAsset(assets.onPayoutReceived, "Assets/Data/Events/OnPayoutReceived.asset");

        assets.onMarbleFinished = ScriptableObject.CreateInstance<MarbleEvent>();
        AssetDatabase.CreateAsset(assets.onMarbleFinished, "Assets/Data/Events/OnMarbleFinished.asset");

        return assets;
    }

    static GameObject CreateMarblePrefab(RaceSettings raceSettings)
    {
        var marble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marble.name = "Marble";
        marble.tag = "Marble";
        marble.transform.localScale = Vector3.one * 0.5f;

        // Shiny metallic marble material
        var marbleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        marbleMat.SetFloat("_Metallic", 0.8f);
        marbleMat.SetFloat("_Smoothness", 0.9f);
        marble.GetComponent<Renderer>().material = marbleMat;
        AssetDatabase.CreateAsset(marbleMat, "Assets/Data/MarbleMaterial.mat");

        // Rigidbody
        var rb = marble.GetComponent<Rigidbody>();
        if (rb == null) rb = marble.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Physics material
        var physMat = new PhysicsMaterial("MarblePhysics");
        physMat.bounciness = 0.6f;
        physMat.dynamicFriction = 0.3f;
        physMat.staticFriction = 0.3f;
        physMat.bounceCombine = PhysicsMaterialCombine.Average;
        AssetDatabase.CreateAsset(physMat, "Assets/Data/MarblePhysics.physicMaterial");
        marble.GetComponent<SphereCollider>().material = physMat;


        // Scripts
        var controller = marble.AddComponent<MarbleController>();
        marble.AddComponent<MarbleIdentity>();

        // Save prefab
        var prefab = PrefabUtility.SaveAsPrefabAsset(marble, "Assets/Prefabs/Marble.prefab");
        Object.DestroyImmediate(marble);
        return prefab;
    }

    static void SetupPostProcessing()
    {
        var mainCam = UnityEngine.Camera.main;
        if (mainCam != null)
            PostProcessingSetup.Setup(mainCam);
    }

    static Material CreateTrackMaterial(string name, Color color, float metallic = 0.2f, float smoothness = 0.6f)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }

    static PhysicsMaterial CreateTrackPhysicsMaterial()
    {
        var trackPhysMat = new PhysicsMaterial("TrackPhysics");
        trackPhysMat.bounciness = 0.3f;
        trackPhysMat.dynamicFriction = 0.4f;
        trackPhysMat.staticFriction = 0.5f;
        AssetDatabase.CreateAsset(trackPhysMat, "Assets/Data/TrackPhysics.physicMaterial");
        return trackPhysMat;
    }

    static Transform[] CreateSpawnPoints()
    {
        var spawnParent = new GameObject("SpawnPoints");
        spawnParent.transform.position = Vector3.zero;

        // Single row of 8 marbles evenly spaced — all start at the same z for fairness
        float z = 1.0f;
        float surfaceY = TrackGenerator.GetStartSurfaceY();
        float trackWidth = 4.0f; // usable width inside 5-unit track with walls
        float spacing = trackWidth / 7f; // 7 gaps between 8 marbles
        float startX = -trackWidth / 2f;

        var points = new Transform[8];
        for (int i = 0; i < 8; i++)
        {
            var point = new GameObject($"SpawnPoint_{i}");
            float x = startX + i * spacing;
            point.transform.position = new Vector3(x, surfaceY + 0.4f, z);
            point.transform.parent = spawnParent.transform;
            points[i] = point.transform;
        }

        return points;
    }

    static GameObject CreateFinishLine(TrackType trackType)
    {
        Vector3 finishPos = TrackGenerator.GetFinishLinePosition(trackType);

        var finishObj = new GameObject("FinishLine");
        finishObj.transform.position = finishPos;

        var col = finishObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(10, 12, 10);

        finishObj.AddComponent<FinishLine>();

        // Visual marker
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "FinishVisual";
        visual.transform.parent = finishObj.transform;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(6, 0.1f, 0.3f);
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());

        var renderer = visual.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.yellow;
        mat.SetFloat("_Metallic", 0.7f);
        mat.SetFloat("_Smoothness", 0.9f);
        renderer.material = mat;

        return finishObj;
    }

    static GameObject CreateStartGate()
    {
        var gateObj = new GameObject("StartGate");
        Vector3 gatePos = TrackGenerator.GetStartGatePosition(TrackType.Downhill);
        gateObj.transform.position = gatePos;

        var gateVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gateVisual.name = "GateVisual";
        gateVisual.transform.parent = gateObj.transform;
        gateVisual.transform.localPosition = Vector3.zero;
        gateVisual.transform.localScale = new Vector3(5, 1.5f, 0.2f);

        var renderer = gateVisual.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.8f, 0.1f, 0.1f);
        renderer.material = mat;

        var gate = gateObj.AddComponent<StartGate>();
        // Use SerializedObject to set private fields
        var so = new SerializedObject(gate);
        so.FindProperty("gateVisual").objectReferenceValue = gateVisual;
        so.FindProperty("gateCollider").objectReferenceValue = gateVisual.GetComponent<BoxCollider>();
        so.ApplyModifiedProperties();

        return gateObj;
    }

    struct ManagerRefs
    {
        public GameManager gameManager;
        public RaceManager raceManager;
        public BettingManager bettingManager;
        public EconomyManager economyManager;
        public AudioManager audioManager;
        public UIManager uiManager;
        public MarbleSpawner marbleSpawner;
    }

    static ManagerRefs CreateManagers(EconomyConfig economyConfig, RaceSettings raceSettings, TrackData trackData,
        MarbleData[] marbleConfigs, GameObject marblePrefab, Transform[] spawnPoints, EventAssets events)
    {
        var refs = new ManagerRefs();

        // --- GameManager ---
        var gmObj = new GameObject("GameManager");
        refs.gameManager = gmObj.AddComponent<GameManager>();

        // --- RaceManager ---
        var rmObj = new GameObject("RaceManager");
        refs.raceManager = rmObj.AddComponent<RaceManager>();

        // --- MarbleSpawner ---
        var msObj = new GameObject("MarbleSpawner");
        refs.marbleSpawner = msObj.AddComponent<MarbleSpawner>();

        // Wire MarbleSpawner
        var msSO = new SerializedObject(refs.marbleSpawner);
        msSO.FindProperty("raceSettings").objectReferenceValue = raceSettings;
        msSO.FindProperty("marblePrefab").objectReferenceValue = marblePrefab;

        var marbleConfigsProp = msSO.FindProperty("marbleConfigs");
        marbleConfigsProp.arraySize = marbleConfigs.Length;
        for (int i = 0; i < marbleConfigs.Length; i++)
            marbleConfigsProp.GetArrayElementAtIndex(i).objectReferenceValue = marbleConfigs[i];

        var spawnPointsProp = msSO.FindProperty("spawnPoints");
        spawnPointsProp.arraySize = spawnPoints.Length;
        for (int i = 0; i < spawnPoints.Length; i++)
            spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
        msSO.ApplyModifiedProperties();

        // Wire RaceManager
        var rmSO = new SerializedObject(refs.raceManager);
        rmSO.FindProperty("raceSettings").objectReferenceValue = raceSettings;
        rmSO.FindProperty("currentTrack").objectReferenceValue = trackData;
        rmSO.FindProperty("marbleSpawner").objectReferenceValue = refs.marbleSpawner;
        // Wire the start gate
        var startGateComp = Object.FindAnyObjectByType<StartGate>();
        if (startGateComp != null)
            rmSO.FindProperty("startGate").objectReferenceValue = startGateComp;
        // raceHUD will be wired after UI is created
        rmSO.FindProperty("onRacePrepared").objectReferenceValue = events.onRacePrepared;
        rmSO.FindProperty("onCountdownStarted").objectReferenceValue = events.onCountdownStarted;
        rmSO.FindProperty("onRaceStarted").objectReferenceValue = events.onRaceStarted;
        rmSO.FindProperty("onRaceFinished").objectReferenceValue = events.onRaceFinished;
        rmSO.FindProperty("onMarbleFinished").objectReferenceValue = events.onMarbleFinished;
        rmSO.FindProperty("onCountdownTick").objectReferenceValue = events.onCountdownTick;
        rmSO.ApplyModifiedProperties();

        // --- BettingManager ---
        var bmObj = new GameObject("BettingManager");
        refs.bettingManager = bmObj.AddComponent<BettingManager>();
        var bmSO = new SerializedObject(refs.bettingManager);
        bmSO.FindProperty("economyConfig").objectReferenceValue = economyConfig;
        bmSO.FindProperty("onBettingOpened").objectReferenceValue = events.onBettingOpened;
        bmSO.FindProperty("onBettingClosed").objectReferenceValue = events.onBettingClosed;
        bmSO.FindProperty("onBetPlaced").objectReferenceValue = events.onBetPlaced;
        bmSO.FindProperty("onPayoutReceived").objectReferenceValue = events.onPayoutReceived;
        bmSO.ApplyModifiedProperties();

        // --- EconomyManager ---
        var emObj = new GameObject("EconomyManager");
        refs.economyManager = emObj.AddComponent<EconomyManager>();
        var emSO = new SerializedObject(refs.economyManager);
        emSO.FindProperty("config").objectReferenceValue = economyConfig;
        emSO.FindProperty("onCoinsChanged").objectReferenceValue = events.onCoinsChanged;
        emSO.FindProperty("onBailoutUsed").objectReferenceValue = events.onBailoutUsed;
        emSO.ApplyModifiedProperties();

        // --- AudioManager ---
        var amObj = new GameObject("AudioManager");
        refs.audioManager = amObj.AddComponent<AudioManager>();
        var sfxSource = amObj.AddComponent<AudioSource>();
        var musicSource = amObj.AddComponent<AudioSource>();
        var crowdSource = amObj.AddComponent<AudioSource>();
        var amSO = new SerializedObject(refs.audioManager);
        amSO.FindProperty("sfxSource").objectReferenceValue = sfxSource;
        amSO.FindProperty("musicSource").objectReferenceValue = musicSource;
        amSO.FindProperty("crowdSource").objectReferenceValue = crowdSource;
        amSO.ApplyModifiedProperties();

        // Wire GameManager
        var gmSO = new SerializedObject(refs.gameManager);
        gmSO.FindProperty("onStateChanged").objectReferenceValue = events.onStateChanged;
        gmSO.FindProperty("raceManager").objectReferenceValue = refs.raceManager;
        gmSO.FindProperty("bettingManager").objectReferenceValue = refs.bettingManager;
        gmSO.FindProperty("economyManager").objectReferenceValue = refs.economyManager;
        // uiManager will be wired after UI is created
        gmSO.ApplyModifiedProperties();

        // --- RaceStatsManager ---
        var statsObj = new GameObject("RaceStatsManager");
        statsObj.AddComponent<RaceStatsManager>();

        // Wire FinishLine
        var finishLine = Object.FindAnyObjectByType<FinishLine>();
        if (finishLine != null)
        {
            var flSO = new SerializedObject(finishLine);
            flSO.FindProperty("raceManager").objectReferenceValue = refs.raceManager;
            flSO.ApplyModifiedProperties();
        }

        // Wire finishLine reference on RaceManager
        if (finishLine != null)
        {
            var rmSO2 = new SerializedObject(refs.raceManager);
            rmSO2.FindProperty("finishLine").objectReferenceValue = finishLine;
            rmSO2.ApplyModifiedProperties();
        }

        return refs;
    }

    static GameObject CreateUI(ManagerRefs managers, EconomyConfig economyConfig)
    {
        // Create Canvas
        var canvasObj = new GameObject("GameCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Safe area container for mobile notches
        var safeArea = new GameObject("SafeArea");
        var safeRT = safeArea.AddComponent<RectTransform>();
        safeRT.SetParent(canvasObj.transform, false);
        safeRT.anchorMin = Vector2.zero;
        safeRT.anchorMax = Vector2.one;
        safeRT.sizeDelta = Vector2.zero;
        safeArea.AddComponent<SafeAreaHandler>();

        // --- Main Menu Panel ---
        var mainMenu = CreatePanel(canvasObj.transform, "MainMenuPanel", new Color(0.05f, 0.05f, 0.1f, 1f));
        CreateText(mainMenu.transform, "Title", "DONKEY MARBLE RACING", 48, new Vector2(0, 300), Color.white);
        CreateText(mainMenu.transform, "Subtitle", "Place your bets!", 24, new Vector2(0, 220), Color.gray);
        var coinDisplay = CreateText(mainMenu.transform, "CoinBalance", "1000 coins", 32, new Vector2(0, 140), Color.yellow);
        var playBtn = CreateButton(mainMenu.transform, "PlayButton", "RACE!", new Vector2(0, -50), new Vector2(400, 80),
            new Color(0.1f, 0.7f, 0.2f));
        var dailyBtn = CreateButton(mainMenu.transform, "DailyRewardButton", "Daily Reward", new Vector2(0, -160), new Vector2(300, 60),
            new Color(0.9f, 0.6f, 0.1f));
        var statsBtn = CreateButton(mainMenu.transform, "StatsButton", "STATS", new Vector2(0, -260), new Vector2(300, 60),
            new Color(0.3f, 0.5f, 0.9f));
        var settingsBtn = CreateButton(mainMenu.transform, "SettingsButton", "SETTINGS", new Vector2(0, -350), new Vector2(300, 60),
            new Color(0.4f, 0.4f, 0.5f));

        // Add animations and button feedback to main menu
        mainMenu.AddComponent<PanelAnimator>();
        playBtn.AddComponent<ButtonFeedback>();
        dailyBtn.AddComponent<ButtonFeedback>();
        statsBtn.AddComponent<ButtonFeedback>();
        settingsBtn.AddComponent<ButtonFeedback>();

        var mainMenuScript = mainMenu.AddComponent<MainMenuPanel>();
        var mmSO = new SerializedObject(mainMenuScript);
        mmSO.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<Button>();
        mmSO.FindProperty("dailyRewardButton").objectReferenceValue = dailyBtn.GetComponent<Button>();
        mmSO.FindProperty("coinBalanceText").objectReferenceValue = coinDisplay.GetComponent<TMP_Text>();
        mmSO.FindProperty("economyManager").objectReferenceValue = managers.economyManager;
        mmSO.ApplyModifiedProperties();

        // --- Betting Panel ---
        var betting = CreatePanel(canvasObj.transform, "BettingPanel", new Color(0.05f, 0.05f, 0.1f, 1f));
        betting.SetActive(false);
        CreateText(betting.transform, "BettingTitle", "PICK YOUR MARBLE", 36, new Vector2(0, 400), Color.white);
        var selectedText = CreateText(betting.transform, "SelectedMarble", "Select a marble", 24, new Vector2(0, 100), Color.cyan);
        var oddsText = CreateText(betting.transform, "OddsText", "Odds: --", 22, new Vector2(0, 60), Color.yellow);
        var betAmountText = CreateText(betting.transform, "BetAmount", "Bet: 0", 22, new Vector2(0, 20), Color.white);
        var balanceText = CreateText(betting.transform, "Balance", "1000 coins", 20, new Vector2(0, -20), Color.gray);

        // Bet slider
        var sliderObj = CreateSlider(betting.transform, "BetSlider", new Vector2(0, -80), new Vector2(400, 30));

        // Marble buttons container
        var marbleContainer = new GameObject("MarbleButtons");
        var marbleContainerRT = marbleContainer.AddComponent<RectTransform>();
        marbleContainerRT.SetParent(betting.transform, false);
        marbleContainerRT.anchoredPosition = new Vector2(0, 280);
        marbleContainerRT.sizeDelta = new Vector2(500, 200);
        var gridLayout = marbleContainer.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(70, 70);
        gridLayout.spacing = new Vector2(12, 12);
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4;

        // Marble button prefab — circular colored button (no text)
        var marbleBtnPrefab = CreateButton(canvasObj.transform, "MarbleButtonPrefab", "", Vector2.zero,
            new Vector2(70, 70), new Color(0.3f, 0.3f, 0.4f));
        var mbPrefab = PrefabUtility.SaveAsPrefabAsset(marbleBtnPrefab, "Assets/Prefabs/MarbleButton.prefab");
        Object.DestroyImmediate(marbleBtnPrefab);

        // Quick bet buttons
        var quickBetContainer = new GameObject("QuickBets");
        var qbRT = quickBetContainer.AddComponent<RectTransform>();
        qbRT.SetParent(betting.transform, false);
        qbRT.anchoredPosition = new Vector2(0, -140);
        qbRT.sizeDelta = new Vector2(500, 50);
        var qbLayout = quickBetContainer.AddComponent<HorizontalLayoutGroup>();
        qbLayout.spacing = 20;
        qbLayout.childAlignment = TextAnchor.MiddleCenter;

        var qb1 = CreateButton(quickBetContainer.transform, "QuickBet25", "25%", Vector2.zero, new Vector2(100, 40),
            new Color(0.2f, 0.4f, 0.6f));
        var qb2 = CreateButton(quickBetContainer.transform, "QuickBet50", "50%", Vector2.zero, new Vector2(100, 40),
            new Color(0.2f, 0.4f, 0.6f));
        var qb3 = CreateButton(quickBetContainer.transform, "QuickBetAll", "ALL IN", Vector2.zero, new Vector2(100, 40),
            new Color(0.7f, 0.2f, 0.2f));

        var confirmBtn = CreateButton(betting.transform, "ConfirmBet", "PLACE BET", new Vector2(0, -220), new Vector2(300, 70),
            new Color(0.1f, 0.7f, 0.2f));

        var skipBtn = CreateButton(betting.transform, "SkipBet", "SKIP - JUST WATCH", new Vector2(0, -310), new Vector2(250, 50),
            new Color(0.4f, 0.4f, 0.5f));

        betting.AddComponent<PanelAnimator>();
        confirmBtn.AddComponent<ButtonFeedback>();
        var bettingScript = betting.AddComponent<BettingPanel>();
        var bpSO = new SerializedObject(bettingScript);
        bpSO.FindProperty("bettingManager").objectReferenceValue = managers.bettingManager;
        bpSO.FindProperty("economyManager").objectReferenceValue = managers.economyManager;
        bpSO.FindProperty("raceManager").objectReferenceValue = managers.raceManager;
        bpSO.FindProperty("marbleButtonContainer").objectReferenceValue = marbleContainerRT;
        bpSO.FindProperty("marbleButtonPrefab").objectReferenceValue = mbPrefab;
        bpSO.FindProperty("selectedMarbleText").objectReferenceValue = selectedText.GetComponent<TMP_Text>();
        bpSO.FindProperty("oddsText").objectReferenceValue = oddsText.GetComponent<TMP_Text>();
        bpSO.FindProperty("betAmountText").objectReferenceValue = betAmountText.GetComponent<TMP_Text>();
        bpSO.FindProperty("balanceText").objectReferenceValue = balanceText.GetComponent<TMP_Text>();
        bpSO.FindProperty("betSlider").objectReferenceValue = sliderObj.GetComponent<Slider>();
        bpSO.FindProperty("confirmBetButton").objectReferenceValue = confirmBtn.GetComponent<Button>();
        bpSO.FindProperty("skipBetButton").objectReferenceValue = skipBtn.GetComponent<Button>();

        var quickBetsProp = bpSO.FindProperty("quickBetButtons");
        quickBetsProp.arraySize = 3;
        quickBetsProp.GetArrayElementAtIndex(0).objectReferenceValue = qb1.GetComponent<Button>();
        quickBetsProp.GetArrayElementAtIndex(1).objectReferenceValue = qb2.GetComponent<Button>();
        quickBetsProp.GetArrayElementAtIndex(2).objectReferenceValue = qb3.GetComponent<Button>();
        bpSO.ApplyModifiedProperties();

        // --- Race HUD ---
        var raceHud = CreatePanel(canvasObj.transform, "RaceHUD", new Color(0, 0, 0, 0));
        raceHud.SetActive(false);
        var timerText = CreateText(raceHud.transform, "Timer", "0.0s", 28, new Vector2(0, 420), Color.white);
        var positionsText = CreateText(raceHud.transform, "Positions", "", 20, new Vector2(-380, 300), Color.white);
        positionsText.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.TopLeft;
        var betIndicator = CreateText(raceHud.transform, "BetIndicator", "", 18, new Vector2(0, -400), Color.cyan);

        var countdownPanel = new GameObject("CountdownPanel");
        var cdRT = countdownPanel.AddComponent<RectTransform>();
        cdRT.SetParent(raceHud.transform, false);
        cdRT.sizeDelta = new Vector2(300, 300);
        var countdownText = CreateText(countdownPanel.transform, "CountdownNumber", "3", 120, Vector2.zero, Color.white);

        var hudScript = raceHud.AddComponent<RaceHUD>();
        var hudSO = new SerializedObject(hudScript);
        hudSO.FindProperty("raceManager").objectReferenceValue = managers.raceManager;
        hudSO.FindProperty("timerText").objectReferenceValue = timerText.GetComponent<TMP_Text>();
        hudSO.FindProperty("positionsText").objectReferenceValue = positionsText.GetComponent<TMP_Text>();
        hudSO.FindProperty("playerBetIndicator").objectReferenceValue = betIndicator.GetComponent<TMP_Text>();
        hudSO.FindProperty("countdownText").objectReferenceValue = countdownText.GetComponent<TMP_Text>();
        hudSO.FindProperty("countdownPanel").objectReferenceValue = countdownPanel;
        hudSO.ApplyModifiedProperties();

        // --- Results Panel ---
        var results = CreatePanel(canvasObj.transform, "ResultsPanel", new Color(0.05f, 0.05f, 0.1f, 1f));
        results.SetActive(false);
        var winnerText = CreateText(results.transform, "Winner", "Winner!", 48, new Vector2(0, 350), Color.yellow);
        var resultMsg = CreateText(results.transform, "ResultMessage", "", 36, new Vector2(0, 280), Color.white);
        var payoutText = CreateText(results.transform, "Payout", "", 28, new Vector2(0, 220), Color.green);
        var raceTimeText = CreateText(results.transform, "RaceTime", "", 20, new Vector2(0, 180), Color.gray);
        var newBalText = CreateText(results.transform, "NewBalance", "", 24, new Vector2(0, 130), Color.yellow);

        var finishOrderContainer = new GameObject("FinishOrder");
        var foRT = finishOrderContainer.AddComponent<RectTransform>();
        foRT.SetParent(results.transform, false);
        foRT.anchoredPosition = new Vector2(0, 0);
        foRT.sizeDelta = new Vector2(400, 200);
        var foLayout = finishOrderContainer.AddComponent<VerticalLayoutGroup>();
        foLayout.spacing = 5;
        foLayout.childAlignment = TextAnchor.UpperCenter;

        var finishEntryPrefab = new GameObject("FinishEntry");
        var feRT = finishEntryPrefab.AddComponent<RectTransform>();
        feRT.sizeDelta = new Vector2(400, 30);
        var feText = finishEntryPrefab.AddComponent<TextMeshProUGUI>();
        feText.fontSize = 18;
        feText.color = Color.white;
        feText.alignment = TextAlignmentOptions.Center;
        var fePrefab = PrefabUtility.SaveAsPrefabAsset(finishEntryPrefab, "Assets/Prefabs/UI/FinishEntry.prefab");
        Object.DestroyImmediate(finishEntryPrefab);

        var playAgainBtn = CreateButton(results.transform, "PlayAgainButton", "RACE AGAIN", new Vector2(0, -250), new Vector2(300, 70),
            new Color(0.1f, 0.7f, 0.2f));
        var menuBtn = CreateButton(results.transform, "MainMenuButton", "MAIN MENU", new Vector2(0, -340), new Vector2(250, 50),
            new Color(0.4f, 0.4f, 0.5f));

        results.AddComponent<PanelAnimator>();
        playAgainBtn.AddComponent<ButtonFeedback>();
        menuBtn.AddComponent<ButtonFeedback>();
        var resultsScript = results.AddComponent<ResultsPanel>();
        var rpSO = new SerializedObject(resultsScript);
        rpSO.FindProperty("raceManager").objectReferenceValue = managers.raceManager;
        rpSO.FindProperty("bettingManager").objectReferenceValue = managers.bettingManager;
        rpSO.FindProperty("economyManager").objectReferenceValue = managers.economyManager;
        rpSO.FindProperty("winnerText").objectReferenceValue = winnerText.GetComponent<TMP_Text>();
        rpSO.FindProperty("resultMessage").objectReferenceValue = resultMsg.GetComponent<TMP_Text>();
        rpSO.FindProperty("payoutText").objectReferenceValue = payoutText.GetComponent<TMP_Text>();
        rpSO.FindProperty("raceTimeText").objectReferenceValue = raceTimeText.GetComponent<TMP_Text>();
        rpSO.FindProperty("newBalanceText").objectReferenceValue = newBalText.GetComponent<TMP_Text>();
        rpSO.FindProperty("finishOrderContainer").objectReferenceValue = foRT;
        rpSO.FindProperty("finishOrderEntryPrefab").objectReferenceValue = fePrefab;
        rpSO.FindProperty("playAgainButton").objectReferenceValue = playAgainBtn.GetComponent<Button>();
        rpSO.FindProperty("mainMenuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        rpSO.ApplyModifiedProperties();

        // --- Stats Panel ---
        var statsPanel = CreatePanel(canvasObj.transform, "StatsPanel", new Color(0.03f, 0.03f, 0.08f, 0.97f));
        statsPanel.SetActive(false);
        var statsTitleText = CreateText(statsPanel.transform, "StatsTitle", "STATISTICS", 42, new Vector2(0, 400), Color.white);
        var generalStatsText = CreateText(statsPanel.transform, "GeneralStats", "", 22, new Vector2(-200, 100), Color.white);
        generalStatsText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 600);
        var marbleRankingsText = CreateText(statsPanel.transform, "MarbleRankings", "", 20, new Vector2(200, 100), Color.white);
        marbleRankingsText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 600);
        var closeStatsBtn = CreateButton(statsPanel.transform, "CloseStatsButton", "CLOSE", new Vector2(0, -420), new Vector2(250, 60),
            new Color(0.5f, 0.2f, 0.2f));

        var statsPanelScript = statsPanel.AddComponent<StatsPanel>();
        var spSO = new SerializedObject(statsPanelScript);
        spSO.FindProperty("statsText").objectReferenceValue = generalStatsText.GetComponent<TMP_Text>();
        spSO.FindProperty("marbleStatsText").objectReferenceValue = marbleRankingsText.GetComponent<TMP_Text>();
        spSO.FindProperty("closeButton").objectReferenceValue = closeStatsBtn.GetComponent<Button>();
        spSO.ApplyModifiedProperties();

        // Wire stats button to show stats panel
        statsBtn.GetComponent<Button>().onClick.AddListener(() => statsPanelScript.Show());

        // --- Settings Panel ---
        var settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", new Color(0.03f, 0.03f, 0.08f, 0.97f));
        settingsPanel.SetActive(false);
        CreateText(settingsPanel.transform, "SettingsTitle", "SETTINGS", 42, new Vector2(0, 400), Color.white);

        // Sound toggle
        CreateText(settingsPanel.transform, "SoundLabel", "Sound", 24, new Vector2(-100, 200), Color.white);
        var soundToggleObj = new GameObject("SoundToggle");
        var soundToggleRT = soundToggleObj.AddComponent<RectTransform>();
        soundToggleRT.SetParent(settingsPanel.transform, false);
        soundToggleRT.anchoredPosition = new Vector2(150, 200);
        soundToggleRT.sizeDelta = new Vector2(80, 40);
        var soundBg = soundToggleObj.AddComponent<Image>();
        soundBg.color = new Color(0.2f, 0.5f, 0.3f);
        var soundToggle = soundToggleObj.AddComponent<Toggle>();
        soundToggle.isOn = true;

        // Particles toggle
        CreateText(settingsPanel.transform, "ParticlesLabel", "Particles", 24, new Vector2(-100, 120), Color.white);
        var particlesToggleObj = new GameObject("ParticlesToggle");
        var particlesToggleRT = particlesToggleObj.AddComponent<RectTransform>();
        particlesToggleRT.SetParent(settingsPanel.transform, false);
        particlesToggleRT.anchoredPosition = new Vector2(150, 120);
        particlesToggleRT.sizeDelta = new Vector2(80, 40);
        var particlesBg = particlesToggleObj.AddComponent<Image>();
        particlesBg.color = new Color(0.2f, 0.5f, 0.3f);
        var particlesToggle = particlesToggleObj.AddComponent<Toggle>();
        particlesToggle.isOn = true;

        // Quality slider
        CreateText(settingsPanel.transform, "QualityLabel", "Quality", 24, new Vector2(-100, 40), Color.white);
        var qualityLabel = CreateText(settingsPanel.transform, "QualityValue", "HIGH", 24, new Vector2(150, 40), Color.cyan);
        var qualitySlider = CreateSlider(settingsPanel.transform, "QualitySlider", new Vector2(0, -30), new Vector2(350, 30));

        // Reset stats button
        var resetBtn = CreateButton(settingsPanel.transform, "ResetStatsButton", "RESET STATS", new Vector2(0, -150), new Vector2(250, 50),
            new Color(0.7f, 0.2f, 0.2f));
        var closeSettingsBtn = CreateButton(settingsPanel.transform, "CloseSettingsButton", "CLOSE", new Vector2(0, -420), new Vector2(250, 60),
            new Color(0.5f, 0.2f, 0.2f));

        var settingsPanelScript = settingsPanel.AddComponent<SettingsPanel>();
        var stSO = new SerializedObject(settingsPanelScript);
        stSO.FindProperty("soundToggle").objectReferenceValue = soundToggle;
        stSO.FindProperty("particlesToggle").objectReferenceValue = particlesToggle;
        stSO.FindProperty("qualitySlider").objectReferenceValue = qualitySlider.GetComponent<Slider>();
        stSO.FindProperty("qualityLabel").objectReferenceValue = qualityLabel.GetComponent<TMP_Text>();
        stSO.FindProperty("closeButton").objectReferenceValue = closeSettingsBtn.GetComponent<Button>();
        stSO.FindProperty("resetStatsButton").objectReferenceValue = resetBtn.GetComponent<Button>();
        stSO.ApplyModifiedProperties();

        // Wire settings button
        settingsBtn.GetComponent<Button>().onClick.AddListener(() => settingsPanelScript.Show());

        // --- Countdown Overlay ---
        var countdownOverlay = new GameObject("CountdownOverlay");
        var coRT = countdownOverlay.AddComponent<RectTransform>();
        coRT.SetParent(canvasObj.transform, false);
        coRT.anchorMin = Vector2.zero;
        coRT.anchorMax = Vector2.one;
        coRT.sizeDelta = Vector2.zero;
        countdownOverlay.SetActive(false);

        // --- UIManager ---
        var uiManagerObj = new GameObject("UIManager");
        refs_uiManager = uiManagerObj.AddComponent<UIManager>();
        var umSO = new SerializedObject(refs_uiManager);
        umSO.FindProperty("mainMenuPanel").objectReferenceValue = mainMenu;
        umSO.FindProperty("bettingPanel").objectReferenceValue = betting;
        umSO.FindProperty("raceHUD").objectReferenceValue = raceHud;
        umSO.FindProperty("resultsPanel").objectReferenceValue = results;
        umSO.FindProperty("countdownOverlay").objectReferenceValue = countdownOverlay;
        umSO.FindProperty("gameManager").objectReferenceValue = managers.gameManager;
        umSO.ApplyModifiedProperties();

        // EventSystem with new Input System module
        var existingES = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (existingES != null)
            Object.DestroyImmediate(existingES.gameObject);

        var eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<InputSystemUIInputModule>();

        managers.uiManager = refs_uiManager;

        // Wire UIManager to GameManager now that it exists
        var gmSO2 = new SerializedObject(managers.gameManager);
        gmSO2.FindProperty("uiManager").objectReferenceValue = refs_uiManager;
        gmSO2.ApplyModifiedProperties();

        // Wire RaceHUD to RaceManager now that it exists
        var rmSO2 = new SerializedObject(managers.raceManager);
        rmSO2.FindProperty("raceHUD").objectReferenceValue = hudScript;
        rmSO2.ApplyModifiedProperties();

        return canvasObj;
    }

    static UIManager refs_uiManager;

    static void SetupCamera(ManagerRefs managers)
    {
        // Destroy any existing cameras to avoid duplicates
        var existingCams = Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None);
        foreach (var c in existingCams)
            Object.DestroyImmediate(c.gameObject);

        var camObj = new GameObject("Main Camera");
        var mainCam = camObj.AddComponent<UnityEngine.Camera>();
        camObj.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();

        mainCam.transform.position = new Vector3(0f, 9f, -5f);
        mainCam.transform.rotation = Quaternion.Euler(40f, 0f, 0f);
        mainCam.fieldOfView = 65f;

        // Higher quality rendering
        mainCam.allowMSAA = true;
        mainCam.allowHDR = true;

        // Set project quality to highest level
        QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
        QualitySettings.antiAliasing = 4; // 4x MSAA
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowDistance = 100f;
        QualitySettings.pixelLightCount = 4;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;

        var raceCamera = camObj.AddComponent<RaceCamera>();

        // Wire RaceCamera to RaceManager
        var rmSO = new SerializedObject(managers.raceManager);
        rmSO.FindProperty("raceCamera").objectReferenceValue = raceCamera;
        rmSO.ApplyModifiedProperties();

        // Wire RaceCamera to FinishLine for celebration
        var fl = Object.FindAnyObjectByType<FinishLine>();
        if (fl != null)
        {
            var flSO = new SerializedObject(fl);
            flSO.FindProperty("raceCamera").objectReferenceValue = raceCamera;
            flSO.ApplyModifiedProperties();
        }
    }

    static void CreateHazards(TrackType trackType)
    {
        var hazardsParent = new GameObject("Hazards");

        // Place different hazards based on track type
        switch (trackType)
        {
            case TrackType.Downhill:
                CreateBumper(hazardsParent.transform, new Vector3(0, -2.5f, 15), 3f);
                CreateBumper(hazardsParent.transform, new Vector3(1.5f, -6f, 26), 2.5f);
                CreateBoostPad(hazardsParent.transform, new Vector3(0, -7.8f, 31), 5f);
                CreateSpinner(hazardsParent.transform, new Vector3(0, -4f, 19), 60f);
                break;
            case TrackType.Zigzag:
                CreateBumper(hazardsParent.transform, new Vector3(0, -3.5f, 14), 4f);
                CreateBumper(hazardsParent.transform, new Vector3(-1f, -6f, 24), 3f);
                CreateBumper(hazardsParent.transform, new Vector3(1f, -8.5f, 34), 3.5f);
                CreateBoostPad(hazardsParent.transform, new Vector3(0, -10.5f, 42), 6f);
                break;
            case TrackType.Funnel:
                CreateSpinner(hazardsParent.transform, new Vector3(0, -3.5f, 14), 80f);
                CreateBumper(hazardsParent.transform, new Vector3(0, -6f, 22), 4f);
                CreateSpinner(hazardsParent.transform, new Vector3(0, -8f, 29), 90f);
                break;
            case TrackType.Spiral:
                CreateBumper(hazardsParent.transform, new Vector3(0, -5f, 18), 3f);
                CreateBumper(hazardsParent.transform, new Vector3(0, -9f, 32), 3.5f);
                CreateBoostPad(hazardsParent.transform, new Vector3(0, -7f, 25), 4f);
                CreateSpinner(hazardsParent.transform, new Vector3(0, -11f, 39), 70f);
                break;
            case TrackType.MultiPath:
                CreateBumper(hazardsParent.transform, new Vector3(-2.5f, -5f, 20), 3f);
                CreateBumper(hazardsParent.transform, new Vector3(2f, -4f, 20), 2.5f);
                CreateBoostPad(hazardsParent.transform, new Vector3(0, -8f, 32), 5f);
                CreateSpinner(hazardsParent.transform, new Vector3(0, -6f, 28), 75f);
                break;
        }
    }

    static void CreateBumper(Transform parent, Vector3 position, float force)
    {
        var bumper = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bumper.name = "Bumper";
        bumper.transform.parent = parent;
        bumper.transform.position = position;
        bumper.transform.localScale = new Vector3(1f, 0.5f, 1f);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(1f, 0.3f, 0.1f);
        mat.SetFloat("_Metallic", 0.6f);
        mat.SetFloat("_Smoothness", 0.8f);
        bumper.GetComponent<Renderer>().material = mat;

        var hazard = bumper.AddComponent<TrackHazard>();
        var so = new SerializedObject(hazard);
        so.FindProperty("hazardType").enumValueIndex = 1;
        so.FindProperty("forceStrength").floatValue = force;
        so.ApplyModifiedProperties();
    }

    static void CreateBoostPad(Transform parent, Vector3 position, float force)
    {
        var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = "BoostPad";
        pad.transform.parent = parent;
        pad.transform.position = position;
        pad.transform.localScale = new Vector3(3, 0.1f, 2);
        pad.GetComponent<BoxCollider>().isTrigger = true;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.1f, 0.9f, 0.3f);
        mat.SetFloat("_Smoothness", 0.9f);
        mat.SetFloat("_Metallic", 0.5f);
        pad.GetComponent<Renderer>().material = mat;

        var hazard = pad.AddComponent<TrackHazard>();
        var so = new SerializedObject(hazard);
        so.FindProperty("hazardType").enumValueIndex = 0;
        so.FindProperty("forceStrength").floatValue = force;
        so.FindProperty("forceDirection").vector3Value = Vector3.forward;
        so.ApplyModifiedProperties();
    }

    static void CreateSpinner(Transform parent, Vector3 position, float spinSpeed)
    {
        var spinner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spinner.name = "Spinner";
        spinner.transform.parent = parent;
        spinner.transform.position = position;
        spinner.transform.localScale = new Vector3(2.5f, 0.3f, 0.4f);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.9f, 0.7f, 0.1f);
        mat.SetFloat("_Metallic", 0.7f);
        mat.SetFloat("_Smoothness", 0.85f);
        spinner.GetComponent<Renderer>().material = mat;

        var hazard = spinner.AddComponent<TrackHazard>();
        var so = new SerializedObject(hazard);
        so.FindProperty("hazardType").enumValueIndex = 2; // Spinner
        so.FindProperty("forceStrength").floatValue = 4f;
        so.FindProperty("spinSpeed").floatValue = spinSpeed;
        so.ApplyModifiedProperties();
    }

    static void SetupLightingAndAtmosphere()
    {
        // Remove default directional light if exists
        var existingLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in existingLights)
            Object.DestroyImmediate(l.gameObject);

        // Main directional light (sun)
        var sunObj = new GameObject("Sun Light");
        var sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.95f, 0.85f);
        sun.intensity = 1.5f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.8f;
        sun.shadowBias = 0.02f;
        sun.shadowNormalBias = 0.3f;
        sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Accent light (colored fill from below)
        var fillObj = new GameObject("Fill Light");
        var fill = fillObj.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.color = new Color(0.4f, 0.5f, 0.8f);
        fill.intensity = 0.4f;
        fill.shadows = LightShadows.None;
        fillObj.transform.rotation = Quaternion.Euler(-20f, 60f, 0f);

        // Point lights along the track for dramatic effect
        Color[] spotColors = {
            new Color(0.2f, 1f, 0.3f),   // Green at start
            new Color(0.3f, 0.5f, 1f),   // Blue at mid
            new Color(1f, 0.3f, 0.2f),   // Red at funnel
            new Color(1f, 0.9f, 0.2f),   // Gold at finish
        };
        Vector3[] spotPositions = {
            new Vector3(0, 5f, 5f),
            new Vector3(0, 2f, 27f),
            new Vector3(0, -1f, 53f),
            new Vector3(0, -4f, 78f),
        };

        for (int i = 0; i < spotColors.Length; i++)
        {
            var lightObj = new GameObject($"TrackLight_{i}");
            var pl = lightObj.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = spotColors[i];
            pl.intensity = 3f;
            pl.range = 15f;
            pl.shadows = LightShadows.None;
            lightObj.transform.position = spotPositions[i];
        }

        // Set ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.2f);

        // Procedural skybox — dark gradient with purple/blue tones
        var skyMat = new Material(Shader.Find("Skybox/Procedural"));
        if (skyMat != null)
        {
            skyMat.SetFloat("_SunSize", 0.02f);
            skyMat.SetFloat("_SunSizeConvergence", 10f);
            skyMat.SetFloat("_AtmosphereThickness", 0.8f);
            skyMat.SetColor("_SkyTint", new Color(0.1f, 0.05f, 0.2f));
            skyMat.SetColor("_GroundColor", new Color(0.02f, 0.02f, 0.05f));
            skyMat.SetFloat("_Exposure", 0.5f);
            RenderSettings.skybox = skyMat;
            AssetDatabase.CreateAsset(skyMat, "Assets/Data/Skybox.mat");
        }

        // Add fog for depth
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.1f);
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 120f;
    }

    // --- UI Helper Methods ---

    static GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        var panel = new GameObject(name);
        var rt = panel.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        var image = panel.AddComponent<Image>();
        image.color = bgColor;

        return panel;
    }

    static GameObject CreateText(Transform parent, string name, string content, int fontSize, Vector2 position, Color color)
    {
        var textObj = new GameObject(name);
        var rt = textObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(600, 80);

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return textObj;
    }

    static GameObject CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color)
    {
        var btnObj = new GameObject(name);
        var rt = btnObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        var image = btnObj.AddComponent<Image>();
        image.color = color;

        var button = btnObj.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        button.colors = colors;

        var textObj = new GameObject("Text");
        var textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(btnObj.transform, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return btnObj;
    }

    static GameObject CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var sliderObj = new GameObject(name);
        var rt = sliderObj.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Background
        var bg = new GameObject("Background");
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.SetParent(sliderObj.transform, false);
        bgRT.anchorMin = new Vector2(0, 0.25f);
        bgRT.anchorMax = new Vector2(1, 0.75f);
        bgRT.sizeDelta = Vector2.zero;
        var bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.25f);

        // Fill area
        var fillArea = new GameObject("Fill Area");
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.SetParent(sliderObj.transform, false);
        faRT.anchorMin = new Vector2(0, 0.25f);
        faRT.anchorMax = new Vector2(1, 0.75f);
        faRT.sizeDelta = Vector2.zero;

        var fill = new GameObject("Fill");
        var fillRT = fill.AddComponent<RectTransform>();
        fillRT.SetParent(fillArea.transform, false);
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.6f, 0.9f);

        // Handle
        var handleArea = new GameObject("Handle Slide Area");
        var haRT = handleArea.AddComponent<RectTransform>();
        haRT.SetParent(sliderObj.transform, false);
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.sizeDelta = Vector2.zero;

        var handle = new GameObject("Handle");
        var handleRT = handle.AddComponent<RectTransform>();
        handleRT.SetParent(handleArea.transform, false);
        handleRT.sizeDelta = new Vector2(20, 30);
        var handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        var slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0.5f;

        return sliderObj;
    }

    static void CleanupExistingScene()
    {
        // Destroy known game objects to prevent duplicates
        string[] objectNames = new string[]
        {
            "Track", "SpawnPoints", "FinishLine", "StartGate",
            "GameManager", "RaceManager", "MarbleSpawner", "BettingManager",
            "EconomyManager", "AudioManager", "UIManager",
            "GameCanvas", "EventSystem", "Main Camera",
            "Hazards", "Sun Light", "Fill Light",
            "TrackLight_0", "TrackLight_1", "TrackLight_2", "TrackLight_3",
            "PostProcessVolume", "RaceStatsManager"
        };

        foreach (var name in objectNames)
        {
            var existing = GameObject.Find(name);
            while (existing != null)
            {
                Object.DestroyImmediate(existing);
                existing = GameObject.Find(name);
            }
        }
    }

    struct EventAssets
    {
        public GameEvent onStateChanged;
        public GameEvent onRacePrepared;
        public GameEvent onCountdownStarted;
        public GameEvent onRaceStarted;
        public GameEvent onRaceFinished;
        public GameEvent onBettingOpened;
        public GameEvent onBettingClosed;
        public GameEvent onBetPlaced;
        public GameEvent onBailoutUsed;
        public IntEvent onCoinsChanged;
        public IntEvent onCountdownTick;
        public IntEvent onPayoutReceived;
        public MarbleEvent onMarbleFinished;
    }
}
