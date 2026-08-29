using Data;
using Gameplay;
using GenomeEffects;
using Properties.Interfaces;
using UnityEngine;


namespace GenomeEffects
{
    public class Upper10Effect : GenomeEffectBase, IOnHarvestCalculation
    {
        public Upper10Effect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public int CalculateHarvest(PlantInstance plant, int baseCalories, IGridBoard board)
        {
            return baseCalories + 10;
        }
    }
}