using System.Collections.Generic;
using UnityEngine;
using Data;

[CreateAssetMenu(fileName = "PlantRarityPool", menuName = "Game/Plant Rarity Pool")]
public class PlantRarityPool : ScriptableObject
{
    public List<PlantData> commonPlants = new List<PlantData>();
    public List<PlantData> uncommonPlants = new List<PlantData>();
    public List<PlantData> rarePlants = new List<PlantData>();
    public List<PlantData> epicPlants = new List<PlantData>();
    public List<PlantData> legendaryPlants = new List<PlantData>();
}