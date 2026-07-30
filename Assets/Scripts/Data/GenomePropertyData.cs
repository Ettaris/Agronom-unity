using Gameplay;
using GenomeEffects;
using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GenomePropertyData", menuName = "Game/Genome Property Data")]
    public class GenomePropertyData : UniqueScriptableObject
    {
        public string propertyName;
        public string description;
        public Sprite icon;
        public Rarity rarity;
        public int genomeCost; // стоимость в очках генома

        [Header("Effect")]
        [SerializeField] private string _effectClassName; // им€ класса эффекта (выбираетс€ через кастомный редактор)

        public string EffectClassName => _effectClassName;

        /// <summary>
        /// —оздаЄт экземпл€р эффекта по имени класса.
        /// </summary>
        public GenomePropertyInstance CreateEffect(int stacks = 1)
        {
            if (string.IsNullOrEmpty(_effectClassName))
            {
                Debug.Log($"CreateEffect: {propertyName} has no effect class, using base instance");
                return new GenomePropertyInstance(this, stacks);
            }

            Type type = Type.GetType($"GenomeEffects.{_effectClassName}");
            if (type == null)
            {
                Debug.LogWarning($"CreateEffect: Class 'GenomeEffects.{_effectClassName}' not found for {propertyName}");
                return new GenomePropertyInstance(this, stacks);
            }

            if (!typeof(GenomeEffectBase).IsAssignableFrom(type))
            {
                Debug.LogWarning($"CreateEffect: Class '{type.Name}' does not inherit GenomeEffectBase");
                return new GenomePropertyInstance(this, stacks);
            }

            var instance = Activator.CreateInstance(type, this, stacks) as GenomePropertyInstance;
            Debug.Log($"CreateEffect: Created {instance.GetType().Name} for {propertyName}");
            return instance ?? new GenomePropertyInstance(this, stacks);
        }

        public void SetEffectClassName(string className)
        {
            _effectClassName = className;
        }
    }
}