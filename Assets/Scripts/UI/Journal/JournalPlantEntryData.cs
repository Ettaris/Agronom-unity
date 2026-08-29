using UnityEngine;
using Data;
using System.Collections.Generic;

public class JournalPlantEntryData : IJournalEntryData
{
    private PlantData _plantData;
    private int _analysisCount;

    public JournalPlantEntryData(PlantData plantData, int count)
    {
        _plantData = plantData;
        _analysisCount = count;
    }

    public string Title => _plantData.itemName;
    public Sprite Icon => _plantData.icon;
    public string Description => $"Изучено: {_analysisCount} раз\nКалории: {_plantData.baseCalories}\nРост: {_plantData.growthTime} дн.";
    public List<GenomePropertyData> Properties => new List<GenomePropertyData>();
    public bool IsPermanent => false;
    public string PermanentFor => "";
    public int Count => _analysisCount;
}