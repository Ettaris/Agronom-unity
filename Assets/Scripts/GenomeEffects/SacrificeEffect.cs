using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Data;
using Systems;
using Infrastructure.Events;

namespace GenomeEffects
{
    public class SacrificeEffect : GenomeEffectBase, IOnPlantPlaced
    {
        public SacrificeEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnPlantPlaced(PlantInstance plant, int x, int y, GridBoard board)
        {
            // Выращиваем растение слева (x-1, y)
            var leftCell = board.GetCell(x - 1, y);
            if (leftCell != null && leftCell.Plant == null)
            {
                var clone = new PlantInstance(plant.PlantData, plant.Genome.MaxCapacity);
                // Копируем свойства? Можно без них.
                if (board.PlacePlant(clone, x - 1, y))
                {
                    clone.CurrentCell = leftCell;
                    ServiceLocator.Get<PropertyResolverSystem>().RegisterPlant(clone);
                }
            }

            // Умираем сами
            board.RemovePlant(x, y);
            plant.CurrentCell = null;
            EventBus.Publish(new PlantKilledEvent { Plant = plant, X = x, Y = y, Reason = "Sacrifice" });
        }
    }
}