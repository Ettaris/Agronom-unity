using UnityEngine;
using Infrastructure;
using Managers;
using Systems;
using Data;
using Infrastructure.Events;
using System.Collections.Generic;
using Gameplay;
using Gameplay.Calculation;

/// <summary>
/// Главный менеджер игры. Отвечает за инициализацию всех систем и управление состоянием игры.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    [Header("DEBUG")]
    [SerializeField] private bool _showTutorial;

    [Header("Configs")]
    [SerializeField] private GameConfig _gameConfig;

    [Header("SceneContext")]
    [SerializeField] private BoardRoot _boardRoot;
    [SerializeField] private NotificationSystem _notificationSystem;
    [SerializeField] private HandView _handView;
    [SerializeField] private BoardView _boardView;
    [SerializeField] private GameOverView _gameOverView;
    [SerializeField] private JournalView _journalView;
    [SerializeField] private HUDView _HUDView;
    [SerializeField] private LaboratoryView _laboratoryView;
    [SerializeField] private CardDrawView _cardDrawView;
    [SerializeField] private AudioRoot _audioRoot;

    [Header("NarrativeContext")]
    [SerializeField] private GameObject _tutorialBackground;

    private bool _isInitialized = false;
    private readonly List<IRunAware> _runAwares = new();

    public static bool IsNewGame = false;
    public static int NewGameSeed = -1;

    void Awake()
    {
        if (_isInitialized) return;

        _isInitialized = true;

        RegisterServices();
    }


    /// <summary>
    /// Регистрирует все сервисы в ServiceLocator.
    /// </summary>
    private void RegisterServices()
    {
        ServiceLocator.Register(_gameConfig);
        ServiceLocator.Register(this);

        // Менеджеры (сервисы)
        var saveManager = new SaveManager();
        var runManager = new RunManager();
        var dayManager = new DayManager();


        RegisterService(saveManager);
        RegisterService(runManager);
        RegisterService(dayManager);


        // Системы
        var propertyResolver = new PropertyResolverSystem();
        var runGeneration = new RunGenerationSystem();
        var growthSystem = new GrowthSystem();
        var harvestSystem = new HarvestSystem();
        var scoreSystem = new ScoreSystem();
        var analyzerSystem = new AnalyzerSystem();
        var centrifugeSystem = new CentrifugeSystem();
        var cardDrawSystem = new CardDrawSystem();
        var journalSystem = new JournalSystem();
        var dropHandler = new DropHandler();
        var narrativeSystem = new NarrativeSystem();

        var harvestCalculator = new HarvestCalculator();
        var placementPreviewSystem = new PlacementPreviewSystem();

        RegisterService(propertyResolver);
        RegisterService(runGeneration);
        RegisterService(growthSystem);
        RegisterService(harvestSystem);
        RegisterService(scoreSystem);
        RegisterService(analyzerSystem);
        RegisterService(centrifugeSystem);
        RegisterService(cardDrawSystem);
        RegisterService(journalSystem);
        RegisterService(dropHandler);
        RegisterService(narrativeSystem);
        RegisterService(harvestCalculator);
        RegisterService(placementPreviewSystem);

        RegisterService(_notificationSystem);
        RegisterService(_handView);
        RegisterService(_boardRoot);
        RegisterService(_boardView);
        RegisterService(_gameOverView);
        RegisterService(_HUDView);
        RegisterService(_journalView);
        RegisterService(_laboratoryView);
        RegisterService(_cardDrawView);
        RegisterService(_audioRoot);

        InitializeServices();
    }

    private void InitializeServices()
    {
        ServiceLocator.Get<SaveManager>().Initialize();
        ServiceLocator.Get<RunManager>().Initialize();
        ServiceLocator.Get<DayManager>().Initialize();

        ServiceLocator.Get<HarvestSystem>().Initialize();
        ServiceLocator.Get<PropertyResolverSystem>().Initialize();
        ServiceLocator.Get<RunGenerationSystem>().Initialize();
        ServiceLocator.Get<GrowthSystem>().Initialize();
        ServiceLocator.Get<ScoreSystem>().Initialize();
        ServiceLocator.Get<CentrifugeSystem>().Initialize();
        ServiceLocator.Get<CardDrawSystem>().Initialize();
        ServiceLocator.Get<JournalSystem>().Initialize();
        ServiceLocator.Get<BoardRoot>().Initialize();
        ServiceLocator.Get<BoardView>().Initialize();
        ServiceLocator.Get<HandView>().Initialize();
        ServiceLocator.Get<NotificationSystem>().Initialize();
        ServiceLocator.Get<JournalView>().Initialize();
        ServiceLocator.Get<HUDView>().Initialize();
        ServiceLocator.Get<GameOverView>().Initialize();
        ServiceLocator.Get<LaboratoryView>().Initialize();
        ServiceLocator.Get<CardDrawView>().Initialize();
        ServiceLocator.Get<DropHandler>().Initialize();
        ServiceLocator.Get<NarrativeSystem>().Initialize();
        ServiceLocator.Get<AudioRoot>().Initialize();
        ServiceLocator.Get<PlacementPreviewSystem>().Initialize();

        StartGame();

    }

    public void InitializeAndActivateRun(RunData runData)
    {
        foreach (var runInterface in _runAwares)
        {
            runInterface.OnRunDataSetup(runData);
        }
        Debug.Log("RunStarted Event");
        EventBus.Publish(new RunStartedEvent { RunData = runData });
        
        if (_showTutorial)
        {
            var narrativeSystem = ServiceLocator.Get<NarrativeSystem>();
            narrativeSystem.StartSequence("Tutorial_Intro", () =>
            {
                Debug.Log("Tutorial done");
            });
        }
        else { _tutorialBackground.SetActive(false); }
    }

    private async void StartGame()
    {
        Debug.Log("StartGame");
        if (IsNewGame)
        {
            int seed = NewGameSeed >= 0 ? NewGameSeed : Random.Range(0, int.MaxValue);
            ServiceLocator.Get<RunManager>().StartNewRun(seed);
            IsNewGame = false;
            NewGameSeed = -1;
            return;
        }

        var saveManager = ServiceLocator.Get<SaveManager>();
        if (await saveManager.HasSaveAsync())
        {
            bool loaded = await saveManager.LoadGameAsync();
            if (loaded)
            {
                Debug.Log("Game loaded successfully");
                return;
            }
            Debug.LogWarning("Failed to load save, starting new run.");
        }

        int newSeed = Random.Range(0, int.MaxValue);
        ServiceLocator.Get<RunManager>().StartNewRun(newSeed);
    }

    void OnDestroy()
    {
        DisposeServices();
    }

    /// <summary>
    /// Общий метод для регистрации
    /// </summary>
    private void RegisterService<T>(T service) where T : class
    {
        ServiceLocator.Register(service);

        if (service is IRunAware aware)
            _runAwares.Add(aware);
    }

    /// <summary>
    /// Вызывает Dispose для всех систем в обратном порядке инициализации.
    /// </summary>
    private void DisposeServices()
    {
        ServiceLocator.Get<AudioRoot>().Dispose();
        ServiceLocator.Get<NarrativeSystem>().Dispose();
        ServiceLocator.Get<DropHandler>().Dispose();
        ServiceLocator.Get<NotificationSystem>().Dispose();
        ServiceLocator.Get<CardDrawView>().Dispose();
        ServiceLocator.Get<GameOverView>().Dispose();
        ServiceLocator.Get<LaboratoryView>().Dispose();
        ServiceLocator.Get<HUDView>().Dispose();
        ServiceLocator.Get<JournalView>().Dispose();
        ServiceLocator.Get<HandView>().Dispose();
        ServiceLocator.Get<BoardView>().Dispose();
        ServiceLocator.Get<BoardRoot>().Dispose();
        ServiceLocator.Get<JournalSystem>().Dispose();
        ServiceLocator.Get<CardDrawSystem>().Dispose();
        ServiceLocator.Get<CentrifugeSystem>().Dispose();
        ServiceLocator.Get<ScoreSystem>().Dispose();
        ServiceLocator.Get<HarvestSystem>().Dispose();
        ServiceLocator.Get<GrowthSystem>().Dispose();
        ServiceLocator.Get<RunGenerationSystem>().Dispose();
        ServiceLocator.Get<PropertyResolverSystem>().Dispose();

        ServiceLocator.Get<DayManager>().Dispose();
        ServiceLocator.Get<RunManager>().Dispose();
        ServiceLocator.Get<SaveManager>().Dispose();
    }

    public void RestartGame()
    {
        _showTutorial = false;
        var runManager = ServiceLocator.Get<RunManager>();
        runManager.ResetRun();

        _boardView.ClearBoard();
        _boardView.ClearGridBoard();

        _handView.ClearHand();

        _laboratoryView.ClearSlots();

        ServiceLocator.Get<PropertyResolverSystem>().ClearCache();

        int seed = Random.Range(0, int.MaxValue);
        runManager.StartNewRun(seed);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}