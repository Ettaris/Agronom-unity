using System.Collections.Generic;
using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;

namespace Systems
{
    public class CardDrawSystem : IGameSystem
    {
        private RunData _runData;
        private GameConfig _config;
        private List<ItemInstance> _currentOffer;
        private int _cardsToSelect;

        public void Initialize()
        {
            _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in CardDrawSystem!");
                return;
            }

            _config = ServiceLocator.Get<GameConfig>();
            _currentOffer = new List<ItemInstance>();
            _cardsToSelect = _config.cardsToSelect;

            // Подписка на события
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            _currentOffer.Clear();
        }

        public IReadOnlyList<ItemInstance> GetCurrentOffer()
        {
            return _currentOffer.AsReadOnly();
        }

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

            var random = _runData.Random;
            int totalCards = _config.cardsPerDay;
            int plantCount = totalCards / 2;
            int fermentCount = totalCards / 3;
            int batteryCount = totalCards - plantCount - fermentCount;

            var plantPool = _config.plantPool;
            var fermentPool = _config.fermentPool;
            var batteryPool = _config.batteryPool;

            // Растения
            if (plantPool != null && plantPool.plants.Count > 0)
            {
                for (int i = 0; i < plantCount; i++)
                {
                    int index = random.NextInt(0, plantPool.plants.Count);
                    var plantData = plantPool.plants[index];
                    int maxCap = plantData.maxGenomeCapacity > 0 ? plantData.maxGenomeCapacity : _config.defaultMaxGenomeCapacity;
                    var plant = new PlantInstance(plantData, maxCap);
                    _currentOffer.Add(plant);
                }
            }

            // Ферменты
            if (fermentPool != null && fermentPool.ferments.Count > 0)
            {
                for (int i = 0; i < fermentCount; i++)
                {
                    int index = random.NextInt(0, fermentPool.ferments.Count);
                    var ferment = new ItemInstance(fermentPool.ferments[index]);
                    _currentOffer.Add(ferment);
                }
            }

            // Батарейки
            if (batteryPool != null && batteryPool.batteries.Count > 0)
            {
                for (int i = 0; i < batteryCount; i++)
                {
                    int index = random.NextInt(0, batteryPool.batteries.Count);
                    var battery = new ItemInstance(batteryPool.batteries[index]);
                    _currentOffer.Add(battery);
                }
            }

            // Перемешиваем
            for (int i = _currentOffer.Count - 1; i > 0; i--)
            {
                int j = random.NextInt(0, i + 1);
                var temp = _currentOffer[i];
                _currentOffer[i] = _currentOffer[j];
                _currentOffer[j] = temp;
            }

            // Публикуем событие для UI
            EventBus.Publish(new OfferGeneratedEvent
            {
                Offer = _currentOffer,
                MaxSelectable = _cardsToSelect
            });
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _runData = evt.RunData;
            // Генерируем предложение только для нового забега (не для загрузки)
            if (!evt.IsLoaded)
            {
                GenerateOffer();
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            // Генерируем для дней после первого (чтобы не дублировать)
            if (evt.DayNumber > 1)
            {
                GenerateOffer();
            }
        }
    }
}