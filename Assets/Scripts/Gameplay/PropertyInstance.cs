using Data;

namespace Gameplay
{
    public class PropertyInstance
    {
        public readonly PropertyData Data;
        public int Stacks { get; set; } // для свойств, которые могут стакаться
        public float RemainingDuration { get; set; } = -1f; // -1 = бесконечно

        public PropertyInstance(PropertyData data, int initialStacks = 1)
        {
            Data = data;
            Stacks = initialStacks;
        }
    }
}