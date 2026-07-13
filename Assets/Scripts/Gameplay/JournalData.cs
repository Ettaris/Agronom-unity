using System.Collections.Generic;
using Data;

namespace Gameplay
{
    public class JournalData
    {
        private readonly Dictionary<PropertyData, int> _discoveredProperties = new Dictionary<PropertyData, int>();

        public void AddEntry(PropertyData property)
        {
            if (_discoveredProperties.ContainsKey(property))
                _discoveredProperties[property]++;
            else
                _discoveredProperties[property] = 1;
        }

        public bool IsPropertyDiscovered(PropertyData property) => _discoveredProperties.ContainsKey(property);

        public IReadOnlyDictionary<PropertyData, int> GetAllEntries() => _discoveredProperties;

        public void Clear() => _discoveredProperties.Clear();
    }
}