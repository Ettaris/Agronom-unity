using System.Collections.Generic;
using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using UnityEngine;

namespace Systems
{
    public class CardDrawSystem : IGameSystem
    {
        private RunData _runData;
        private GameConfig _config;
        private List<ItemInstance> _currentOffer;
        private int _cardsToSelect;
        private PropertyResolverSystem _propertyResolver;
        private GenomePool _genomePool;
        private int _maxPropertiesPerPlant;

        public void Initialize()
        {

            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            _currentOffer.Clear();
        }

        public IReadOnlyList<ItemInstance> GetCurrentOffer() => _currentOffer.AsReadOnly();

        public int GetMaxSelectable() => _cardsToSelect;

        public bool SelectCards(List<ItemInstance> selected)
        {
            if (selected == null || selected.Count != _cardsToSelect)
            {
                UnityEngine.Debug.LogWarning($"SelectCards: must select exactly {_cardsToSelect} cards.");
                return false;
            }

            foreach (var item in selected)
            {
                if (!_currentOffer.Contains(item))
                {
                    UnityEngine.Debug.LogWarning("SelectCards: selected card not in current offer.");
                    return false;
                }
            }

            foreach (var item in selected)
            {
                if (!_runData.Hand.Add(item))
                {
                    UnityEngine.Debug.LogWarning($"Hand is full, cannot add {item.Data.itemName}.");
                }
                else
                {
                    _currentOffer.Remove(item);
                }
            }

            _currentOffer.Clear();
            EventBus.Publish(new HandUpdatedEvent());
            return true;
        }

        private void GenerateOffer()
        {
            _currentOffer.Clear();

            var offerConfig = _config.offerGenerationConfig;
            var rarityConfig = _config.plantRarityConfig;
            var rarityPool = _config.plantRarityPool;

            if (offerConfig == null || rarityConfig == null || rarityPool == null)
            {
                UnityEngine.Debug.LogError("CardDrawSystem: OfferGenerationConfig, PlantRarityConfig, or PlantRarityPool is null!");
                return;
            }

            int totalSlots = offerConfig.cardsPerDay;
            int guaranteed = offerConfig.guaranteedPlants;
            int remaining = totalSlots - guaranteed;

            var random = _runData.Random;

            for (int i = 0; i < guaranteed; i++)
            {
                var plant = GenerateRandomPlant(random, rarityConfig, rarityPool);
                if (plant != null)
                    _currentOffer.Add(plant);
            }

            int[] typeWeights = new int[] {
                offerConfig.plantWeight,
                offerConfig.fermentWeight,
                offerConfig.batteryWeight
            };

            for (int i = 0; i < remaining; i++)
            {
                int typeIndex = WeightedRandom.ChooseIndex(typeWeights, random);
                ItemInstance item = null;

                switch (typeIndex)
                {
                    case 0: // Plant
                        item = GenerateRandomPlant(random, rarityConfig, rarityPool);
                        break;
                    case 1: // Ferment
                        var fermentPool = _config.fermentPool;
                        if (fermentPool != null && fermentPool.ferments.Count > 0)
                        {
                            int idx = random.NextInt(fermentPool.ferments.Count);
                            item = new ItemInstance(fermentPool.ferments[idx]);
                        }
                        break;
                    case 2: // Battery
                        var batteryPool = _config.batteryPool;
                        if (batteryPool != null && batteryPool.batteries.Count > 0)
                        {
                            int idx = random.NextInt(batteryPool.batteries.Count);
                            item = new ItemInstance(batteryPool.batteries[idx]);
                        }
                        break;
                }

                if (item != null)
                    _currentOffer.Add(item);
            }

            // Перемешивание
            for (int i = _currentOffer.Count - 1; i > 0; i--)
            {
                int j = random.NextInt(i + 1);
                var temp = _currentOffer[i];
                _currentOffer[i] = _currentOffer[j];
                _currentOffer[j] = temp;
            }

            EventBus.Publish(new OfferGeneratedEvent
            {
                Offer = _currentOffer,
                MaxSelectable = offerConfig.selectableCards
            });
        }

        private PlantInstance GenerateRandomPlant(SeedGenerator random, PlantRarityConfig rarityConfig, PlantRarityPool rarityPool)
        {
            int[] rarityWeights = new int[] { rarityConfig.commonWeight, rarityConfig.uncommonWeight, rarityConfig.rareWeight, rarityConfig.epicWeight, rarityConfig.legendaryWeight };
            int rarityIndex = WeightedRandom.ChooseIndex(rarityWeights, random);

            List<PlantData> plantList = null;
            switch (rarityIndex)
            {
                case 0: plantList = rarityPool.commonPlants; break;
                case 1: plantList = rarityPool.uncommonPlants; break;
                case 2: plantList = rarityPool.rarePlants; break;
                case 3: plantList = rarityPool.epicPlants; break;
                case 4: plantList = rarityPool.legendaryPlants; break;
            }

            if (plantList == null || plantList.Count == 0)
            {
                Debug.LogWarning($"No plants of rarity index {rarityIndex} in pool.");
                return null;
            }

            int plantIdx = random.NextInt(plantList.Count);
            var plantData = plantList[plantIdx];

            // Используем фабрику
            return PlantFactory.CreatePlantWithProperties(plantData, random, _genomePool, _maxPropertiesPerPlant);
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _runData = evt.RunData;
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in CardDrawSystem!");
                return;
            }

            _config = ServiceLocator.Get<GameConfig>();
            _propertyResolver = ServiceLocator.Get<PropertyResolverSystem>();
            _genomePool = _config.genomePool;
            _maxPropertiesPerPlant = _config.maxPropertiesPerPlant;

            _currentOffer = new List<ItemInstance>();

            if (_config.offerGenerationConfig != null)
                _cardsToSelect = _config.offerGenerationConfig.selectableCards;
            else
                _cardsToSelect = 2;

            if (!evt.IsLoaded)
                GenerateOffer();
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (evt.DayNumber > 1)
                GenerateOffer();
        }
    }
}