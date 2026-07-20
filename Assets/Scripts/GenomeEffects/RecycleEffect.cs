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
            // Добавляем семя в руку
            var seed = new ItemInstance(plant.PlantData);
            var hand = ServiceLocator.Get<RunManager>().CurrentRunData.Hand;
            hand.Add(seed);
            EventBus.Publish(new HandUpdatedEvent());
            return baseCalories;
        }
    }
}