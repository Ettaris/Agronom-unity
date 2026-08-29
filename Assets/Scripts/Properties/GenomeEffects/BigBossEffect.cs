using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using UnityEngine;
using Data;
using Infrastructure.Events;

namespace GenomeEffects
{
    /// <summary>
    /// ≈сли у владельца есть соседи сверху, снизу, слева и справа (все четыре клетки зан€ты),
    /// то владелец получает +100% к калори€м при сборе.
    /// ≈сли хот€ бы один сосед отсутствует, владелец получает -40% к калори€м.
    /// </summary>

    public class BigBossEffect : GenomeEffectBase, IOnHarvestCalculation, IOnHarvestApplication
    {
        public BigBossEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        private bool CheckAllNeighbors(PlantInstance plant, IGridBoard board, out int x, out int y)
        {
            x = plant.CurrentCell.X;
            y = plant.CurrentCell.Y;
            bool hasUp = board.GetCell(x, y + 1)?.Plant != null;
            bool hasDown = board.GetCell(x, y - 1)?.Plant != null;
            bool hasLeft = board.GetCell(x - 1, y)?.Plant != null;
            bool hasRight = board.GetCell(x + 1, y)?.Plant != null;
            return hasUp && hasDown && hasLeft && hasRight;
        }

        public int CalculateHarvest(PlantInstance plant, int baseCalories, IGridBoard board)
        {
            if (plant.CurrentCell == null) return baseCalories;
            bool allNeighbors = CheckAllNeighbors(plant, board, out _, out _);
            return allNeighbors ? Mathf.RoundToInt(baseCalories * 2f) : Mathf.RoundToInt(baseCalories * 0.6f);
        }

        public void ApplyHarvest(PlantInstance plant, int baseCalories, IGridBoard board)
        {
            if (plant.CurrentCell == null) return;
            bool allNeighbors = CheckAllNeighbors(plant, board, out int x, out int y);
            EffectType type = allNeighbors ? EffectType.Boost : EffectType.Debuff;
            EventBus.Publish(new EffectAppliedEvent { X = x, Y = y, Type = type, Duration = 1f });
        }
    }
}
