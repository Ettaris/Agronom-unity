using UnityEngine;
using Infrastructure;
using Managers;
using Systems;
using Data;
using Infrastructure.Events;
using System.Collections.Generic;
using Gameplay;

/// <summary>
/// Главный менеджер игры. Отвечает за инициализацию всех систем и управление состоянием игры.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
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

    private bool _isInitialized = false;
    private readonly List<IRunAware> _runAwares = new();

    public static bool IsNewGame = false;
    public static int NewGameSeed = -1;

    void Awake()
    {
        if (_isInitialized) return;

        _isInitialized = true;
        ServiceLocator.Register(_gameConfig);
        ServiceLocator.Register(this);
        RegisterServices();

    }


    /// <summary>
    /// Регистрирует все сервисы в ServiceLocator.
    /// </summary>
    private void RegisterServices()
    {
        Debug.Log("RegisterServices START");

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

        RegisterService(propertyResolver);
        RegisterService(runGeneration);
        RegisterService(growthSystem);
        RegisterService(harvestSystem);
        RegisterService(scoreSystem);
        RegisterService(analyzerSystem);
        RegisterService(centrifugeSystem);
        RegisterService(cardDrawSystem);
        RegisterService(journalSystem);

        RegisterService(_notificationSystem);
        RegisterService(_handView);
        RegisterService(_boardRoot);
        RegisterService(_boardView);
        RegisterService(_gameOverView);
        RegisterService(_HUDView);
        RegisterService(_journalView);
        RegisterService(_laboratoryView);
        RegisterService(_cardDrawView);

        InitializeServices();
    }

    /// <summary>
    /// Инициализирует все зарегистрированные сервисы (вызывает Initialize()).
    /// </summary>
    private void InitializeServices()
    {
        Debug.Log("InitializeServices START");
        // Менеджеры
        ServiceLocator.Get<SaveManager>().Initialize();
        ServiceLocator.Get<RunManager>().Initialize();
        ServiceLocator.Get<DayManager>().Initialize();

        // Системы (порядок не важен, но PropertyResolver должен быть раньше других, если они используют его в Initialize)
        // Но так как все зависимости получаются через ServiceLocator в момент вызова, порядок не критичен.
        ServiceLocator.Get<PropertyResolverSystem>().Initialize();
        ServiceLocator.Get<RunGenerationSystem>().Initialize();
        ServiceLocator.Get<GrowthSystem>().Initialize();
        ServiceLocator.Get<HarvestSystem>().Initialize();
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

        EventBus.Publish(new ServicesInitializedEvent());

    }

    /// <summary>
    /// Запускает игровой процесс.
    /// </summary>
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

    /// <summary>
    /// Вызывается при уничтожении объекта. Освобождает ресурсы всех систем.
    /// </summary>
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
        // Системы
        ServiceLocator.Get<JournalSystem>().Dispose();
        ServiceLocator.Get<CardDrawSystem>().Dispose();
        ServiceLocator.Get<CentrifugeSystem>().Dispose();
        ServiceLocator.Get<ScoreSystem>().Dispose();
        ServiceLocator.Get<HarvestSystem>().Dispose();
        ServiceLocator.Get<GrowthSystem>().Dispose();
        ServiceLocator.Get<RunGenerationSystem>().Dispose();
        ServiceLocator.Get<PropertyResolverSystem>().Dispose();

        // Менеджеры
        ServiceLocator.Get<DayManager>().Dispose();
        ServiceLocator.Get<RunManager>().Dispose();
        ServiceLocator.Get<SaveManager>().Dispose();
    }

    /// <summary>
    /// Публичный метод для перезапуска игры (например, по кнопке "Новая игра").
    /// </summary>
    public void RestartGame()
    {
        // Завершаем текущий забег, если он активен
        var runManager = ServiceLocator.Get<RunManager>();
        if (runManager.CurrentRunData != null)
        {
            runManager.EndRun();
        }

        // Запускаем новый забег
        int seed = Random.Range(0, int.MaxValue);
        runManager.StartNewRun(seed);
    }

    /// <summary>
    /// Публичный метод для выхода из игры.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}