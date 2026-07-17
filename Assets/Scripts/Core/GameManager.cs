using UnityEngine;
using Infrastructure;
using Managers;
using Systems;
using Data;

/// <summary>
/// Главный менеджер игры. Отвечает за инициализацию всех систем и управление состоянием игры.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] private GameConfig _gameConfig;

    private bool _isInitialized = false;

    public static bool IsNewGame = false;
    public static int NewGameSeed = -1;

    void Awake()
    {
        if (_isInitialized) return;

        // Регистрируем конфиг в ServiceLocator
        ServiceLocator.Register(_gameConfig);

        // Регистрируем все сервисы (менеджеры и системы)
        RegisterServices();

        // Инициализируем все сервисы
        InitializeServices();

        _isInitialized = true;

        // Запускаем игру (для прототипа сразу стартуем забег)
        StartGame();
    }

    /// <summary>
    /// Регистрирует все сервисы в ServiceLocator.
    /// </summary>
    private void RegisterServices()
    {
        // Менеджеры (сервисы)
        var saveManager = new SaveManager();
        var runManager = new RunManager();
        var dayManager = new DayManager();

        ServiceLocator.Register(saveManager);
        ServiceLocator.Register(runManager);
        ServiceLocator.Register(dayManager);

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
        var notificationSystem = new NotificationSystem();

        ServiceLocator.Register(notificationSystem);
        ServiceLocator.Register(propertyResolver);
        ServiceLocator.Register(runGeneration);
        ServiceLocator.Register(growthSystem);
        ServiceLocator.Register(harvestSystem);
        ServiceLocator.Register(scoreSystem);
        ServiceLocator.Register(analyzerSystem);
        ServiceLocator.Register(centrifugeSystem);
        ServiceLocator.Register(cardDrawSystem);
        ServiceLocator.Register(journalSystem);
    }

    /// <summary>
    /// Инициализирует все зарегистрированные сервисы (вызывает Initialize()).
    /// </summary>
    private void InitializeServices()
    {
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
        ServiceLocator.Get<AnalyzerSystem>().Initialize();
        ServiceLocator.Get<CentrifugeSystem>().Initialize();
        ServiceLocator.Get<CardDrawSystem>().Initialize();
        ServiceLocator.Get<JournalSystem>().Initialize();
    }

    /// <summary>
    /// Запускает игровой процесс. Для прототипа сразу создаёт новый забег.
    /// </summary>
    private async void StartGame()
    {
        if (IsNewGame)
        {
            // Новая игра – запускаем новый забег с переданным seed или случайным
            int seed = NewGameSeed >= 0 ? NewGameSeed : Random.Range(0, int.MaxValue);
            ServiceLocator.Get<RunManager>().StartNewRun(seed);
            IsNewGame = false; // сбрасываем флаг
            NewGameSeed = -1;
        }
        else
        {
            // Проверяем сохранение
            var saveManager = ServiceLocator.Get<SaveManager>();
            if (saveManager.HasSave)
            {
                bool loaded = await saveManager.LoadGameAsync();
                if (loaded)
                {
                    Debug.Log("Game loaded successfully");
                    return;
                }
            }
            // Если сохранения нет или загрузка не удалась – начинаем новый забег
            int seed = Random.Range(0, int.MaxValue);
            ServiceLocator.Get<RunManager>().StartNewRun(seed);
        }
    }

    /// <summary>
    /// Вызывается при уничтожении объекта. Освобождает ресурсы всех систем.
    /// </summary>
    void OnDestroy()
    {
        DisposeServices();
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
        ServiceLocator.Get<AnalyzerSystem>().Dispose();
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