using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GenomePropertyData", menuName = "Game/Genome Property Data")]
    public class GenomePropertyData : UniqueScriptableObject
    {
        public string propertyName;
        public string description;
        public Sprite icon;
        public Rarity rarity;
        public int genomeCost; // стоимость в очках генома
    }
}