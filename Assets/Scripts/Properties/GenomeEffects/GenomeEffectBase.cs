using Gameplay;
using Data;

namespace GenomeEffects
{
    /// <summary>
    /// Базовый класс для всех эффектов генома.
    /// Наследует GenomePropertyInstance и служит маркером для выбора в инспекторе.
    /// </summary>
    public abstract class GenomeEffectBase : GenomePropertyInstance
    {
        protected GenomeEffectBase(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }
    }
}