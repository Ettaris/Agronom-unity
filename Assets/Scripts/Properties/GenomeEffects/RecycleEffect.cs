using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using Data;

namespace GenomeEffects
{
    public class RecycleEffect : GenomeEffectBase, IOnHarvestCalculation, IOnHarvestApplication
    {
        public RecycleEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void ApplyHarvest(PlantInstance plant, int baseCalories, IGridBoard board)
        {
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            var config = ServiceLocator.Get<GameConfig>();

            // Создаём новое растение (семя) клон
            var sprout = PlantFactory.ClonePlant(plant, config);

            // Добавляем в руку
            runData.Hand.Add(sprout);
            EventBus.Publish(new HandUpdatedEvent());
        }

        public int CalculateHarvest(PlantInstance plant, int baseCalories, IGridBoard board)
        {
            return baseCalories;
        }

    }
}