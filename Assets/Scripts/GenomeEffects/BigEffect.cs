using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Properties.Interfaces;
using UnityEngine;

namespace GenomeEffects
{
    public class BigEffect : GenomeEffectBase, IOnHarvest, IModifyGrowth
    {
        public BigEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }



        public int ModifyHarvest(PlantInstance plant, int baseCalories, GridBoard board)
        {
            EventBus.Publish(new EffectAppliedEvent { Type = EffectType.Boost, X = plant.Position.x, Y = plant.Position.y });
            return Mathf.RoundToInt(baseCalories * 1.3f);
        }

        public float ModifyGrowth(PlantInstance plant, float currentGrowth)
        {
            return currentGrowth * 0.5f;
        }
    }
}