using System;
using System.Collections.Generic;
using Gameplay;
using Data;

[Serializable]
public class SaveData
{
    public string version;
    public DateTime saveTime;
    public int seed;
    public int currentDay;
    public int calories;
    public List<SerializedPlant> boardPlants;
    public List<SerializedItem> handItems;
    public List<SerializedItem> deckItems;
    public JournalData journal;
    public bool isQuotaReached;
    public int dailyQuota;

    // Для сериализации растений (состояние поля)
    [Serializable]
    public class SerializedPlant
    {
        public int x, y;
        public string plantDataId;   // уникальный ID PlantData
        public float growthProgress;
        public List<SerializedProperty> properties;
        public int maxGenomeCapacity;
        public int currentGenomeWeight;
    }

    [Serializable]
    public class SerializedProperty
    {
        public string propertyDataId;
        public int stacks;
    }

    [Serializable]
    public class SerializedItem
    {
        public string itemDataId;
        public int quantity; // для стаков
        public bool isPlant; // true – растение, false – обычный предмет
        // дополнительные поля для растения (если нужно)
        public SerializedPlant plantData;
    }
}