using Data;
using Gameplay;
using GenomeEffects;
using Properties.Interfaces;
using UnityEngine;


namespace GenomeEffects
{
    public class Upper10Effect : GenomeEffectBase, IOnHarvest
    {
        public Upper10Effect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public int ModifyHarvest(PlantInstance plant, int baseCalories, GridBoard board)
        {

            return baseCalories + 10;
        }
    }
}