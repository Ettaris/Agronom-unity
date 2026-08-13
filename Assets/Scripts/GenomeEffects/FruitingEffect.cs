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
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (runData == null) return;

            var config = ServiceLocator.Get<GameConfig>();
            var random = runData.Random;

            var sprout = PlantFactory.CreatePlantWithProperties(plant.PlantData, random, config, runData);

            Vector2Int pos = new Vector2Int(x, y);
            if (board.CanPlace(pos, sprout.PlantData.size))
            {
                board.PlacePlant(sprout, pos);
                EventBus.Publish(new PlantPlacedEvent { Plant = sprout, X = x, Y = y });
            }
            else
            {
                Debug.LogWarning("FruitingEffect: Cannot place sprout at original position");
            }
        }
    }
}