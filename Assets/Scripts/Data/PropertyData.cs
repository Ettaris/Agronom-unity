using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PropertyData", menuName = "Game/Property Data")]
    public class PropertyData : ScriptableObject
    {
        [Header("Основные")]
        public string propertyName;
        public Sprite icon;
        [TextArea(3, 5)]
        public string description;
        public Rarity rarity = Rarity.Common;
        public PropertyType type = PropertyType.Positive;

        [Header("Параметры (для некоторых свойств)")]
        public float modifierValue = 1f; // например, множитель калорий
        public int stackLimit = 1; // 0 = без ограничений
    }

    public enum PropertyType
    {
        Positive,
        Negative,
        Neutral
    }
}