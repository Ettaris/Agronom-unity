using System;

namespace Infrastructure
{
    public class SeedGenerator
    {
        private readonly int _seed;
        private readonly Random _random;

        public SeedGenerator(int seed)
        {
            _seed = seed;
            _random = new Random(seed);
        }

        public int Seed => _seed;

        public int NextInt() => _random.Next();
        public int NextInt(int maxValue) => _random.Next(maxValue);
        public int NextInt(int minValue, int maxValue) => _random.Next(minValue, maxValue);
        public float NextFloat() => (float)_random.NextDouble();
        public double NextDouble() => _random.NextDouble();

        public SeedGenerator Clone() => new SeedGenerator(_seed);
    }
}