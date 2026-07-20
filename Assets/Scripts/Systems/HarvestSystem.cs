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
            _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in HarvestSystem!");
                return;
            }

            _propertyResolver = ServiceLocator.Get<PropertyResolverSystem>();
            _scoreSystem = ServiceLocator.Get<ScoreSystem>();
        }

        public void Dispose()
        {
            // Нет подписок на события, система вызывается напрямую
        }

        /// <summary>
        /// Собирает растение в указанной клетке, если оно зрелое.
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <returns>Количество собранных калорий, или -1 если растение не найдено или незрелое.</returns>
        public int HarvestPlantAt(int x, int y)
        {
            var cell = _runData.Board.GetCell(x, y);
            if (cell == null || cell.Plant == null)
                return -1;

            var plant = cell.Plant;
            if (!plant.IsGrown)
                return -1;

            // Базовые калории
            int baseCalories = plant.PlantData.baseCalories;

            // 1. Модификация от соседей (например, Generosity)
            int modifiedCalories = _propertyResolver.ModifyHarvestByNeighbors(plant, baseCalories);

            // Модифицируем через свойства (без публикации события)
            modifiedCalories = _propertyResolver.ModifyHarvest(plant, modifiedCalories);

            // Удаляем растение с поля
            ServiceLocator.Get<PropertyResolverSystem>().UnregisterPlant(plant);
            _runData.Board.RemovePlant(x, y);
            plant.CurrentCell = null;

            var resolver = ServiceLocator.Get<PropertyResolverSystem>();
            resolver.OnPlantDestroyed(plant, x, y);

            // Обновляем общий счёт
            _scoreSystem.AddCalories(modifiedCalories);

            // Публикуем событие о сборе конкретного растения (для UI и эффектов)
            EventBus.Publish(new PlantHarvestedEvent
            {
                Plant = plant,
                X = x,
                Y = y,
                CaloriesGained = modifiedCalories
            });

            return modifiedCalories;
        }
    }
}