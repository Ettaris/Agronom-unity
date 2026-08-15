using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Infrastructure.Events;

namespace GenomeEffects
{
    public class HugeEffect : GenomeEffectBase, IOnHarvest, IOnPlantPlaced
    {
        public HugeEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public int ModifyHarvest(PlantInstance plant, int baseCalories, GridBoard board)
        {
            return baseCalories * 2;
        }

        public void OnPlantPlaced(PlantInstance plant, int x, int y, GridBoard board)
        {
            // 4 смежные клетки превращаются в сорняк
            var neighbors = board.GetNeighbors(x, y, false);
            var weedData = ServiceLocator.Get<GameConfig>().weedPlantData;
            if (weedData == null) return;

            foreach (var cell in neighbors)
            {
                if (cell.Plant == null)
                {
                    var weed = new PlantInstance(weedData, 0);
                    if (board.PlacePlant(weed, new Vector2Int(cell.X, cell.Y)))
                    {
                        weed.CurrentCell = cell;
                        EventBus.Publish(new PlantPlacedEvent { Plant = weed, X = cell.X, Y = cell.Y });
                        EventBus.Publish(new EffectAppliedEvent { Type = EffectType.Weed, Duration = 0.75f, X = cell.X, Y = cell.Y });
                    }
                }
            }
        }
    }
}