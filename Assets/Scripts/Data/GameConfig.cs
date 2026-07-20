using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Пулы данных")]
        public PlantPool plantPool;
        public GenomePool genomePool;
        public FermentPool fermentPool;   // новый пул ферментов
        public BatteryPool batteryPool;   // новый пул батареек

        [Header("Префаб сорняк")]
        public PlantData weedPlantData;

        [Header("Лимиты")]
        public int maxHandSize = 10;
        public int initialHandSize = 5;
        public int maxGenomeCapacity = 60; // дефолтный, но перекрывается растением
        public int dailyQuota = 50;
        public int totalCaloriesGoal = 300;
        public int totalDays = 10;
        public int defaultMaxGenomeCapacity = 60;
        public int cardsPerDay = 6;
        public int cardsToSelect = 2;

        [Header("Стартовые параметры")]
        public int startingCalories = 0;
        public int startingDeckSize = 15;

        [Header("Настройки поля")]
        public int boardWidth = 5;
        public int boardHeight = 5;
    }
}