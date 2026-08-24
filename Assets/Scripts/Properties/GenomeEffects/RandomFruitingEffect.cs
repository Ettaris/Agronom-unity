using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using System.Collections.Generic;
using Data;
using Systems;
using UnityEngine;
using Infrastructure.Events;
using Managers;

namespace GenomeEffects
{
    public class RandomFruitingEffect : GenomeEffectBase, IOnDestroyedWithCoords
    {
        public RandomFruitingEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnDestroyed(PlantInstance plant, int x, int y, GridBoard board)
        {
            var config = ServiceLocator.Get<GameConfig>();
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;

            var freeCells = new List<Cell>();
            for (int i = 0; i < board.Width; i++)
                for (int j = 0; j < board.Height; j++)
                    if (board.IsFree(i, j)) freeCells.Add(board.GetCell(i, j));

            if (freeCells.Count > 0)
            {
                var cell = freeCells[Random.Range(0, freeCells.Count)];
                Vector2Int pos = new Vector2Int(cell.X, cell.Y);
                var sprout = PlantFactory.ClonePlant(plant, config);
                if (board.PlacePlant(sprout, pos))
                {
                    sprout.CurrentCell = cell;
                    EventBus.Publish(new PlantPlacedEvent { Plant = sprout, X = cell.X, Y = cell.Y });
                    EventBus.Publish(new EffectAppliedEvent { Type = EffectType.Grow, X = cell.X, Y = cell.Y, Duration = 0.6f});
                }
            }
        }
    }
}