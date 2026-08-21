using Data;
using Gameplay;
using Properties.Interfaces;

namespace GenomeEffects
{
    public class HyperSpeedEffect : GenomeEffectBase, IModifyGrowth
    {
        public HyperSpeedEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public float ModifyGrowth(PlantInstance plant, float currentGrowth)
        {
            return 1f; // рост за один день
        }
    }
}