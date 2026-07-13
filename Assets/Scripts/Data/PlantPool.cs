using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PlantPool", menuName = "Game/Plant Pool")]
    public class PlantPool : ScriptableObject
    {
        public List<PlantData> plants = new List<PlantData>();

        public PlantData GetRandomPlant() // можно использовать для тестов, но логика генерации будет в системах
        {
            if (plants.Count == 0) return null;
            return plants[Random.Range(0, plants.Count)];
        }
    }
}