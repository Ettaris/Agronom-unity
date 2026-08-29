using System;
using System.Collections.Generic;
using System.Linq;
using Data;

namespace Gameplay
{
    [Serializable]
    public class JournalData
    {
        public List<string> studiedPlantIds = new List<string>();// ID растений, которые были изучены
        public Dictionary<string, int> plantAnalysisCount = new Dictionary<string, int>();// ID растения → количество анализов
        public List<string> discoveredModifierIds = new List<string>();// ID открытых модификаторов
        public Dictionary<string, List<string>> permanentModifierForPlants = new Dictionary<string, List<string>>(); // ID модификатора → список ID растений, для которых он перманентен


        public void StudyPlant(PlantData plant, int analysisCount = 1)
        {
            if (plant == null || string.IsNullOrEmpty(plant.Id)) return;
            if (!studiedPlantIds.Contains(plant.Id))
                studiedPlantIds.Add(plant.Id);
            if (!plantAnalysisCount.ContainsKey(plant.Id))
                plantAnalysisCount[plant.Id] = 0;
            plantAnalysisCount[plant.Id] += analysisCount;
        }

        public void DiscoverModifier(GenomePropertyData modifier, PlantInstance plant, bool isPermanent)
        {
            if (modifier == null || string.IsNullOrEmpty(modifier.Id)) return;
            if (!discoveredModifierIds.Contains(modifier.Id))
                discoveredModifierIds.Add(modifier.Id);

            if (isPermanent && plant != null)
            {
                string plantId = plant.PlantData.Id;
                if (!permanentModifierForPlants.ContainsKey(modifier.Id))
                    permanentModifierForPlants[modifier.Id] = new List<string>();
                if (!permanentModifierForPlants[modifier.Id].Contains(plantId))
                    permanentModifierForPlants[modifier.Id].Add(plantId);
            }
        }

        // ---- Запросы ----

        public bool IsPlantStudied(PlantData plant) => plant != null && studiedPlantIds.Contains(plant.Id);

        public bool IsModifierDiscovered(GenomePropertyData modifier) => modifier != null && discoveredModifierIds.Contains(modifier.Id);

        public int GetPlantAnalysisCount(PlantData plant)
        {
            if (plant == null || !plantAnalysisCount.ContainsKey(plant.Id)) return 0;
            return plantAnalysisCount[plant.Id];
        }

        public List<IJournalEntryData> GetPlantEntries(GameConfig config)
        {
            var result = new List<IJournalEntryData>();
            foreach (var plantId in studiedPlantIds)
            {
                var plantData = FindPlantDataById(plantId, config);
                if (plantData == null) continue;
                int count = GetPlantAnalysisCount(plantData);
                result.Add(new JournalPlantEntryData(plantData, count));
            }
            return result;
        }

        public List<IJournalEntryData> GetModifierEntries(GameConfig config)
        {
            var result = new List<IJournalEntryData>();
            foreach (var modId in discoveredModifierIds)
            {
                var modifierData = FindModifierDataById(modId, config);
                if (modifierData == null) continue;
                bool isPermanent = permanentModifierForPlants.ContainsKey(modId);
                List<string> plantNames = new List<string>();
                if (isPermanent)
                {
                    foreach (var plantId in permanentModifierForPlants[modId])
                    {
                        var plantData = FindPlantDataById(plantId, config);
                        if (plantData != null)
                            plantNames.Add(plantData.itemName);
                    }
                }
                string permanentFor = string.Join(", ", plantNames);
                result.Add(new JournalModifierEntryData(modifierData, isPermanent, permanentFor));
            }
            return result;
        }

        // ---- Вспомогательные методы поиска (нужен GameConfig) ----
        private PlantData FindPlantDataById(string id, GameConfig config)
        {
            if (config.plantRarityPool != null)
            {
                var pool = config.plantRarityPool;
                var all = pool.commonPlants.Concat(pool.uncommonPlants)
                            .Concat(pool.rarePlants)
                            .Concat(pool.epicPlants)
                            .Concat(pool.legendaryPlants);
                return all.FirstOrDefault(p => p.Id == id);
            }
            return null;
        }

        private GenomePropertyData FindModifierDataById(string id, GameConfig config)
        {
            if (config.genomeRarityPool != null)
            {
                var pool = config.genomeRarityPool;
                var all = pool.common.Concat(pool.uncommon)
                            .Concat(pool.rare)
                            .Concat(pool.epic)
                            .Concat(pool.legendary);
                return all.FirstOrDefault(m => m.Id == id);
            }
            return null;
        }
    }
}