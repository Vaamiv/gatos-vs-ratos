using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GatosVsRatos
{
    public sealed class GameApp : MonoBehaviour
    {
        public static GameApp Instance { get; private set; }
        public GamePhase Phase { get; private set; }
        public AudioKit Audio { get; private set; }
        public IReadOnlyList<Enemy> Enemies => enemies;
        public Difficulty CurrentDifficulty => difficulty;

        private readonly List<Enemy> enemies = new();
        private readonly Dictionary<TowerKind, Image> buildButtonImages = new();
        private readonly List<Vector3> path = new();

        private Camera gameCamera;
        private Canvas canvas;
        private RectTransform uiRoot;
        private GameObject worldRoot;
        private GameObject selectionPanel;
        private Text resourceText;
        private Text baseText;
        private Text timeText;
        private Text waveText;
        private Text defeatedText;
        private Text selectionTitle;
        private Text selectionStats;
        private Text upgradeButtonText;
        private Text sellButtonText;
        private Text musicToggleText;
        private Button upgradeButton;
        private Text toastText;
        private Coroutine toastRoutine;

        private TowerKind selectedKind = TowerKind.Metralhadora;
        private Tower selectedTower;
        private Difficulty difficulty;
        private StageDefinition activeStage;
        private int selectedStage;
        private int currency;
        private int baseHealth;
        private int maxBaseHealth;
        private int defeated;
        private int currentWave;
        private int totalWaves;
        private float timeRemaining;
        private bool wavesFinished;
        private bool waveInProgress;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<GameApp>() == null)
                new GameObject("GatosVsRatos").AddComponent<GameApp>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Application.targetFrameRate = 60;
            CreateCamera();
            CreateCanvas();
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = 0.72f;
            var musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.volume = 0.28f;
            Audio = new AudioKit(source, musicSource);
            ShowMenu();
        }

        private void Update()
        {
            if (Phase != GamePhase.Playing) return;

            if (waveInProgress)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0f)
                {
                    timeRemaining = 0f;
                    Defeat("O tempo da missão acabou!");
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectBuildKind(TowerKind.Metralhadora);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectBuildKind(TowerKind.Bazuca);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectBuildKind(TowerKind.Catapulta);
            if (Input.GetKeyDown(KeyCode.E) && selectedTower != null) UpgradeSelected();
            UpdateHud();
        }

        private void CreateCamera()
        {
            gameCamera = Camera.main;
            if (gameCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                gameCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }
            gameCamera.transform.position = new Vector3(0, 0, -10f);
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = 5.625f;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color32(117, 185, 105, 255);
        }

        private void CreateCanvas()
        {
            var canvasObject = new GameObject("UI");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            uiRoot = new GameObject("ScreenRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            uiRoot.SetParent(canvas.transform, false);
            Stretch(uiRoot);

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("EventSystem");
                events.AddComponent<EventSystem>();
                events.AddComponent<StandaloneInputModule>();
            }
        }

        public void ShowMenu()
        {
            StopAllCoroutines();
            Phase = GamePhase.Menu;
            enemies.Clear();
            selectedTower = null;
            if (worldRoot != null) Destroy(worldRoot);
            Clear(uiRoot);
            gameCamera.backgroundColor = new Color32(42, 91, 82, 255);
            Audio.PlayMenuMusic();

            Texture2D background = Resources.Load<Texture2D>("MenuBackground");
            var backdrop = NewUI<RawImage>("MenuBackground", uiRoot);
            Stretch(backdrop.rectTransform);
            backdrop.texture = background;
            backdrop.color = background == null ? new Color32(64, 125, 102, 255) : Color.white;
            var fitter = backdrop.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = background == null ? 16f / 9f : background.width / (float)background.height;

            var shade = Panel("Shade", uiRoot, new Color(0.03f, 0.08f, 0.08f, 0.34f));
            Stretch(shade.rectTransform);
            var card = Panel("MenuCard", uiRoot, new Color(0.055f, 0.105f, 0.105f, 0.91f));
            SetAnchors(card.rectTransform, new Vector2(0.045f, 0.075f), new Vector2(0.42f, 0.925f));

            Text title = Label("Title", card.transform, "GATOS\nVS RATOS", 86, FontStyle.Bold, new Color32(255, 221, 83, 255));
            Place(title.rectTransform, new Vector2(0.5f, 0.79f), new Vector2(620, 230), Vector2.zero);
            title.alignment = TextAnchor.MiddleCenter;
            Text subtitle = Label("Subtitle", card.transform, "DEFENDA A DESPENSA!", 28, FontStyle.Bold, new Color32(226, 244, 229, 255));
            Place(subtitle.rectTransform, new Vector2(0.5f, 0.64f), new Vector2(570, 55), Vector2.zero);

            Text choose = Label("Choose", card.transform, "Campanha com 5 fases", 25, FontStyle.Normal, new Color32(206, 220, 213, 255));
            Place(choose.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(500, 45), Vector2.zero);

            Button map = UiButton("Campaign", card.transform, "ABRIR MAPA", new Color32(68, 171, 112, 255), ShowWorldMap);
            Place(map.GetComponent<RectTransform>(), new Vector2(0.5f, 0.455f), new Vector2(500, 78), Vector2.zero);
            Button help = UiButton("Help", card.transform, "COMO JOGAR", new Color32(58, 112, 130, 255), ShowInstructions);
            Place(help.GetComponent<RectTransform>(), new Vector2(0.5f, 0.35f), new Vector2(500, 66), Vector2.zero);
            Button music = UiButton("Music", card.transform, Audio.MusicEnabled ? "MÚSICA: LIGADA" : "MÚSICA: DESLIGADA", new Color32(86, 105, 121, 255), ToggleMusic);
            Place(music.GetComponent<RectTransform>(), new Vector2(0.5f, 0.255f), new Vector2(500, 58), Vector2.zero);
            musicToggleText = music.GetComponentInChildren<Text>();
            Button exit = UiButton("Exit", card.transform, "SAIR", new Color32(72, 80, 83, 255), Application.Quit);
            Place(exit.GetComponent<RectTransform>(), new Vector2(0.5f, 0.17f), new Vector2(500, 54), Vector2.zero);

            int record = PlayerPrefs.GetInt("GVR_BestKills", 0);
            int wins = PlayerPrefs.GetInt("GVR_Wins", 0);
            int medals = Campaign.ClearedCount();
            Text ranking = Label("Ranking", card.transform, $"MEDALHAS {medals}/15  •  Recorde: {record} ratos  •  Vitórias: {wins}", 19, FontStyle.Normal, new Color32(185, 205, 196, 255));
            Place(ranking.rectTransform, new Vector2(0.5f, 0.075f), new Vector2(590, 44), Vector2.zero);
        }

        public void ShowWorldMap()
        {
            StopAllCoroutines();
            Phase = GamePhase.Map;
            enemies.Clear();
            selectedTower = null;
            if (worldRoot != null) Destroy(worldRoot);
            Clear(uiRoot);
            Audio.PlayMenuMusic();

            Texture2D background = Resources.Load<Texture2D>("MenuBackground");
            var backdrop = NewUI<RawImage>("MapBackground", uiRoot);
            Stretch(backdrop.rectTransform);
            backdrop.texture = background;
            backdrop.color = background == null ? new Color32(64, 125, 102, 255) : new Color(0.62f, 0.72f, 0.67f, 1f);
            var fitter = backdrop.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = background == null ? 16f / 9f : background.width / (float)background.height;

            var shade = Panel("MapShade", uiRoot, new Color(0.02f, 0.055f, 0.055f, 0.63f));
            Stretch(shade.rectTransform);
            var board = Panel("CampaignBoard", uiRoot, new Color(0.045f, 0.085f, 0.082f, 0.96f));
            Place(board.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1760, 930), Vector2.zero);

            Text title = Label("MapTitle", board.transform, "MAPA DA CAMPANHA", 52, FontStyle.Bold, new Color32(255, 220, 78, 255));
            Place(title.rectTransform, new Vector2(0.5f, 0.925f), new Vector2(900, 76), Vector2.zero);
            Text subtitle = Label("MapSubtitle", board.transform, "Conquiste Normal, Difícil e Insano em cada fase • progresso salvo automaticamente", 23, FontStyle.Normal, new Color32(199, 219, 208, 255));
            Place(subtitle.rectTransform, new Vector2(0.5f, 0.86f), new Vector2(1200, 46), Vector2.zero);

            Button back = UiButton("Back", board.transform, "VOLTAR", new Color32(75, 94, 98, 255), ShowMenu);
            Place(back.GetComponent<RectTransform>(), new Vector2(0.075f, 0.925f), new Vector2(190, 58), Vector2.zero);
            Button music = UiButton("Music", board.transform, Audio.MusicEnabled ? "MÚSICA: ON" : "MÚSICA: OFF", new Color32(75, 94, 108, 255), ToggleMusic);
            Place(music.GetComponent<RectTransform>(), new Vector2(0.925f, 0.925f), new Vector2(210, 58), Vector2.zero);
            musicToggleText = music.GetComponentInChildren<Text>();

            Vector2[] nodePositions =
            {
                new(-650, -150), new(-330, 135), new(0, -125), new(330, 145), new(650, -105)
            };
            for (int i = 0; i < nodePositions.Length - 1; i++)
                UiLinePixels(board.transform, nodePositions[i] + new Vector2(0, 15), nodePositions[i + 1] + new Vector2(0, 15), 15f, new Color32(172, 137, 65, 255));

            int unlocked = Campaign.UnlockedStage;
            for (int i = 0; i < Campaign.Stages.Length; i++)
            {
                int stageIndex = i;
                StageDefinition stage = Campaign.Stages[i];
                bool available = i <= unlocked;
                bool normalClear = Campaign.IsCleared(i, Difficulty.Normal);
                bool hardClear = Campaign.IsCleared(i, Difficulty.Dificil);
                bool insaneClear = Campaign.IsCleared(i, Difficulty.Insano);
                string medals = available
                    ? $"NORMAL {(normalClear ? "✓" : "○")}  •  DIFÍCIL {(hardClear ? "★" : "○")}\nINSANO {(insaneClear ? "◆" : "○")}" 
                    : "BLOQUEADA";
                Color nodeColor = available ? stage.AccentColor : new Color32(73, 81, 82, 255);
                Button node = UiButton("Stage" + (i + 1), board.transform,
                    $"FASE {i + 1}\n{stage.Name.ToUpper()}\n{medals}", nodeColor, () => ShowStageDetails(stageIndex));
                Place(node.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(280, 185), nodePositions[i]);
                node.interactable = available;
                node.GetComponentInChildren<Text>().fontSize = 18;

                Text marker = Label("Marker", board.transform, (i + 1).ToString(), 38, FontStyle.Bold, Color.white);
                Place(marker.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(62, 62), nodePositions[i] + new Vector2(0, 108));
                var markerBack = Panel("MarkerBack", board.transform, available ? stage.AccentColor : new Color32(75, 80, 81, 255));
                Place(markerBack.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(62, 62), nodePositions[i] + new Vector2(0, 108));
                marker.transform.SetAsLastSibling();
            }

            Text footer = Label("MapFooter", board.transform,
                $"Fases liberadas: {Campaign.UnlockedStage + 1}/5    •    Medalhas conquistadas: {Campaign.ClearedCount()}/15",
                23, FontStyle.Bold, new Color32(220, 230, 222, 255));
            Place(footer.rectTransform, new Vector2(0.5f, 0.075f), new Vector2(1000, 50), Vector2.zero);
        }

        private void ShowStageDetails(int stageIndex)
        {
            if (stageIndex > Campaign.UnlockedStage) return;
            StageDefinition stage = Campaign.Stages[stageIndex];
            var shade = Panel("StageShade", uiRoot, new Color(0.01f, 0.025f, 0.025f, 0.82f));
            Stretch(shade.rectTransform);
            var modal = Panel("StageDetails", shade.transform, new Color32(34, 61, 60, 255));
            Place(modal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(900, 900), Vector2.zero);

            Text number = Label("StageNumber", modal.transform, $"FASE {stageIndex + 1}", 25, FontStyle.Bold, stage.AccentColor);
            Place(number.rectTransform, new Vector2(0.5f, 0.925f), new Vector2(600, 45), Vector2.zero);
            Text title = Label("StageTitle", modal.transform, stage.Name.ToUpper(), 45, FontStyle.Bold, Color.white);
            Place(title.rectTransform, new Vector2(0.5f, 0.84f), new Vector2(740, 70), Vector2.zero);
            Text description = Label("StageDescription", modal.transform, stage.Description, 24, FontStyle.Normal, new Color32(205, 222, 213, 255));
            Place(description.rectTransform, new Vector2(0.5f, 0.75f), new Vector2(710, 70), Vector2.zero);

            bool normalClear = Campaign.IsCleared(stageIndex, Difficulty.Normal);
            Button normal = UiButton("Normal", modal.transform,
                $"NORMAL • 10 ONDAS {(normalClear ? "• CONCLUÍDO ✓" : "")}", new Color32(68, 171, 112, 255), () => StartGame(stageIndex, Difficulty.Normal));
            Place(normal.GetComponent<RectTransform>(), new Vector2(0.5f, 0.60f), new Vector2(650, 76), Vector2.zero);
            Text normalInfo = Label("NormalInfo", modal.transform, "Hordas graduais • economia equilibrada • 7 a 8 minutos", 19, FontStyle.Normal, new Color32(177, 203, 190, 255));
            Place(normalInfo.rectTransform, new Vector2(0.5f, 0.545f), new Vector2(700, 32), Vector2.zero);

            bool hardClear = Campaign.IsCleared(stageIndex, Difficulty.Dificil);
            Button hard = UiButton("Hard", modal.transform,
                $"DIFÍCIL • 15 ONDAS {(hardClear ? "• CONCLUÍDO ★" : "")}", new Color32(211, 110, 67, 255), () => StartGame(stageIndex, Difficulty.Dificil));
            Place(hard.GetComponent<RectTransform>(), new Vector2(0.5f, 0.42f), new Vector2(650, 76), Vector2.zero);
            Text hardInfo = Label("HardInfo", modal.transform, "Mais ratos • menos recompensas • melhorias mais caras", 19, FontStyle.Normal, new Color32(218, 190, 178, 255));
            Place(hardInfo.rectTransform, new Vector2(0.5f, 0.365f), new Vector2(700, 32), Vector2.zero);

            bool insaneClear = Campaign.IsCleared(stageIndex, Difficulty.Insano);
            Button insane = UiButton("Insane", modal.transform,
                $"INSANO • 20 ONDAS {(insaneClear ? "• CONCLUÍDO ◆" : "")}", new Color32(164, 64, 120, 255), () => StartGame(stageIndex, Difficulty.Insano));
            Place(insane.GetComponent<RectTransform>(), new Vector2(0.5f, 0.24f), new Vector2(650, 76), Vector2.zero);
            Text insaneInfo = Label("InsaneInfo", modal.transform, "Hordas imensas • economia severa • ratos causam mais dano", 19, FontStyle.Normal, new Color32(229, 178, 211, 255));
            Place(insaneInfo.rectTransform, new Vector2(0.5f, 0.185f), new Vector2(730, 32), Vector2.zero);

            Button close = UiButton("Close", modal.transform, "FECHAR", new Color32(76, 96, 99, 255), () => Destroy(shade.gameObject));
            Place(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0.075f), new Vector2(330, 52), Vector2.zero);
        }

        private void ToggleMusic()
        {
            bool enabled = Audio.ToggleMusic();
            if (musicToggleText != null) musicToggleText.text = enabled ? "MÚSICA: LIGADA" : "MÚSICA: DESLIGADA";
        }

        private void StartGame(int stageIndex, Difficulty selectedDifficulty)
        {
            StopAllCoroutines();
            Audio.Click();
            Clear(uiRoot);
            if (worldRoot != null) Destroy(worldRoot);
            enemies.Clear();
            buildButtonImages.Clear();
            selectedTower = null;
            selectedKind = TowerKind.Metralhadora;
            selectedStage = Mathf.Clamp(stageIndex, 0, Campaign.Stages.Length - 1);
            activeStage = Campaign.Stages[selectedStage];
            difficulty = selectedDifficulty;
            Phase = GamePhase.Playing;
            currency = difficulty switch
            {
                Difficulty.Dificil => 275,
                Difficulty.Insano => 250,
                _ => 300
            };
            maxBaseHealth = difficulty switch
            {
                Difficulty.Dificil => 20,
                Difficulty.Insano => 16,
                _ => 25
            };
            baseHealth = maxBaseHealth;
            timeRemaining = difficulty switch
            {
                Difficulty.Dificil => 600f + selectedStage * 25f,
                Difficulty.Insano => 780f + selectedStage * 30f,
                _ => 420f + selectedStage * 20f
            };
            defeated = 0;
            currentWave = 0;
            totalWaves = difficulty switch
            {
                Difficulty.Dificil => 15,
                Difficulty.Insano => 20,
                _ => 10
            };
            wavesFinished = false;
            waveInProgress = false;
            gameCamera.backgroundColor = activeStage.GroundColor;
            Audio.PlayBattleMusic();

            CreateWorld();
            CreateGameUi();
            UpdateBuildButtons();
            UpdateHud();
            ShowToast($"FASE {selectedStage + 1}: {activeStage.Name} • Prepare suas defesas!", 3.2f);
            StartCoroutine(RunWaves());
        }

        private void CreateWorld()
        {
            worldRoot = new GameObject("World");
            ArtFactory.SpriteObject("Ground", worldRoot.transform, ArtFactory.Square, activeStage.GroundColor, new Vector2(20.5f, 11.5f), Vector3.zero, -20);

            path.Clear();
            path.AddRange(activeStage.Path);
            ArtFactory.Line("PathOutline", worldRoot.transform, path, 1.34f, activeStage.PathOutlineColor, -8);
            ArtFactory.Line("Path", worldRoot.transform, path, 0.98f, activeStage.PathColor, -7);
            DrawPathStones();
            CreateDecorations();
            CreateBase();

            foreach (Vector3 position in activeStage.TowerSpots)
            {
                var spot = new GameObject("Ponto de Torre");
                spot.transform.SetParent(worldRoot.transform, false);
                spot.transform.position = position;
                spot.AddComponent<TowerSpot>();
            }

            Vector3 entryLabel = path[0] + new Vector3(1.15f, -0.72f, 0);
            ArtFactory.WorldText("Entry", worldRoot.transform, "ENTRADA", entryLabel, 36, 0.052f, 20).color = Color.Lerp(activeStage.PathOutlineColor, Color.black, 0.25f);
            ArtFactory.WorldText("StageName", worldRoot.transform, $"FASE {selectedStage + 1} • {activeStage.Name.ToUpper()}", new Vector3(0, 4.65f), 36, 0.049f, 20).color = new Color(1f, 1f, 1f, 0.72f);
        }

        private void DrawPathStones()
        {
            for (int s = 0; s < path.Count - 1; s++)
            {
                Vector3 a = path[s];
                Vector3 b = path[s + 1];
                float distance = Vector3.Distance(a, b);
                int count = Mathf.FloorToInt(distance / 0.72f);
                for (int i = 1; i < count; i++)
                {
                    if ((i + s) % 2 != 0) continue;
                    Vector3 p = Vector3.Lerp(a, b, i / (float)count);
                    Vector3 normal = Vector3.Cross((b - a).normalized, Vector3.forward) * ((i % 3 - 1) * 0.22f);
                    ArtFactory.SpriteObject("Pebble", worldRoot.transform, ArtFactory.Circle, new Color(0.56f, 0.42f, 0.28f, 0.35f), new Vector2(0.18f, 0.1f), p + normal, -6);
                }
            }
        }

        private void CreateDecorations()
        {
            Vector3[] bushes = { new(-9.1f, 3.7f), new(-8.4f, 3.3f), new(-1f, 3.9f), new(7.2f, 3.55f), new(8.2f, 3.75f), new(8.6f, -3.5f), new(-8.9f, -3.8f) };
            Color bushDark = Color.Lerp(activeStage.GroundColor, new Color32(25, 105, 65, 255), 0.62f);
            Color bushLight = Color.Lerp(activeStage.GroundColor, new Color32(107, 190, 91, 255), 0.58f);
            foreach (Vector3 p in bushes)
            {
                ArtFactory.SpriteObject("Bush", worldRoot.transform, ArtFactory.Circle, bushDark, new Vector2(1.3f, 0.8f), p, -4);
                ArtFactory.SpriteObject("BushLight", worldRoot.transform, ArtFactory.Circle, bushLight, new Vector2(0.65f, 0.5f), p + new Vector3(-0.2f, 0.16f), -3);
            }
            Vector3[] flowers = { new(-7.9f, 3.3f), new(-1.4f, 3.55f), new(3f, -3.6f), new(7.65f, 3.35f), new(8.2f, -3.25f), new(-9.1f, -3.45f) };
            Color[] colors = { activeStage.AccentColor, new Color32(255, 224, 91, 255), new Color32(132, 188, 255, 255) };
            for (int i = 0; i < flowers.Length; i++)
            {
                Vector3 p = flowers[i];
                for (int petal = 0; petal < 5; petal++)
                {
                    float angle = petal * Mathf.PI * 2f / 5f;
                    ArtFactory.SpriteObject("Petal", worldRoot.transform, ArtFactory.Circle, colors[i % colors.Length], new Vector2(0.14f, 0.14f), p + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.1f, -2);
                }
                ArtFactory.SpriteObject("FlowerCenter", worldRoot.transform, ArtFactory.Circle, new Color32(255, 231, 95, 255), new Vector2(0.1f, 0.1f), p, -1);
            }

            CreateStageProps();
        }

        private void CreateStageProps()
        {
            if (selectedStage == 1)
            {
                Vector3[] vegetables = { new(-9.0f, 0.8f), new(-2.6f, 3.75f), new(3.7f, -3.5f), new(7.3f, 2.7f) };
                for (int i = 0; i < vegetables.Length; i++)
                {
                    Color crop = i % 2 == 0 ? new Color32(232, 105, 57, 255) : new Color32(178, 57, 72, 255);
                    ArtFactory.SpriteObject("Vegetable", worldRoot.transform, ArtFactory.Circle, crop, new Vector2(0.34f, 0.34f), vegetables[i], -2);
                    ArtFactory.SpriteObject("Leaves", worldRoot.transform, ArtFactory.Triangle, new Color32(61, 135, 64, 255), new Vector2(0.28f, 0.32f), vegetables[i] + new Vector3(0, 0.25f), -1);
                }
            }
            else if (selectedStage == 2)
            {
                Vector3[] ponds = { new(-9.0f, -3.75f), new(8.45f, -3.55f), new(-0.4f, 4.0f) };
                foreach (Vector3 pond in ponds)
                {
                    ArtFactory.SpriteObject("Pond", worldRoot.transform, ArtFactory.Circle, new Color32(71, 154, 188, 255), new Vector2(2.15f, 0.9f), pond, -12);
                    ArtFactory.SpriteObject("PondLight", worldRoot.transform, ArtFactory.Circle, new Color(0.55f, 0.88f, 0.95f, 0.45f), new Vector2(0.7f, 0.16f), pond + new Vector3(-0.35f, 0.08f), -11);
                }
            }
            else if (selectedStage == 3)
            {
                for (int x = -8; x <= 8; x += 4)
                {
                    ArtFactory.SpriteObject("RoofTile", worldRoot.transform, ArtFactory.Square, new Color(0.26f, 0.31f, 0.35f, 0.28f), new Vector2(2.1f, 0.55f), new Vector3(x, -4.05f), -13);
                    ArtFactory.SpriteObject("RoofTile", worldRoot.transform, ArtFactory.Square, new Color(0.25f, 0.29f, 0.34f, 0.24f), new Vector2(2.1f, 0.55f), new Vector3(x + 1.4f, 4.05f), -13);
                }
            }
            else if (selectedStage == 4)
            {
                Vector3[] blocks = { new(-9.0f, 3.9f), new(-1.7f, -3.85f), new(4.8f, 3.8f), new(8.3f, -3.7f) };
                foreach (Vector3 block in blocks)
                {
                    ArtFactory.SpriteObject("FortressStone", worldRoot.transform, ArtFactory.Square, new Color32(102, 106, 99, 255), new Vector2(1.5f, 0.65f), block, -4);
                    ArtFactory.SpriteObject("StoneHighlight", worldRoot.transform, ArtFactory.Square, new Color(0.7f, 0.72f, 0.66f, 0.5f), new Vector2(1.28f, 0.12f), block + new Vector3(0, 0.18f), -3);
                }
            }
        }

        private void CreateBase()
        {
            var baseObject = new GameObject("Despensa");
            baseObject.transform.SetParent(worldRoot.transform, false);
            baseObject.transform.position = path[path.Count - 1] + new Vector3(0.16f, 0.08f, 0);
            var collider = baseObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.2f, 1.5f);
            baseObject.AddComponent<BaseGoal>();

            ArtFactory.SpriteObject("Shadow", baseObject.transform, ArtFactory.Circle, new Color(0.1f, 0.1f, 0.1f, 0.25f), new Vector2(1.8f, 0.55f), new Vector3(0, -0.66f), 2);
            ArtFactory.SpriteObject("House", baseObject.transform, ArtFactory.Square, new Color32(238, 209, 145, 255), new Vector2(1.35f, 1.25f), new Vector3(0, -0.05f), 5);
            var roof = ArtFactory.SpriteObject("Roof", baseObject.transform, ArtFactory.Triangle, activeStage.AccentColor, new Vector2(1.75f, 1.05f), new Vector3(0, 0.88f), 6);
            roof.transform.localRotation = Quaternion.identity;
            ArtFactory.SpriteObject("Door", baseObject.transform, ArtFactory.Square, new Color32(108, 71, 49, 255), new Vector2(0.46f, 0.76f), new Vector3(0, -0.28f), 7);
            ArtFactory.SpriteObject("Food", baseObject.transform, ArtFactory.Circle, new Color32(241, 175, 65, 255), new Vector2(0.23f, 0.18f), new Vector3(0, -0.22f), 8);
            ArtFactory.WorldText("BaseLabel", baseObject.transform, "DESPENSA", new Vector3(0, -1.03f), 34, 0.048f, 20).color = new Color32(55, 70, 53, 255);
        }

        private void CreateGameUi()
        {
            var top = Panel("TopBar", uiRoot, new Color(0.035f, 0.07f, 0.075f, 0.94f));
            SetAnchors(top.rectTransform, new Vector2(0, 0.925f), Vector2.one);
            resourceText = TopStat(top.transform, "PEIXES 300", 0.08f);
            baseText = TopStat(top.transform, "BASE 25/25", 0.27f);
            waveText = TopStat(top.transform, $"ONDA 0/{totalWaves}", 0.47f);
            defeatedText = TopStat(top.transform, "RATOS 0", 0.66f);
            timeText = TopStat(top.transform, "TEMPO 07:00", 0.82f);
            Button menu = UiButton("Menu", top.transform, "MAPA", new Color32(80, 94, 98, 255), ShowWorldMap);
            Place(menu.GetComponent<RectTransform>(), new Vector2(0.955f, 0.5f), new Vector2(125, 48), Vector2.zero);

            var bottom = Panel("BottomBar", uiRoot, new Color(0.035f, 0.07f, 0.075f, 0.95f));
            SetAnchors(bottom.rectTransform, Vector2.zero, new Vector2(1, 0.13f));
            Text buildTitle = Label("BuildTitle", bottom.transform, "CONSTRUIR", 20, FontStyle.Bold, new Color32(204, 221, 213, 255));
            Place(buildTitle.rectTransform, new Vector2(0.055f, 0.78f), new Vector2(180, 32), Vector2.zero);

            CreateBuildButton(bottom.transform, TowerKind.Metralhadora, 0.145f, "1");
            CreateBuildButton(bottom.transform, TowerKind.Bazuca, 0.315f, "2");
            CreateBuildButton(bottom.transform, TowerKind.Catapulta, 0.485f, "3");

            selectionPanel = Panel("Selection", bottom.transform, new Color(0.09f, 0.14f, 0.14f, 1f)).gameObject;
            Place(selectionPanel.GetComponent<RectTransform>(), new Vector2(0.805f, 0.5f), new Vector2(700, 112), Vector2.zero);
            selectionTitle = Label("SelectionTitle", selectionPanel.transform, "Selecione uma torre", 25, FontStyle.Bold, Color.white);
            Place(selectionTitle.rectTransform, new Vector2(0.27f, 0.66f), new Vector2(330, 36), Vector2.zero);
            selectionStats = Label("SelectionStats", selectionPanel.transform, "", 18, FontStyle.Normal, new Color32(191, 212, 202, 255));
            Place(selectionStats.rectTransform, new Vector2(0.27f, 0.3f), new Vector2(330, 34), Vector2.zero);
            upgradeButton = UiButton("Upgrade", selectionPanel.transform, "EVOLUIR", new Color32(203, 144, 53, 255), UpgradeSelected);
            Place(upgradeButton.GetComponent<RectTransform>(), new Vector2(0.65f, 0.5f), new Vector2(175, 72), Vector2.zero);
            upgradeButtonText = upgradeButton.GetComponentInChildren<Text>();
            upgradeButtonText.fontSize = 19;
            Button sellButton = UiButton("Sell", selectionPanel.transform, "VENDER", new Color32(176, 83, 62, 255), SellSelected);
            Place(sellButton.GetComponent<RectTransform>(), new Vector2(0.88f, 0.5f), new Vector2(145, 72), Vector2.zero);
            sellButtonText = sellButton.GetComponentInChildren<Text>();
            sellButtonText.fontSize = 18;
            selectionPanel.SetActive(false);

            toastText = Label("Toast", uiRoot, "", 25, FontStyle.Bold, Color.white);
            Place(toastText.rectTransform, new Vector2(0.5f, 0.865f), new Vector2(980, 56), Vector2.zero);
            toastText.alignment = TextAnchor.MiddleCenter;
        }

        private Text TopStat(Transform parent, string value, float anchorX)
        {
            Text text = Label("Stat", parent, value, 24, FontStyle.Bold, new Color32(238, 244, 239, 255));
            Place(text.rectTransform, new Vector2(anchorX, 0.5f), new Vector2(270, 55), Vector2.zero);
            return text;
        }

        private void CreateBuildButton(Transform parent, TowerKind kind, float anchorX, string key)
        {
            string shortName = kind switch
            {
                TowerKind.Metralhadora => "METRALHA",
                TowerKind.Bazuca => "BAZUCA",
                _ => "CATAPULTA"
            };
            Button button = UiButton(kind.ToString(), parent, $"[{key}] {shortName}\nPEIXES {Balance.BuildCost(kind, difficulty)}", new Color32(72, 104, 101, 255), () => SelectBuildKind(kind));
            Place(button.GetComponent<RectTransform>(), new Vector2(anchorX, 0.43f), new Vector2(285, 78), Vector2.zero);
            button.GetComponentInChildren<Text>().fontSize = 20;
            buildButtonImages[kind] = button.GetComponent<Image>();
        }

        private IEnumerator RunWaves()
        {
            yield return new WaitForSeconds(2.2f);
            for (int wave = 0; wave < totalWaves && Phase == GamePhase.Playing; wave++)
            {
                currentWave = wave + 1;
                int count = EnemiesInWave(wave);
                string warning = wave == totalWaves - 1 ? "ONDA FINAL" : $"ONDA {currentWave}";
                ShowToast($"{warning} • {count} ratos chegando!", 1.8f);
                yield return new WaitForSeconds(1f);
                waveInProgress = true;
                for (int i = 0; i < count && Phase == GamePhase.Playing; i++)
                {
                    EnemyKind kind = ChooseEnemy(wave, i);
                    SpawnEnemy(kind);
                    float interval = difficulty switch
                    {
                        Difficulty.Dificil => Mathf.Max(0.36f, 0.55f - selectedStage * 0.016f - wave * 0.009f),
                        Difficulty.Insano => Mathf.Max(0.24f, 0.44f - selectedStage * 0.012f - wave * 0.008f),
                        _ => Mathf.Max(0.50f, 0.70f - selectedStage * 0.02f - wave * 0.012f)
                    };
                    yield return new WaitForSeconds(interval);
                }

                while (enemies.Count > 0 && Phase == GamePhase.Playing) yield return null;
                waveInProgress = false;
                if (wave < totalWaves - 1 && Phase == GamePhase.Playing)
                {
                    int timeBonus = difficulty switch
                    {
                        Difficulty.Dificil => 10,
                        Difficulty.Insano => 12,
                        _ => 8
                    };
                    timeRemaining += timeBonus;
                    ShowToast($"Onda vencida! +{timeBonus}s para a próxima.", 2.1f);
                    yield return new WaitForSeconds(2.3f);
                }
            }

            waveInProgress = false;
            wavesFinished = true;
            if (Phase == GamePhase.Playing && enemies.Count == 0) Victory();
        }

        private int EnemiesInWave(int zeroBasedWave)
        {
            int stageExtra = selectedStage / 2;
            int baseCount = difficulty switch
            {
                Difficulty.Dificil => 6 + stageExtra + zeroBasedWave * 2,
                Difficulty.Insano => 7 + stageExtra + zeroBasedWave * 2 + zeroBasedWave / 3,
                _ => 5 + stageExtra + zeroBasedWave + zeroBasedWave / 2
            };
            float waveProgress = totalWaves <= 1 ? 1f : zeroBasedWave / (totalWaves - 1f);
            float pressureMultiplier = Mathf.Lerp(1.10f, 1.25f, waveProgress);
            return Mathf.CeilToInt(baseCount * pressureMultiplier);
        }

        private EnemyKind ChooseEnemy(int wave, int index)
        {
            int heavyRate = difficulty == Difficulty.Insano
                ? (selectedStage >= 2 || wave >= 8 ? 3 : 4)
                : (selectedStage >= 3 || wave >= 7 ? 4 : 5);
            if (wave >= 1 && (index + wave) % heavyRate == heavyRate - 1) return EnemyKind.Grandao;
            if ((index + wave * 2) % 3 == 2) return EnemyKind.Corredor;
            return EnemyKind.Comum;
        }

        private void SpawnEnemy(EnemyKind kind)
        {
            var enemyObject = new GameObject("Rato " + kind);
            enemyObject.transform.SetParent(worldRoot.transform, false);
            float difficultyHealth;
            float difficultySpeed;
            float healthPerWave;
            float speedPerWave;
            float maximumSpeed;
            float bountyMultiplier;
            int baseDamageBonus;
            switch (difficulty)
            {
                case Difficulty.Dificil:
                    difficultyHealth = 1.22f;
                    difficultySpeed = 1.07f;
                    healthPerWave = 0.06f;
                    speedPerWave = 0.008f;
                    maximumSpeed = 1.30f;
                    bountyMultiplier = 0.60f;
                    baseDamageBonus = 0;
                    break;
                case Difficulty.Insano:
                    difficultyHealth = 1.32f;
                    difficultySpeed = 1.10f;
                    healthPerWave = 0.07f;
                    speedPerWave = 0.009f;
                    maximumSpeed = 1.36f;
                    bountyMultiplier = 0.35f;
                    baseDamageBonus = 1;
                    break;
                default:
                    difficultyHealth = 1f;
                    difficultySpeed = 1f;
                    healthPerWave = 0.055f;
                    speedPerWave = 0.008f;
                    maximumSpeed = 1.28f;
                    bountyMultiplier = 1f;
                    baseDamageBonus = 0;
                    break;
            }

            float healthMultiplier = difficultyHealth * (1f + selectedStage * 0.07f + (currentWave - 1) * healthPerWave);
            float speedMultiplier = Mathf.Min(maximumSpeed,
                difficultySpeed * (1f + selectedStage * 0.018f + (currentWave - 1) * speedPerWave));
            enemyObject.AddComponent<Enemy>().Initialize(kind, path, healthMultiplier, speedMultiplier,
                bountyMultiplier, baseDamageBonus);
        }

        public void RegisterEnemy(Enemy enemy)
        {
            if (!enemies.Contains(enemy)) enemies.Add(enemy);
        }

        public void UnregisterEnemy(Enemy enemy, bool wasDefeated, int bounty)
        {
            enemies.Remove(enemy);
            if (wasDefeated)
            {
                defeated++;
                currency += bounty;
            }
            if (wavesFinished && enemies.Count == 0 && Phase == GamePhase.Playing) Victory();
        }

        public void EnemyReachedBase(int damage)
        {
            if (Phase != GamePhase.Playing) return;
            baseHealth = Mathf.Max(0, baseHealth - damage);
            ShowToast($"Um rato entrou! A base perdeu {damage} de vida.", 1.4f);
            if (baseHealth <= 0) Defeat("Os ratos invadiram a despensa!");
        }

        public bool TrySpend(int amount)
        {
            if (currency < amount)
            {
                ShowToast("Peixes insuficientes! Elimine mais ratos.", 1.8f);
                return false;
            }
            currency -= amount;
            return true;
        }

        public void TryBuild(TowerSpot spot)
        {
            if (Phase != GamePhase.Playing || spot.IsOccupied) return;
            int cost = Balance.BuildCost(selectedKind, difficulty);
            if (!TrySpend(cost)) return;
            Tower tower = spot.Occupy(selectedKind);
            Audio.Upgrade();
            SelectTower(tower);
            ShowToast($"{Balance.TowerName(selectedKind)} entrou em ação!", 1.6f);
        }

        private void SelectBuildKind(TowerKind kind)
        {
            selectedKind = kind;
            Audio.Click();
            if (selectedTower != null) selectedTower.SetSelected(false);
            selectedTower = null;
            if (selectionPanel != null) selectionPanel.SetActive(false);
            UpdateBuildButtons();
            ShowToast($"{Balance.TowerName(kind)} selecionado • toque em um ponto +", 1.6f);
        }

        private void UpdateBuildButtons()
        {
            foreach (var pair in buildButtonImages)
                pair.Value.color = pair.Key == selectedKind ? new Color32(222, 157, 61, 255) : new Color32(72, 104, 101, 255);
        }

        public void SelectTower(Tower tower)
        {
            if (selectedTower != null && selectedTower != tower) selectedTower.SetSelected(false);
            selectedTower = tower;
            selectedTower.SetSelected(true);
            selectionPanel.SetActive(true);
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            if (selectedTower == null) return;
            selectionTitle.text = $"{Balance.TowerName(selectedTower.Kind)} • Nível {selectedTower.Level}";
            selectionStats.text = Balance.TowerRole(selectedTower.Kind);
            sellButtonText.text = $"VENDER\n+{selectedTower.SellPrice} PEIXES";
            if (selectedTower.Level >= 3)
            {
                upgradeButtonText.text = "NÍVEL MÁXIMO";
                upgradeButton.interactable = false;
            }
            else
            {
                upgradeButtonText.text = $"EVOLUIR [E]\nPEIXES {selectedTower.UpgradePrice}";
                upgradeButton.interactable = true;
            }
        }

        private void UpgradeSelected()
        {
            if (selectedTower == null) return;
            if (selectedTower.TryUpgrade())
            {
                RefreshSelection();
                ShowToast("Torre evoluída! Alcance e poder aumentaram.", 1.8f);
            }
        }

        private void SellSelected()
        {
            if (Phase != GamePhase.Playing || selectedTower == null) return;
            Tower tower = selectedTower;
            int refund = tower.SellPrice;
            selectedTower.SetSelected(false);
            selectedTower = null;
            selectionPanel.SetActive(false);
            currency += refund;
            tower.Sell();
            ShowToast($"Torre vendida por {refund} peixes.", 1.8f);
        }

        private void UpdateHud()
        {
            if (resourceText == null) return;
            resourceText.text = $"PEIXES  {currency}";
            baseText.text = $"BASE  {baseHealth}/{maxBaseHealth}";
            baseText.color = baseHealth <= maxBaseHealth * 0.3f ? new Color32(255, 111, 94, 255) : new Color32(238, 244, 239, 255);
            waveText.text = $"ONDA  {currentWave}/{totalWaves}";
            defeatedText.text = $"RATOS  {defeated}";
            int seconds = Mathf.CeilToInt(timeRemaining);
            timeText.text = $"TEMPO  {seconds / 60:00}:{seconds % 60:00}";
        }

        private void Victory()
        {
            if (Phase != GamePhase.Playing) return;
            Phase = GamePhase.Victory;
            Audio.Victory();
            bool firstClear = Campaign.MarkCleared(selectedStage, difficulty);
            PlayerPrefs.SetInt("GVR_BestKills", Mathf.Max(defeated, PlayerPrefs.GetInt("GVR_BestKills", 0)));
            PlayerPrefs.SetInt("GVR_Wins", PlayerPrefs.GetInt("GVR_Wins", 0) + 1);
            PlayerPrefs.Save();
            string mode = difficulty switch
            {
                Difficulty.Dificil => "Difícil",
                Difficulty.Insano => "Insano",
                _ => "Normal"
            };
            string unlock = firstClear && selectedStage < Campaign.Stages.Length - 1 ? $"\nFase {selectedStage + 2} desbloqueada!" : "";
            ShowResult(true, "A DESPENSA ESTÁ SALVA!",
                $"{activeStage.Name} • {mode} concluído!\nVocê venceu {totalWaves} ondas, derrotou {defeated} ratos e preservou {baseHealth} de vida.{unlock}");
        }

        private void Defeat(string reason)
        {
            if (Phase != GamePhase.Playing) return;
            Phase = GamePhase.Defeat;
            Audio.Defeat();
            PlayerPrefs.SetInt("GVR_BestKills", Mathf.Max(defeated, PlayerPrefs.GetInt("GVR_BestKills", 0)));
            PlayerPrefs.Save();
            ShowResult(false, "OS RATOS VENCERAM!", reason + $"\nFase {selectedStage + 1} • Onda {currentWave}/{totalWaves} • Ratos derrotados: {defeated}");
        }

        private void ShowResult(bool won, string titleValue, string bodyValue)
        {
            var shade = Panel("ResultShade", uiRoot, new Color(0.015f, 0.03f, 0.03f, 0.78f));
            Stretch(shade.rectTransform);
            var modal = Panel("ResultModal", shade.transform, new Color32(32, 57, 58, 255));
            Place(modal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(820, 590), Vector2.zero);
            Text title = Label("ResultTitle", modal.transform, titleValue, 48, FontStyle.Bold, won ? new Color32(255, 218, 78, 255) : new Color32(255, 116, 98, 255));
            Place(title.rectTransform, new Vector2(0.5f, 0.82f), new Vector2(740, 90), Vector2.zero);
            Text body = Label("ResultBody", modal.transform, bodyValue, 26, FontStyle.Normal, new Color32(224, 236, 230, 255));
            Place(body.rectTransform, new Vector2(0.5f, 0.61f), new Vector2(710, 150), Vector2.zero);
            body.alignment = TextAnchor.MiddleCenter;
            if (won && selectedStage < Campaign.Stages.Length - 1)
            {
                Button next = UiButton("Next", modal.transform, "PRÓXIMA FASE", Campaign.Stages[selectedStage + 1].AccentColor,
                    () => StartGame(selectedStage + 1, difficulty));
                Place(next.GetComponent<RectTransform>(), new Vector2(0.5f, 0.37f), new Vector2(500, 70), Vector2.zero);
            }
            Button again = UiButton("Again", modal.transform, "JOGAR NOVAMENTE", new Color32(68, 171, 112, 255), () => StartGame(selectedStage, difficulty));
            Place(again.GetComponent<RectTransform>(), new Vector2(0.5f, won && selectedStage < Campaign.Stages.Length - 1 ? 0.22f : 0.31f), new Vector2(500, 68), Vector2.zero);
            Button menu = UiButton("Map", modal.transform, "VOLTAR AO MAPA", new Color32(73, 102, 106, 255), ShowWorldMap);
            Place(menu.GetComponent<RectTransform>(), new Vector2(0.5f, won && selectedStage < Campaign.Stages.Length - 1 ? 0.085f : 0.14f), new Vector2(500, 60), Vector2.zero);
        }

        private void ShowInstructions()
        {
            Audio.Click();
            var shade = Panel("InstructionsShade", uiRoot, new Color(0.015f, 0.03f, 0.03f, 0.83f));
            Stretch(shade.rectTransform);
            var modal = Panel("Instructions", shade.transform, new Color32(34, 62, 62, 255));
            Place(modal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(980, 820), Vector2.zero);
            Text title = Label("Title", modal.transform, "COMO JOGAR", 48, FontStyle.Bold, new Color32(255, 220, 78, 255));
            Place(title.rectTransform, new Vector2(0.5f, 0.87f), new Vector2(800, 70), Vector2.zero);
            string instructions =
                "1. Escolha um gato na barra inferior.\n" +
                "2. Toque em um ponto + do jardim para construir.\n" +
                "3. Toque em uma torre pronta para EVOLUIR ou VENDER (reembolso de 30%).\n\n" +
                "GATO METRALHA — disparo mais rápido, dano por alvo.\n" +
                "GATO BAZUCA — disparo lento, maior dano e alcance.\n" +
                "GATO CATAPULTA — pedras que atingem vários ratos.\n\n" +
                "O mapa possui 5 fases desbloqueáveis.\n" +
                "Normal: 10 ondas • Difícil: 15 ondas • Insano: 20 ondas.\n" +
                "A resistência, velocidade e tamanho das hordas crescem a cada onda.\n\n" +
                "Impeça os ratos de chegar à despensa antes do tempo acabar.\n" +
                "Atalhos no PC: 1, 2, 3 para escolher torres • E para evoluir.";
            Text body = Label("Body", modal.transform, instructions, 24, FontStyle.Normal, new Color32(224, 237, 231, 255));
            Place(body.rectTransform, new Vector2(0.5f, 0.52f), new Vector2(850, 590), Vector2.zero);
            body.alignment = TextAnchor.MiddleLeft;
            Button close = UiButton("Close", modal.transform, "ENTENDI!", new Color32(68, 171, 112, 255), () => Destroy(shade.gameObject));
            Place(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0.1f), new Vector2(400, 68), Vector2.zero);
        }

        private void ShowToast(string message, float duration)
        {
            if (toastText == null) return;
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ToastRoutine(message, duration));
        }

        private IEnumerator ToastRoutine(string message, float duration)
        {
            toastText.text = message;
            toastText.color = Color.white;
            yield return new WaitForSeconds(duration);
            float fade = 0.35f;
            while (fade > 0f)
            {
                fade -= Time.deltaTime;
                toastText.color = new Color(1, 1, 1, Mathf.Clamp01(fade / 0.35f));
                yield return null;
            }
            toastText.text = "";
            toastRoutine = null;
        }

        private static Image Panel(string name, Transform parent, Color color)
        {
            Image image = NewUI<Image>(name, parent);
            image.color = color;
            return image;
        }

        private static Text Label(string name, Transform parent, string value, int fontSize, FontStyle style, Color color)
        {
            Text text = NewUI<Text>(name, parent);
            text.font = ArtFactory.RuntimeFont;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private Button UiButton(string name, Transform parent, string label, Color color, UnityEngine.Events.UnityAction action)
        {
            Image image = Panel(name, parent, color);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.disabledColor = new Color(0.48f, 0.48f, 0.48f, 0.75f);
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => Audio?.Click());
            button.onClick.AddListener(action);
            Text text = Label("Label", button.transform, label, 23, FontStyle.Bold, Color.white);
            Stretch(text.rectTransform, new Vector2(12, 7), new Vector2(-12, -7));
            return button;
        }

        private static T NewUI<T>(string name, Transform parent) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        private static void UiLinePixels(Transform parent, Vector2 from, Vector2 to, float width, Color color)
        {
            Image image = Panel("MapPath", parent, color);
            Vector2 direction = to - from;
            Place(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(direction.magnitude, width), (from + to) * 0.5f);
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect, Vector2 minOffset, Vector2 maxOffset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = minOffset;
            rect.offsetMax = maxOffset;
        }

        private static void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
        }
    }
}
