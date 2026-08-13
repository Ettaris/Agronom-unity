using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PlantData", menuName = "Game/Plant Data")]
    public class PlantData : ItemData
    {
        [Header("Характеристики растения")]
        public float growthTime = 10f; // в днях
        public int baseCalories = 10;
        public GenomePropertyData fixedPermanentModifier;
        public Vector2Int size = Vector2Int.one; // размер на поле (пока только 1x1)
        public int maxGenomeCapacity = 60; // скрытая характеристика

        [Header("Sprites")]
        public Sprite[] growthSprites;   // стадии роста (росток, средний, зрелый)
        public Sprite[] mutationStages;  // стадии мутации (0-100% заполнения)
    }
}