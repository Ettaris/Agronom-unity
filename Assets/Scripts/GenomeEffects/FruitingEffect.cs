using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Data;
using Systems;

namespace GenomeEffects
{
    public class FruitingEffect : GenomeEffectBase, IOnDestroyedWithCoords
    {
        public FruitingEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnDestroyed(PlantInstance plant, int x, int y, GridBoard board)
        {
            // Оставляем росток на том же месте
            var sprout = new PlantInstance(plant.PlantData, plant.Genome.MaxCapacity);
            if (board.PlacePlant(sprout, x, y))
            {
                sprout.CurrentCell = board.GetCell(x, y);
                ServiceLocator.Get<PropertyResolverSystem>().RegisterPlant(sprout);
            }
        }
    }
}