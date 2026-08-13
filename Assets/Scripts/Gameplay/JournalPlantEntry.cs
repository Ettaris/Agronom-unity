using System;
using System.Collections.Generic;
using Data;

namespace Gameplay
{
    [Serializable]
    public class JournalPlantEntry
    {
        public PlantData plantData;
        public List<GenomePropertyData> discoveredProperties = new List<GenomePropertyData>();
        public GenomePropertyData permanentProperty;
        public int discoveryCount;

        public JournalPlantEntry(PlantData plantData)
        {
            this.plantData = plantData;
            discoveryCount = 1;
        }

        public void AddProperty(GenomePropertyData property, bool isPermanent)
        {
            if (!discoveredProperties.Contains(property))
                discoveredProperties.Add(property);
            if (isPermanent)
                permanentProperty = property;
            discoveryCount++;
        }

        public bool HasProperty(GenomePropertyData property) => discoveredProperties.Contains(property);
    }
}