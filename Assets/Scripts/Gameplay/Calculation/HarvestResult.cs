using System.Collections.Generic;

namespace Gameplay.Calculation
{
    public class HarvestResult
    {
        public int BaseCalories { get; set; }
        public int FinalCalories { get; set; }
        public List<ModifierContribution> Contributions { get; set; } = new List<ModifierContribution>();
        public float FinalMultiplier { get; set; } = 1f;

        public override string ToString()
        {
            return $"Base: {BaseCalories}, Final: {FinalCalories}, Steps: {Contributions.Count}";
        }
    }
}