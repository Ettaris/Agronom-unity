using UnityEngine;

namespace Data
{
    public abstract class ItemData : UniqueScriptableObject
    {
        [Header("Общие данные предмета")]
        public string itemName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public Rarity rarity = Rarity.Common;
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}