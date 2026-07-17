using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PropertyPool", menuName = "Game/Property Pool")]
    public class GenomePool : ScriptableObject
    {
        public List<GenomePropertyData> genomeProperties = new List<GenomePropertyData>();

        public GenomePropertyData GetRandomProperty()
        {
            if (genomeProperties.Count == 0) return null;
            return genomeProperties[Random.Range(0, genomeProperties.Count)];
        }
    }
}