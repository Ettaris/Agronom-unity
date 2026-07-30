using Infrastructure;
using System;

public static class WeightedRandom
{
    public static int ChooseIndex(int[] weights, SeedGenerator random)
    {
        if (weights == null || weights.Length == 0)
            throw new ArgumentException("Weights cannot be null or empty.");

        int total = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i] < 0)
                throw new ArgumentException("Weights must be non-negative.");
            total += weights[i];
        }

        if (total == 0)
            throw new ArgumentException("Total weight must be greater than zero.");

        int r = random.NextInt(total);
        int cumulative = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (r < cumulative)
                return i;
        }
        return weights.Length - 1;
    }
}