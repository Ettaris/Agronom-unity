using System.Collections.Generic;
using Infrastructure;

namespace Gameplay
{
    public class Deck
    {
        private readonly List<PlantInstance> _cards = new List<PlantInstance>();
        private int _currentIndex = 0;

        public int Count => _cards.Count - _currentIndex;

        public void Add(PlantInstance card)
        {
            _cards.Add(card);
        }

        public void AddRange(IEnumerable<PlantInstance> cards)
        {
            _cards.AddRange(cards);
        }

        public void Shuffle(SeedGenerator random)
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = random.NextInt(0, i + 1);
                var temp = _cards[i];
                _cards[i] = _cards[j];
                _cards[j] = temp;
            }
            _currentIndex = 0;
        }

        public PlantInstance Draw()
        {
            if (_currentIndex >= _cards.Count) return null;
            var card = _cards[_currentIndex];
            _currentIndex++;
            return card;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public bool IsEmpty => _currentIndex >= _cards.Count;
    }
}