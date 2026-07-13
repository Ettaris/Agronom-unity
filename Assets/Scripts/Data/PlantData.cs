using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PlantData", menuName = "Game/Plant Data")]
    public class PlantData : ScriptableObject
    {
        [Header("Основные")]
        public string plantName;
        public Sprite icon;
        public float growthTime = 10f; // в секундах или днях
        public int baseCalories = 10;
        public Vector2Int size = Vector2Int.one; // размер на поле (1x1 по умолчанию)
        public Rarity rarity = Rarity.Common;
        public int cost = 1; // стоимость для покупки или генерации

        [Header("Описание")]
        [TextArea(3, 5)]
        public string description;
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