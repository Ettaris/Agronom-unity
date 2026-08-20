using UnityEngine;

namespace Data
{
    [System.Serializable]
    public struct StageData
    {
        public int totalDays;          // общее количество дней к концу этапа (нарастающий итог)
        public int requiredCalories;   // необходимое общее количество калорий к концу этапа
    }
    /// <summary>
    /// Отвечает за конфигурацию забега. Включает в себя все данные, лимиты и параметры игры.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Пулы данных")]
        public PlantPool plantPool;
        public GenomePool genomePool;
        public FermentPool fermentPool;   // новый пул ферментов
        public BatteryPool batteryPool;   // новый пул батареек                        
        public PlantRarityPool plantRarityPool;
        public GenomeRarityPool genomeRarityPool;
        public PlantRarityConfig plantRarityConfig;
        public ModifierAssignmentConfig modifierConfig;
        public OfferGenerationConfig offerGenerationConfig;
        public StageData[] stages;

        [Header("Префаб сорняк")]
        public PlantData weedPlantData;

        [Header("Лимиты")]
        public int maxHandSize = 10;
        public int initialHandSize = 5;
        public int defaultMaxGenomeCapacity = 60;
        public int cardsPerDay = 6;
        public int cardsToSelect = 2;
        public int maxPropertiesPerPlant = 1;

        [Header("Стартовые параметры")]
        public int startingCalories = 0;
        public int startingDeckSize = 25;

        [Header("Настройки поля")]
        public int boardWidth = 5;
        public int boardHeight = 5;
    }
}