using System.Collections.Generic;
using UnityEngine;
using Data;

[CreateAssetMenu(fileName = "GenomeRarityPool", menuName = "Game/Genome Rarity Pool")]
public class GenomeRarityPool : ScriptableObject
{
    public List<GenomePropertyData> common = new List<GenomePropertyData>();
    public List<GenomePropertyData> uncommon = new List<GenomePropertyData>();
    public List<GenomePropertyData> rare = new List<GenomePropertyData>();
    public List<GenomePropertyData> epic = new List<GenomePropertyData>();
    public List<GenomePropertyData> legendary = new List<GenomePropertyData>();
}