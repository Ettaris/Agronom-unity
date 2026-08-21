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
    public class BigBossEffect : GenomeEffectBase, IOnHarvest
    {
        public BigBossEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public int ModifyHarvest(PlantInstance plant, int baseCalories, GridBoard board)
        {
            Debug.Log(plant);
            if (plant.CurrentCell == null) return baseCalories;

            int x = plant.CurrentCell.X;
            int y = plant.CurrentCell.Y;

            bool hasUp = board.GetCell(x, y + 1)?.Plant != null;
            bool hasDown = board.GetCell(x, y - 1)?.Plant != null;
            bool hasLeft = board.GetCell(x - 1, y)?.Plant != null;
            bool hasRight = board.GetCell(x + 1, y)?.Plant != null;


            if (hasUp && hasDown && hasLeft && hasRight)
            {
                Debug.Log("BigBoss done well");
                EventBus.Publish(new EffectAppliedEvent { X = x, Y = y, Type = EffectType.Boost, Duration = 1f });
                return Mathf.RoundToInt(baseCalories * 2f);
            }
            else
            {
                Debug.Log("BigBoss done bad");
                EventBus.Publish(new EffectAppliedEvent { X = x, Y = y, Type = EffectType.Debuff, Duration = 0.8f });
                return Mathf.RoundToInt(baseCalories * 0.6f);
            }
        }
    }
}