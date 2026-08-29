using Gameplay;
using Gameplay.Calculation;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using Properties.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace Systems
{
    /// <summary>
    /// Система сбора урожая. Позволяет собрать одно растение по координатам.
    /// </summary>
    public class HarvestSystem : IGameSystem, IRunAware
    {
        private RunData _runData;
        private PropertyResolverSystem _propertyResolver;
        private ScoreSystem _scoreSystem;
        private BoardRoot _boardRoot;
        private HarvestCalculator _calculator;

        //TODO: harvest system не должен знать про скор систем и ПРС и все остальное. API?
        public void Initialize()
        {
            _propertyResolver = ServiceLocator.Get<PropertyResolverSystem>();
            _scoreSystem = ServiceLocator.Get<ScoreSystem>();
            _boardRoot = ServiceLocator.Get<BoardRoot>();
            _calculator = ServiceLocator.Get<HarvestCalculator>();
        }

        public void Dispose() { }

        public void OnRunDataSetup(RunData runData)
        {
            _runData = runData;
        }

        /// <summary>
        /// Собирает растение в указанной клетке, если оно зрелое.
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <returns>Количество собранных калорий, или -1 если растение не найдено или незрелое.</returns>
        public int HarvestPlantAt(int x, int y, Vector2 screenPos)
        {
            var cell = _runData.Board.GetCell(x, y);
            if (cell == null || cell.Plant == null || !cell.Plant.IsGrown) return -1;

            var plant = cell.Plant;


            var allPlants = _runData.Board.GetAllPlants();
            var propertiesDict = new Dictionary<PlantInstance, List<GenomePropertyInstance>>();
            foreach (var p in allPlants)
            {
                propertiesDict[p] = _propertyResolver.GetPlantProperties(p);
            }

            var context = new HarvestCalculationContext(plant, _runData.Board, false, new ResolverPropertyProvider(_propertyResolver), _runData.DiscoveredGenomes);
            var result = _calculator.Calculate(context);


            var plantProps = _propertyResolver.GetPlantProperties(plant);
            foreach (var prop in plantProps)
            {
                if (prop is IOnHarvestApplication applier)
                    applier.ApplyHarvest(plant, plant.PlantData.baseCalories, _runData.Board);
            }

            var neighbors = _runData.Board.GetNeighbors(x, y, false);
            foreach (var neighborCell in neighbors)
            {
                if (neighborCell.Plant == null) continue;
                var neighborPlant = neighborCell.Plant;
                var neighborProps = _propertyResolver.GetPlantProperties(neighborPlant);
                foreach (var prop in neighborProps)
                {
                    if (prop is IOnNeighborHarvestApplication neighborApplier)
                        neighborApplier.ApplyNeighborHarvest(plant, plant.PlantData.baseCalories, _runData.Board);
                }
            }

            // 3. Применение результата (удаление, добавление калорий, события)
            _runData.Board.RemovePlant(x, y);
            plant.CurrentCell = null;
            _scoreSystem.AddCalories(result.FinalCalories);
            _propertyResolver.UnregisterPlant(plant);

            EventBus.Publish(new PlantHarvestedEvent { Plant = plant, X = x, Y = y, CaloriesGained = result.FinalCalories });
            EventBus.Publish(new PlantKilledEvent { Plant = plant, X = x, Y = y, Reason = "Harvested" });

            EventBus.Publish(new HarvestBreakdownReadyEvent { Result = result, Plant = plant , ScreenPos = screenPos});

            return result.FinalCalories;

            //old code for debug.
            /*
            // 1. Модификация от соседей (Generosity и т.п.)
            int modifiedCalories = _propertyResolver.ModifyHarvestByNeighbors(plant, baseCalories);

            // 2. Модификация от собственных свойств
            modifiedCalories = _propertyResolver.ModifyHarvest(plant, modifiedCalories);

            if (FloatingTextPool.Instance != null)
            {
                var worldPos = _boardRoot.GetCellView(x, y).transform.position;
                string sign = modifiedCalories > 0 ? "+" : "";
                Color color = modifiedCalories > 0 ? Color.white : Color.red;
                FloatingTextPool.Instance.ShowTextAtScreen(worldPos, $"{sign}{modifiedCalories}", color, 60f, 1.5f);
            }

            // 3. Удаляем растение с поля
            _runData.Board.RemovePlant(x, y);
            plant.CurrentCell = null;

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
            */
        }
    }
}