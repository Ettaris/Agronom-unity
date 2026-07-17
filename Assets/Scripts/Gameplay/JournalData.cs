using System.Collections.Generic;
using Data;

namespace Gameplay
{
    [System.Serializable]
    public class JournalData
    {
        private readonly Dictionary<GenomePropertyData, int> _discoveredProperties = new Dictionary<GenomePropertyData, int>();

        public void AddEntry(GenomePropertyData property)
        {
            if (_discoveredProperties.ContainsKey(property))
                _discoveredProperties[property]++;
            else
                _discoveredProperties[property] = 1;
        }

        public bool IsPropertyDiscovered(GenomePropertyData property) => _discoveredProperties.ContainsKey(property);

        public void Clear() => _discoveredProperties.Clear();

        public Dictionary<GenomePropertyData, int> GetAllEntries()
        {
            return new Dictionary<GenomePropertyData, int>(_discoveredProperties);
        }
    }
}