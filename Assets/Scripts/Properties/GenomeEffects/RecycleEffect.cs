using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using Data;

namespace GenomeEffects
{
    public class RecycleEffect : GenomeEffectBase, IOnHarvest
    {
        public RecycleEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public int ModifyHarvest(PlantInstance plant, int baseCalories, GridBoard board)
        {
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (runData == null) return baseCalories;

            // Создаём новое растение (семя) с такой же максимальной ёмкостью генома, но без свойств
            var seed = new PlantInstance(plant.PlantData, plant.Genome.MaxCapacity);
            // (свойства не копируются)

            // Добавляем в руку
            runData.Hand.Add(seed);
            EventBus.Publish(new HandUpdatedEvent());

            return baseCalories;
        }
    }
}