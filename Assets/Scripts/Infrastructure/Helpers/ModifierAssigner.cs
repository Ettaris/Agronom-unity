using System;
using System.Collections.Generic;
using Data;
using Gameplay;
using Infrastructure;
using UnityEngine;

public static class ModifierAssigner
{
    public static void AssignModifiers(PlantInstance plant, SeedGenerator random, ModifierAssignmentConfig config, GenomeRarityPool pool, GenomePropertyData permanentData)
    {
        if (plant == null || config == null) return;

        plant.ClearGenomeProperties();

        if (permanentData != null)
        {
            var prop = permanentData.CreateEffect(1);
            plant.AddGenomeProperty(prop);
            plant.PermanentModifier = prop;
        }

        if (random.NextDouble() < config.secondModifierChance)
        {
            var second = SelectRandomModifier(random, config, pool, exclude: permanentData);
            if (second != null)
            {
                var prop = second.CreateEffect(1);
                plant.AddGenomeProperty(prop);
            }
        }
    }

    private static GenomePropertyData SelectRandomModifier(SeedGenerator random, ModifierAssignmentConfig config, GenomeRarityPool pool, GenomePropertyData exclude = null)
    {
        int[] weights = new int[] {
            config.commonWeight,
            config.uncommonWeight,
            config.rareWeight,
            config.epicWeight,
            config.legendaryWeight
        };

        List<GenomePropertyData>[] lists = new List<GenomePropertyData>[] {
            pool.common, pool.uncommon, pool.rare, pool.epic, pool.legendary
        };

        int attempts = 0;
        const int maxAttempts = 30;

        while (attempts < maxAttempts)
        {
            attempts++;
            int rarityIndex = WeightedRandom.ChooseIndex(weights, random);
            var list = lists[rarityIndex];
            if (list == null || list.Count == 0) continue;

            int idx = random.NextInt(list.Count);
            var candidate = list[idx];
            if (exclude != null && candidate == exclude) continue;
            return candidate;
        }

        // Fallback – если ничего не нашлось, вернуть первый попавшийся из любого пула (кроме exclude)
        foreach (var list in lists)
        {
            if (list == null || list.Count == 0) continue;
            foreach (var item in list)
            {
                if (exclude != null && item == exclude) continue;
                return item;
            }
        }
        return null;
    }

    public static GenomePropertyData SelectPermanentModifier(SeedGenerator random, ModifierAssignmentConfig config, GenomeRarityPool pool)
    {
        if (config == null || pool == null)
        {
            Debug.LogWarning("SelectPermanentModifier: config or pool is null, returning null");
            return null;
        }

        int[] weights = new int[] {
        config.commonWeight,
        config.uncommonWeight,
        config.rareWeight,
        config.epicWeight,
        config.legendaryWeight
    };

        List<GenomePropertyData>[] lists = new List<GenomePropertyData>[] {
        pool.common, pool.uncommon, pool.rare, pool.epic, pool.legendary
    };

        int attempts = 0;
        const int maxAttempts = 30;
        while (attempts < maxAttempts)
        {
            attempts++;
            int rarityIndex = WeightedRandom.ChooseIndex(weights, random);
            var list = lists[rarityIndex];
            if (list == null || list.Count == 0) continue;
            int idx = random.NextInt(list.Count);
            return list[idx];
        }

        // Fallback – первый попавшийся непустой список
        foreach (var list in lists)
        {
            if (list != null && list.Count > 0)
                return list[0];
        }
        return null;
    }
}