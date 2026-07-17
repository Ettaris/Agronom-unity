using Infrastructure;
using System.Collections.Generic;
using Systems;

namespace Gameplay
{
    public class Hand
    {
        private readonly List<ItemInstance> _items = new List<ItemInstance>();
        public int MaxSize { get; set; }
        public int Count => _items.Count;
        public bool IsFull => _items.Count >= MaxSize;

        public Hand(int maxSize)
        {
            MaxSize = maxSize;
        }

        public bool Add(ItemInstance item)
        {
            if (IsFull)
            {
                EventBus.Publish(new HandFullEvent());
                return false;
            }
            _items.Add(item);
            return true;
        }

        public bool Remove(ItemInstance item)
        {
            return _items.Remove(item);
        }

        public ItemInstance RemoveAt(int index)
        {
            if (index < 0 || index >= _items.Count) return null;
            var item = _items[index];
            _items.RemoveAt(index);
            return item;
        }

        public ItemInstance GetAt(int index)
        {
            if (index < 0 || index >= _items.Count) return null;
            return _items[index];
        }

        public IReadOnlyList<ItemInstance> GetAll() => _items.AsReadOnly();

        public void Clear() => _items.Clear();

        // Метод для получения всех растений (удобно)
        public IEnumerable<PlantInstance> GetPlants()
        {
            foreach (var item in _items)
                if (item is PlantInstance plant)
                    yield return plant;
        }
    }
}