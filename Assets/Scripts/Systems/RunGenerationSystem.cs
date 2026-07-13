using System.Collections.Generic;
using Data;
using Gameplay;
using Infrastructure;
using Managers;

namespace Systems
{
    /// <summary>
    /// Отвечает за генерацию начального состояния забега.
    /// Вызывается из RunManager при старте нового забега.
    /// </summary>
    public class RunGenerationSystem : IGameSystem
    {
        private GameConfig _config;
        private RunManager _runManager;
        private SaveManager _saveManager;

        public void Initialize()
        {
            _config = ServiceLocator.Get<GameConfig>();
            _runManager = ServiceLocator.Get<RunManager>();
            _saveManager = ServiceLocator.Get<SaveManager>();

            // Подписываемся на событие запроса генерации (если нужно)
            // Например, RunManager может публиковать RunGenerationRequestedEvent
            EventBus.Subscribe<RunGenerationRequestedEvent>(OnRunGenerationRequested);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunGenerationRequestedEvent>(OnRunGenerationRequested);
        }

        /// <summary>
        /// Обработчик запроса на генерацию забега.
        /// </summary>
        private void OnRunGenerationRequested(RunGenerationRequestedEvent evt)
        {
            GenerateRun(evt.Seed);
        }

        /// <summary>
        /// Основной метод генерации забега.
        /// </summary>
        public void GenerateRun(int seed)
        {
            // 1. Загружаем мета-прогресс (журнал)
            var journal = _saveManager.LoadJournal() ?? new JournalData();

            // 2. Создаём генератор случайных чисел
            var random = new SeedGenerator(seed);

            // 3. Получаем пулы данных
            var plantPool = _config.plantPool;
            var propertyPool = _config.propertyPool;

            if (plantPool == null || plantPool.plants.Count == 0)
            {
                UnityEngine.Debug.LogError("PlantPool is empty or not set in GameConfig!");
                return;
            }

            // 4. Определяем, сколько растений будет в колоде и руке
            int deckSize = _config.startingDeckSize;
            int handSize = _config.initialHandSize;
            int totalPlantsNeeded = deckSize + handSize;

            // 5. Выбираем растения из пула (с учётом seed)
            //    Для простоты берём все растения, перемешиваем и берём первые totalPlantsNeeded
            //    В будущем можно использовать весовые коэффициенты или редкость.
            var availablePlants = new List<PlantData>(plantPool.plants);
            // Перемешиваем
            for (int i = availablePlants.Count - 1; i > 0; i--)
            {
                int j = random.NextInt(0, i + 1);
                var temp = availablePlants[i];
                availablePlants[i] = availablePlants[j];
                availablePlants[j] = temp;
            }

            // Если растений меньше, чем нужно, повторяем или добавляем дефолтные
            while (availablePlants.Count < totalPlantsNeeded)
            {
                // Дублируем первые элементы, чтобы заполнить
                availablePlants.Add(availablePlants[random.NextInt(0, availablePlants.Count)]);
            }

            // 6. Создаём экземпляры растений с назначенными свойствами
            var allPlantInstances = new List<PlantInstance>(totalPlantsNeeded);
            var propertyPoolList = propertyPool.properties;

            for (int i = 0; i < totalPlantsNeeded; i++)
            {
                var plantData = availablePlants[i];
                var plantInstance = new PlantInstance(plantData, _config.maxPropertiesPerPlant);

                // Назначаем свойства
                // Количество свойств: случайное от 0 до maxPropertiesPerPlant (но хотя бы 1, если есть свойства)
                int propertyCount = propertyPoolList.Count > 0
                    ? random.NextInt(1, _config.maxPropertiesPerPlant + 1)
                    : 0;

                // Перемешиваем пул свойств для детерминированного выбора
                var shuffledProperties = new List<PropertyData>(propertyPoolList);
                for (int j = shuffledProperties.Count - 1; j > 0; j--)
                {
                    int k = random.NextInt(0, j + 1);
                    var temp = shuffledProperties[j];
                    shuffledProperties[j] = shuffledProperties[k];
                    shuffledProperties[k] = temp;
                }

                for (int p = 0; p < propertyCount && p < shuffledProperties.Count; p++)
                {
                    var propData = shuffledProperties[p];
                    var propInstance = new PropertyInstance(propData, 1);
                    plantInstance.AddProperty(propInstance);
                }

                allPlantInstances.Add(plantInstance);
            }

            var resolver = ServiceLocator.Get<PropertyResolverSystem>();
            foreach (var plant in allPlantInstances)
            {
                resolver.RegisterPlant(plant);
            }

            // 7. Разделяем на руку и колоду
            var handPlants = new List<PlantInstance>(handSize);
            // Вместо deckPlants (List<PlantData>) используем список готовых экземпляров
            var deckPlantInstances = new List<PlantInstance>(deckSize);

            for (int i = 0; i < totalPlantsNeeded; i++)
            {
                if (i < handSize)
                    handPlants.Add(allPlantInstances[i]);
                else
                    deckPlantInstances.Add(allPlantInstances[i]);
            }

            // 8. Создаём RunData
            int boardWidth = 5;   // или из конфига
            int boardHeight = 5;
            int dailyQuota = _config.dailyQuota;

            var runData = new RunData(seed, boardWidth, boardHeight, _config.maxHandSize, dailyQuota, journal);

            // 9. Наполняем руку
            foreach (var plant in handPlants)
            {
                runData.Hand.Add(plant);
            }

            // 10. Наполняем колоду
            foreach (var plantData in deckPlantInstances)
            {
                runData.Deck.Add(plantData);
            }
            runData.Deck.Shuffle(random); // Перемешиваем колоду

            // 11. Инициализируем поле пустым (уже создано в конструкторе RunData)

            // 12. Устанавливаем начальный день
            runData.CurrentDay = 1;

            // 13. Сохраняем ссылку на RunData в RunManager
            _runManager.CurrentRunData = runData;

            // 14. Публикуем событие о старте забега
            EventBus.Publish(new RunStartedEvent
            {
                Seed = seed,
                RunData = runData
            });

            UnityEngine.Debug.Log($"Run generation completed. Seed: {seed}, Plants in hand: {runData.Hand.Count}, Deck size: {runData.Deck.Count}");
        }
    }

    /// <summary>
    /// Событие запроса генерации забега (публикуется, например, из UI или RunManager).
    /// </summary>
    public struct RunGenerationRequestedEvent
    {
        public int Seed;
    }

    /// <summary>
    /// Событие, что забег запущен (публикуется после генерации).
    /// </summary>
    public struct RunStartedEvent
    {
        public int Seed;
        public RunData RunData;
    }
}