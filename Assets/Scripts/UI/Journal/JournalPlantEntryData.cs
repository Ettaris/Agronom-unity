using UnityEngine;
using System.Collections.Generic;
using Data;
using Gameplay;

public class JournalPlantEntryData : IJournalEntryData
{
    private JournalPlantEntry _entry;

    public JournalPlantEntryData(JournalPlantEntry entry)
    {
        _entry = entry;
    }

    public string Title => _entry.plantData.itemName;
    public Sprite Icon => _entry.plantData.icon;
    public string Description => $"Проанализировано: {_entry.discoveryCount} раз";
    public List<GenomePropertyData> Properties => _entry.discoveredProperties;
    public bool IsPermanent => false;
    public string PermanentFor => "";
    public int Count => _entry.discoveryCount;
}