using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Data;
using Systems;
using UnityEngine;
using Infrastructure.Events;
using Managers;

namespace GenomeEffects
{
    public class FruitingEffect : GenomeEffectBase, IOnDestroyedWithCoords
    {
        public FruitingEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnDestroyed(PlantInstance plant, int x, int y, GridBoard board)
        {
            var config = ServiceLocator.Get<GameConfig>();
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            var genomePool = config.genomePool;
            var maxProperties = config.maxPropertiesPerPlant;

            var sprout = PlantFactory.CreatePlantWithProperties(plant.PlantData, runData.Random, config, runData);


            if (board.PlacePlant(sprout, x, y))
            {
                sprout.CurrentCell = board.GetCell(x, y);
                var resolver = ServiceLocator.Get<PropertyResolverSystem>();
                resolver.RegisterPlant(sprout);
                EventBus.Publish(new PlantPlacedEvent { Plant = sprout, X = x, Y = y });
            }
        }
    }
}