using System;
using System.Collections.Generic;
using Data;

namespace Gameplay
{
    [Serializable]
    public class JournalData
    {
        public List<JournalPlantEntry> plantEntries = new List<JournalPlantEntry>();

        public void AddOrUpdatePlant(PlantInstance plant, GenomePropertyData property, bool isPermanent)
        {
            if (plant == null || property == null) return;
            var entry = plantEntries.Find(e => e.plantData == plant.PlantData);
            if (entry == null)
            {
                entry = new JournalPlantEntry(plant.PlantData);
                plantEntries.Add(entry);
            }
            entry.AddProperty(property, isPermanent);
        }

        public bool IsPropertyDiscovered(PlantData plant, GenomePropertyData property)
        {
            var entry = plantEntries.Find(e => e.plantData == plant);
            return entry != null && entry.HasProperty(property);
        }

        public JournalPlantEntry GetPlantEntry(PlantData plant)
        {
            return plantEntries.Find(e => e.plantData == plant);
        }

        public void Clear() => plantEntries.Clear();
    }
}