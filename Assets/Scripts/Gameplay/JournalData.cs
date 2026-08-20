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
            var entry = plantEntries.Find(e => e.plantData == plant.PlantData);
            if (entry == null)
            {
                entry = new JournalPlantEntry(plant.PlantData);
                plantEntries.Add(entry);
            }
            entry.AddProperty(property, isPermanent);
        }

        public List<IJournalEntryData> GetPlantEntries()
        {
            var result = new List<IJournalEntryData>();
            foreach (var entry in plantEntries)
                result.Add(new JournalPlantEntryData(entry));
            return result;
        }

        public List<IJournalEntryData> GetModifierEntries()
        {
            var dict = new Dictionary<GenomePropertyData, (bool isPermanent, List<string> plants)>();
            foreach (var plantEntry in plantEntries)
            {
                foreach (var prop in plantEntry.discoveredProperties)
                {
                    if (!dict.ContainsKey(prop))
                    {
                        dict[prop] = (false, new List<string>());
                    }
                    // Проверяем, является ли prop перманентным для этого растения
                    if (plantEntry.permanentProperty == prop)
                    {
                        dict[prop] = (true, dict[prop].plants);
                    }
                    // Добавляем название растения (если ещё нет)
                    if (!dict[prop].plants.Contains(plantEntry.plantData.itemName))
                    {
                        dict[prop].plants.Add(plantEntry.plantData.itemName);
                    }
                }
            }

            var result = new List<IJournalEntryData>();
            foreach (var kvp in dict)
            {
                string permanentFor = kvp.Value.isPermanent ? string.Join(", ", kvp.Value.plants) : "";
                result.Add(new JournalModifierEntryData(kvp.Key, kvp.Value.isPermanent, permanentFor));
            }
            return result;
        }

        public bool IsPropertyDiscovered(PlantData plant, GenomePropertyData property)
        {
            var entry = plantEntries.Find(e => e.plantData == plant);
            return entry != null && entry.HasProperty(property);
        }
    }
}