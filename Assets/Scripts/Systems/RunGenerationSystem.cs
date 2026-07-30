using System.Collections.Generic;
using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;

namespace Systems
{
    public class RunGenerationSystem : IGameSystem
    {
        private GameConfig _config;
        private RunManager _runManager;
        private SaveManager _saveManager;
        private PropertyResolverSystem _propertyResolver;

        public void Initialize()
        {
            _config = ServiceLocator.Get<GameConfig>();
            _runManager = ServiceLocator.Get<RunManager>();
            _saveManager = ServiceLocator.Get<SaveManager>();
            _propertyResolver = ServiceLocator.Get<PropertyResolverSystem>();


            EventBus.Subscribe<RunGenerationRequestedEvent>(OnRunGenerationRequested);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunGenerationRequestedEvent>(OnRunGenerationRequested);
        }

        private void OnRunGenerationRequested(RunGenerationRequestedEvent evt)
        {
            GenerateRun(evt.Seed);
        }

        public void GenerateRun(int seed)
        {
            var journal = _saveManager.LoadJournal() ?? new JournalData();
            var random = new SeedGenerator(seed);

            var plantPool = _config.plantPool;
            var genomePool = _config.genomePool;
            var fermentPool = _config.fermentPool;
            var batteryPool = _config.batteryPool;

            if (plantPool == null || plantPool.plants.Count == 0)
            {
                UnityEngine.Debug.LogError("PlantPool is empty!");
                return;
            }

            int deckSize = _config.startingDeckSize;
            int handSize = _config.initialHandSize;
            int totalPlantsNeeded = deckSize + handSize;

            // 1. Генерация растений
            var availablePlants = new List<PlantData>(plantPool.plants);
            ShuffleList(availablePlants, random);

            while (availablePlants.Count < totalPlantsNeeded)
                availablePlants.Add(availablePlants[random.NextInt(0, availablePlants.Count)]);

            var allPlantInstances = new List<PlantInstance>(totalPlantsNeeded);
            var genomeList = genomePool.genomeProperties;

            for (int i = 0; i < totalPlantsNeeded; i++)
            {
                var plantData = availablePlants[i];
                int maxCap = plantData.maxGenomeCapacity > 0 ? plantData.maxGenomeCapacity : _config.defaultMaxGenomeCapacity;
                var plant = PlantFactory.CreatePlantWithProperties(plantData, random, genomePool, _config.maxPropertiesPerPlant);

                // Назначаем свойства
                PlantGeneratorHelper.AssignRandomProperties(plant, random, genomePool, _config.maxPropertiesPerPlant);

                allPlantInstances.Add(plant);
                // Регистрируем свойства в PropertyResolverSystem
                _propertyResolver.RegisterPlant(plant);
            }

            // 2. Другие предметы (ферменты, батарейки)
            var extraItems = new List<ItemInstance>();
            if (fermentPool != null)
                foreach (var f in fermentPool.ferments)
                    extraItems.Add(new ItemInstance(f));
            if (batteryPool != null)
                foreach (var b in batteryPool.batteries)
                    extraItems.Add(new ItemInstance(b));

            // 3. Разделение на руку и колоду
            var handPlants = new List<PlantInstance>(handSize);
            var deckPlants = new List<PlantInstance>(deckSize);
            for (int i = 0; i < allPlantInstances.Count; i++)
            {
                if (i < handSize)
                    handPlants.Add(allPlantInstances[i]);
                else
                    deckPlants.Add(allPlantInstances[i]);
            }

            // 4. Создание RunData
            var runData = new RunData(seed, _config.boardWidth, _config.boardHeight, _config.maxHandSize, journal);

            runData.TotalCaloriesGoal = _config.totalCaloriesGoal;
            runData.IsTotalGoalReached = false;

            runData.Stages = _config.stages;
            runData.CurrentStageIndex = 0;
            runData.StageStartDay = 1;

            // 5. Заполнение руки
            foreach (var plant in handPlants)
                runData.Hand.Add(plant);

            // 6. Заполнение колоды
            foreach (var plant in deckPlants)
                runData.Deck.Add(plant);
            foreach (var item in extraItems)
                runData.Deck.Add(item);

            runData.Deck.Shuffle(random);
            runData.CurrentDay = 1;
            _runManager.CurrentRunData = runData;

            EventBus.Publish(new RunStartedEvent { Seed = seed, RunData = runData });
            UnityEngine.Debug.Log($"Run generated. Hand: {runData.Hand.Count}, Deck: {runData.Deck.Count}");
        }

        private void ShuffleList<T>(List<T> list, SeedGenerator random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.NextInt(0, i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }

}