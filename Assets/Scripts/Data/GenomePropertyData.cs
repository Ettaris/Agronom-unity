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
        [SerializeField] private string _effectClassName; // имя класса эффекта (выбирается через кастомный редактор)

        public string EffectClassName => _effectClassName;

        /// <summary>
        /// Создаёт экземпляр эффекта по имени класса.
        /// </summary>
        public GenomePropertyInstance CreateEffect(int stacks = 1)
        {
            if (string.IsNullOrEmpty(_effectClassName))
                return new GenomePropertyInstance(this, stacks);

            Type type = Type.GetType($"GenomeEffects.{_effectClassName}");
            if (type == null)
            {
                Debug.LogError($"Effect class '{_effectClassName}' not found. Using base instance.");
                return new GenomePropertyInstance(this, stacks);
            }

            if (!typeof(GenomeEffectBase).IsAssignableFrom(type))
            {
                Debug.LogError($"Class '{_effectClassName}' does not inherit GenomeEffectBase.");
                return new GenomePropertyInstance(this, stacks);
            }

            // Создаём экземпляр через конструктор (GenomePropertyData, int)
            var instance = Activator.CreateInstance(type, this, stacks) as GenomePropertyInstance;
            return instance ?? new GenomePropertyInstance(this, stacks);
        }

        public void SetEffectClassName(string className)
        {
            _effectClassName = className;
        }
    }
}