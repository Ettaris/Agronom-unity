using System.Collections.Generic;
using System.Linq;
using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using UnityEngine;
using static UnityEditor.Tilemaps.RuleTileTemplate;

namespace Systems
{
    public class RunGenerationSystem : IGameSystem
    {
        private GameConfig _config;
        private RunManager _runManager;
        private SaveManager _saveManager;

        public void Initialize()
        {
            EventBus.Subscribe<RunGenerationRequestedEvent>(OnRunGenerationRequested);
            _config = ServiceLocator.Get<GameConfig>();
            _runManager = ServiceLocator.Get<RunManager>();
            _saveManager = ServiceLocator.Get<SaveManager>();
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
            var random = new SeedGenerator(seed);

            int deckSize = _config.startingDeckSize;
            int handSize = _config.initialHandSize;

            var runData = CreateRunData(seed);

            var allPlantTypes = GetAllPlantTypes();

            AssignPermanentModifiersForPlants(allPlantTypes, runData, random);

            var allPlantInstances = CreatePlantInstances(allPlantTypes, runData, random);

            var extraItems = CreateBatteriesAndFerments();

            var (handPlants, deckPlants) = SeparateHandAndDeckPlants(allPlantInstances, handSize, deckSize);

            FillHandAndDeckWithItems(runData, handPlants, deckPlants, extraItems);

            runData.Deck.Shuffle(random);
            runData.CurrentDay = 1;
            _runManager.SetupRunData(runData, seed);
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

        
        
        //Generate Run Methods

        private RunData CreateRunData(int seed)
        {
            var journal = _saveManager.LoadJournal() ?? new JournalData();

            var runData = new RunData(seed, _config.boardWidth, _config.boardHeight, _config.maxHandSize, journal, _config.stages);

            runData.CurrentStageIndex = 0;
            runData.StageStartDay = 1;

            runData.PermanentModifiers = new Dictionary<PlantData, GenomePropertyData>();

            return runData;
        }

        private HashSet<PlantData> GetAllPlantTypes()
        {
            var allPlantTypes = new HashSet<PlantData>();
            var rarityPool = _config.plantRarityPool;

            if (rarityPool != null)
            {
                allPlantTypes.UnionWith(rarityPool.commonPlants);
                allPlantTypes.UnionWith(rarityPool.uncommonPlants);
                allPlantTypes.UnionWith(rarityPool.rarePlants);
                allPlantTypes.UnionWith(rarityPool.epicPlants);
                allPlantTypes.UnionWith(rarityPool.legendaryPlants);
            }
            return allPlantTypes;
        }

        private void AssignPermanentModifiersForPlants(HashSet<PlantData> allPlantTypes, RunData runData, SeedGenerator seedRandom)
        {
            foreach (var plantData in allPlantTypes)
            {
                var perm = ModifierAssigner.SelectPermanentModifier(seedRandom, _config.modifierConfig, _config.genomeRarityPool);
                if (perm != null)
                    runData.PermanentModifiers[plantData] = perm;
                else
                    Debug.LogWarning($"No permanent modifier assigned for {plantData.itemName}");
            }
        }

        private List<PlantInstance> CreatePlantInstances(HashSet<PlantData> allPlantTypes, RunData runData, SeedGenerator random)
        {
            int totalPlantsNeeded = _config.startingDeckSize + _config.initialHandSize;

            var shuffledPlantList = new List<PlantData>(allPlantTypes);
            ShuffleList(shuffledPlantList, random);

            while (shuffledPlantList.Count < totalPlantsNeeded)
            {
                var extra = shuffledPlantList[random.NextInt(0, shuffledPlantList.Count)];
                shuffledPlantList.Add(extra);
            }

            var selectedPlants = shuffledPlantList.Take(totalPlantsNeeded).ToList();

            var allPlantInstances = new List<PlantInstance>(totalPlantsNeeded);
            foreach (var plantData in selectedPlants)
            {
                var plant = PlantFactory.CreatePlantWithProperties(plantData, random, _config, runData);
                allPlantInstances.Add(plant);
            }
            return allPlantInstances;
        }

        private List<ItemInstance> CreateBatteriesAndFerments()
        {
            var fermentPool = _config.fermentPool;
            var batteryPool = _config.batteryPool;

            var extraItems = new List<ItemInstance>();
            if (fermentPool != null)
                foreach (var f in fermentPool.ferments)
                    extraItems.Add(new ItemInstance(f));
            if (batteryPool != null)
                foreach (var b in batteryPool.batteries)
                    extraItems.Add(new ItemInstance(b));
            return extraItems;
        }

        private (List<PlantInstance>, List<PlantInstance>) SeparateHandAndDeckPlants(List<PlantInstance> allPlantInstances, int handSize, int deckSize)
        {
            var handPlants = new List<PlantInstance>(handSize);
            var deckPlants = new List<PlantInstance>(deckSize);
            for (int i = 0; i < allPlantInstances.Count; i++)
            {
                if (i < handSize)
                    handPlants.Add(allPlantInstances[i]);
                else
                    deckPlants.Add(allPlantInstances[i]);
            }
            return (handPlants, deckPlants);
        }

        private void FillHandAndDeckWithItems(RunData runData, List<PlantInstance> handPlants, List<PlantInstance> deckPlants, List<ItemInstance> extraItems)
        {
            foreach (var plant in handPlants)
                runData.Hand.Add(plant);

            foreach (var plant in deckPlants)
                runData.Deck.Add(plant);
            foreach (var item in extraItems)
                runData.Deck.Add(item);
        }
    }

}