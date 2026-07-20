using Data;
using Gameplay;
using Properties.Interfaces;
using UnityEngine;

namespace GenomeEffects
{
    public class BigEffect : GenomeEffectBase, IOnHarvest, IModifyGrowth
    {
        public BigEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public int ModifyHarvest(PlantInstance plant, int baseCalories, GridBoard board)
        {
            return Mathf.RoundToInt(baseCalories * 1.3f);
        }

        public float ModifyGrowth(PlantInstance plant, float currentGrowth)
        {
            // +1 день к росту: уменьшаем прирост за день на 50% (т.е. замедляем вдвое) TODO: wrong
            return currentGrowth * 0.5f;
        }
    }
}