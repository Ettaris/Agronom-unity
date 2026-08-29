using System;
using System.Collections.Generic;
using Gameplay;
using Properties.Interfaces;
using Systems;
using Unity.VisualScripting;

namespace Gameplay.Calculation
{
    /// <summary>
    /// Единый слой расчета урожая. Не изменяет состояние.
    /// </summary>
    public class HarvestCalculator
    {
        public HarvestResult Calculate(HarvestCalculationContext context)
        {
            var result = new HarvestResult();
            var plant = context.Plant;
            var board = context.Board;
            var provider = context.PropertyProvider;
            var discovered = context.DiscoveredGenomes;

            int baseCalories = plant.PlantData.baseCalories;
            result.BaseCalories = baseCalories;
            int current = baseCalories;

            // Соседи (расчёт)
            var neighbors = board.GetNeighbors(plant.CurrentCell.X, plant.CurrentCell.Y, false);
            foreach (var cell in neighbors)
            {
                if (cell.Plant == null) continue;
                var neighborPlant = cell.Plant;
                foreach (var prop in provider.GetProperties(neighborPlant))
                {
                    if (prop is IOnNeighborHarvestCalculation handler)
                    {
                        int old = current;
                        current = handler.CalculateNeighborHarvest(plant, current, board);
                        if (current != old)
                        {
                            bool isKnown = discovered.TryGetValue(neighborPlant.PlantData, out var list) && list.Contains(prop.Data);
                            result.Contributions.Add(new ModifierContribution
                            {
                                Source = isKnown ? prop.Data.propertyName : "???",
                                Description = isKnown ? prop.Data.description : "",
                                ValueChange = current - old,
                                IsMultiplier = false,
                                ModifierName = prop.Data.propertyName,
                                IsKnown = isKnown
                            });
                        }
                    }
                }
            }

            // Свойства самого растения (расчёт)
            foreach (var prop in provider.GetProperties(plant))
            {
                if (prop is IOnHarvestCalculation handler)
                {
                    int old = current;
                    current = handler.CalculateHarvest(plant, current, board);
                    if (current != old)
                    {
                        bool isKnown = discovered.TryGetValue(plant.PlantData, out var list) && list.Contains(prop.Data);
                        result.Contributions.Add(new ModifierContribution
                        {
                            Source = isKnown ? prop.Data.propertyName : "???",
                            Description = isKnown ? prop.Data.description : "",
                            ValueChange = current - old,
                            IsMultiplier = false,
                            ModifierName = prop.Data.propertyName,
                            IsKnown = isKnown
                        });
                    }
                }
            }

            result.FinalCalories = current;
            return result;
        }
    }
}