using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PropertyPool", menuName = "Game/Property Pool")]
    public class PropertyPool : ScriptableObject
    {
        public List<PropertyData> properties = new List<PropertyData>();

        public PropertyData GetRandomProperty()
        {
            if (properties.Count == 0) return null;
            return properties[Random.Range(0, properties.Count)];
        }
    }
}