using UnityEngine;

namespace Data
{
    [System.Serializable]
    public struct StageData
    {
        public int totalDays;          // общее количество дней к концу этапа (нарастающий итог)
        public int requiredCalories;   // необходимое общее количество калорий к концу этапа
    }

    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Пулы данных")]
        public PlantPool plantPool;
        public GenomePool genomePool;
        public FermentPool fermentPool;   // новый пул ферментов
        public BatteryPool batteryPool;   // новый пул батареек                        
        public OfferGenerationConfig offerGenerationConfig;
        public PlantRarityConfig plantRarityConfig;
        public PlantRarityPool plantRarityPool;
        public StageData[] stages;

        [Header("Префаб сорняк")]
        public PlantData weedPlantData;

        [Header("Лимиты")]
        public int maxHandSize = 10;
        public int initialHandSize = 5;
        public int maxGenomeCapacity = 60; // дефолтный, но перекрывается растением
        public int totalCaloriesGoal = 300;
        public int totalDays = 10;
        public int defaultMaxGenomeCapacity = 60;
        public int cardsPerDay = 6;
        public int cardsToSelect = 2;
        public int maxPropertiesPerPlant = 2;

        [Header("Стартовые параметры")]
        public int startingCalories = 0;
        public int startingDeckSize = 15;

        [Header("Настройки поля")]
        public int boardWidth = 5;
        public int boardHeight = 5;
    }
}