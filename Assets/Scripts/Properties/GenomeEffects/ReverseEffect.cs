using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using UnityEngine;
using Data;

namespace GenomeEffects
{
    /// <summary>
    /// При сборе: 
    /// - базовые калории уменьшаются на 10 (минимум 1)
    /// - за каждый модификатор на соседях (плюсом) +15% (максимум +90%)
    /// </summary>
    public class ReverseEffect : GenomeEffectBase, IOnHarvestCalculation
    {
        public ReverseEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public int CalculateHarvest(PlantInstance plant, int baseCalories, IGridBoard board)
        {
            if (plant.CurrentCell == null) return baseCalories;
            int x = plant.CurrentCell.X;
            int y = plant.CurrentCell.Y;

            int modifiedBase = Mathf.Max(1, baseCalories - 10);

            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(-1, 0), 
                new Vector2Int(1, 0), 
                new Vector2Int(0, -1), 
                new Vector2Int(0, 1)   
            };

            int totalModifiers = 0;
            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                var cell = board.GetCell(nx, ny);
                if (cell != null && cell.Plant != null)
                {
                    totalModifiers += cell.Plant.Genome.Properties.Count;
                }
            }

            float bonus = 0.15f * Mathf.Min(totalModifiers, 6);
            int finalCalories = Mathf.RoundToInt(modifiedBase * (1f + bonus));

            return finalCalories;
        }
    }
}