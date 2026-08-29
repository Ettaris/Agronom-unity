namespace Gameplay.Calculation
{
    public class ModifierContribution
    {
        public string Source { get; set; } // Например, "Base", "BigEffect", "Generosity"
        public string Description { get; set; }
        public int ValueChange { get; set; } 
        public float Multiplier { get; set; } = 1f;
        public string ModifierName { get; set; }
        public bool IsMultiplier { get; set; } = false;
        public bool IsKnown { get; set; } = true;
    }
}