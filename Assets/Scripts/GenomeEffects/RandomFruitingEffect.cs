using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using System.Collections.Generic;
using Data;
using Systems;

namespace GenomeEffects
{
    public class RandomFruitingEffect : GenomeEffectBase, IOnDestroyedWithCoords
    {
        public RandomFruitingEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnDestroyed(PlantInstance plant, int x, int y, GridBoard board)
        {
            // Найти случайную свободную клетку
            var freeCells = new List<Cell>();
            for (int i = 0; i < board.Width; i++)
                for (int j = 0; j < board.Height; j++)
                    if (board.IsFree(i, j)) freeCells.Add(board.GetCell(i, j));

            if (freeCells.Count > 0)
            {
                var cell = freeCells[UnityEngine.Random.Range(0, freeCells.Count)];
                var sprout = new PlantInstance(plant.PlantData, plant.Genome.MaxCapacity);
                if (board.PlacePlant(sprout, cell.X, cell.Y))
                {
                    sprout.CurrentCell = cell;
                    ServiceLocator.Get<PropertyResolverSystem>().RegisterPlant(sprout);
                }
            }
        }
    }
}