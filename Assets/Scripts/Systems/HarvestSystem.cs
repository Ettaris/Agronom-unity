using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;

namespace Systems
{
    /// <summary>
    /// Система сбора урожая. Позволяет собрать одно растение по координатам.
    /// </summary>
    public class HarvestSystem : IGameSystem
    {
        private RunData _runData;
        private PropertyResolverSystem _propertyResolver;
        private ScoreSystem _scoreSystem;

        public void Initialize()
        {
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _runData = evt.RunData;
            _propertyResolver = ServiceLocator.Get<PropertyResolverSystem>();
            _scoreSystem = ServiceLocator.Get<ScoreSystem>();
        }



        /// <summary>
        /// Собирает растение в указанной клетке, если оно зрелое.
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <returns>Количество собранных калорий, или -1 если растение не найдено или незрелое.</returns>
        public int HarvestPlantAt(int x, int y)
        {
            if (_runData == null || _propertyResolver == null || _scoreSystem == null) return -1;

            var cell = _runData.Board.GetCell(x, y);
            if (cell == null || cell.Plant == null || !cell.Plant.IsGrown) return -1;

            var plant = cell.Plant;
            int baseCalories = plant.PlantData.baseCalories;

            // 1. Модификация от соседей (Generosity и т.п.)
            int modifiedCalories = _propertyResolver.ModifyHarvestByNeighbors(plant, baseCalories);

            // 2. Модификация от собственных свойств
            modifiedCalories = _propertyResolver.ModifyHarvest(plant, modifiedCalories);

            // 3. Удаляем растение с поля
            _runData.Board.RemovePlant(x, y);
            plant.CurrentCell = null;

            // 4. Вызываем эффекты уничтожения (Fruiting, RandomFruiting и т.д.)
            //    Это должно происходить до UnregisterPlant, чтобы свойства были доступны
            _propertyResolver.OnPlantDestroyed(plant, x, y);

            // 5. Публикуем событие уничтожения (для других систем)
            EventBus.Publish(new PlantKilledEvent { Plant = plant, X = x, Y = y, Reason = "Harvested" });

            // 6. Удаляем все свойства растения из кэша PropertyResolverSystem
            _propertyResolver.UnregisterPlant(plant);

            // 7. Обновляем счёт
            _scoreSystem.AddCalories(modifiedCalories);

            // 8. Публикуем событие сбора (для UI)
            EventBus.Publish(new PlantHarvestedEvent { Plant = plant, X = x, Y = y, CaloriesGained = modifiedCalories });
            return modifiedCalories;
        }
    }
}