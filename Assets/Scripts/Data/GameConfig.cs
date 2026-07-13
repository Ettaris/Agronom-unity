using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Ссылки на пулы")]
        public PlantPool plantPool;
        public PropertyPool propertyPool;

        [Header("Лимиты")]
        public int maxPropertiesPerPlant = 3;
        public int initialHandSize = 5;
        public int maxHandSize = 10;
        public int dailyQuota = 50; // калорий в день
        public int totalDays = 10;

        [Header("Стартовые параметры")]
        public int startingCalories = 0;
        public int startingDeckSize = 15; // сколько растений в колоде в начале
    }
}