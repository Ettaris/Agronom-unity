using UnityEngine;

[CreateAssetMenu(fileName = "PlantRarityConfig", menuName = "Game/Plant Rarity Config")]
public class PlantRarityConfig : ScriptableObject
{
    public int commonWeight = 50;
    public int uncommonWeight = 30;
    public int rareWeight = 15;
    public int epicWeight = 4;
    public int legendaryWeight = 1;
}