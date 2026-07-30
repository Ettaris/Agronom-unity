using System;
using System.Collections.Generic;
using Data;
using Gameplay;
using Infrastructure;
using UnityEngine;

public static class PlantGeneratorHelper
{
    public static void AssignRandomProperties(PlantInstance plant, SeedGenerator baseRandom, GenomePool genomePool, int maxPropertiesPerPlant)
    {
        if (plant == null || genomePool == null || genomePool.genomeProperties.Count == 0)
            return;

        // Уникальный seed для каждого растения
        var uniqueRandom = new SeedGenerator(baseRandom.Seed + plant.GetHashCode());

        int propertyCount = uniqueRandom.NextInt(1, Math.Min(maxPropertiesPerPlant, genomePool.genomeProperties.Count) + 1);
        var shuffledGenomes = new List<GenomePropertyData>(genomePool.genomeProperties);

        // Перемешиваем с уникальным генератором
        for (int i = shuffledGenomes.Count - 1; i > 0; i--)
        {
            int j = uniqueRandom.NextInt(0, i + 1);
            var temp = shuffledGenomes[i];
            shuffledGenomes[i] = shuffledGenomes[j];
            shuffledGenomes[j] = temp;
        }

        for (int i = 0; i < propertyCount && i < shuffledGenomes.Count; i++)
        {
            var propData = shuffledGenomes[i];
            var prop = propData.CreateEffect(1);
            plant.AddGenomeProperty(prop);
            // Опционально логи
        }
    }
}