using System.Collections.Generic;

namespace Gameplay
{
    public class Hand
    {
        private readonly List<PlantInstance> _plants = new List<PlantInstance>();
        public int MaxSize { get; set; }
        public int Count => _plants.Count;
        public bool IsFull => _plants.Count >= MaxSize;

        public Hand(int maxSize)
        {
            MaxSize = maxSize;
        }

        public bool Add(PlantInstance plant)
        {
            if (IsFull) return false;
            _plants.Add(plant);
            return true;
        }

        public bool Remove(PlantInstance plant)
        {
            return _plants.Remove(plant);
        }

        public PlantInstance RemoveAt(int index)
        {
            if (index < 0 || index >= _plants.Count) return null;
            var plant = _plants[index];
            _plants.RemoveAt(index);
            return plant;
        }

        public PlantInstance GetAt(int index)
        {
            if (index < 0 || index >= _plants.Count) return null;
            return _plants[index];
        }

        public IReadOnlyList<PlantInstance> GetAll() => _plants.AsReadOnly();

        public void Clear() => _plants.Clear();
    }
}