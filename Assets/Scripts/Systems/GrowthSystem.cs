using System.Collections.Generic;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using Properties.Interfaces;

namespace Systems
{
    /// <summary>
    /// Система роста растений. Обновляет прогресс роста всех растений на поле в конце каждого дня.
    /// </summary>
    public class GrowthSystem : IGameSystem, IRunAware
    {
        private RunData _runData;
        private PropertyResolverSystem _propertyResolver;

        public void Initialize()
        {
            EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
            _propertyResolver = ServiceLocator.Get<PropertyResolverSystem>();
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
        }

        public void OnRunDataSetup(RunData runData)
        {
            _runData = runData;
        }

        /// <summary>
        /// Обработчик события окончания дня.
        /// Обновляет рост всех растений на поле.
        /// </summary>
        private void OnDayEnded(DayEndedEvent evt)
        {
            // Получаем все растения с поля
            var allPlants = _runData.Board.GetAllPlants();
            if (allPlants.Count == 0) return;

            // Список растений, которые достигли зрелости (для публикации событий)
            var grownPlants = new List<PlantInstance>();

            foreach (var plant in allPlants)
            {
                // Если растение уже зрелое, пропускаем (оно не должно расти дальше)
                if (plant.IsGrown) continue;

                // Базовая скорость роста: сколько процентов прибавляется за день
                // Предполагаем, что growthTime измеряется в днях.
                float baseGrowthPerDay = 1f / plant.PlantData.growthTime;

                // Применяем модификации от свойств через PropertyResolverSystem
                float modifiedGrowth = _propertyResolver.ModifyGrowth(plant, baseGrowthPerDay);

                // Добавляем к прогрессу
                plant.GrowthProgress += modifiedGrowth;

                // Ограничиваем максимум 1f
                if (plant.GrowthProgress > 1f)
                    plant.GrowthProgress = 1f;

                // Если растение стало зрелым, добавляем в список для событий
                if (plant.IsGrown)
                {
                    grownPlants.Add(plant);
                }
            }

            // Публикуем события для всех созревших растений
            foreach (var plant in grownPlants)
            {
                EventBus.Publish(new PlantGrownEvent { Plant = plant });
            }

            // Дополнительно можно опубликовать событие об обновлении роста (для UI)
            // но UI может подписаться на PlantGrownEvent и обновить отображение.
        }
    }
}