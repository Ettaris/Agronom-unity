using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using System.Collections.Generic;
using Systems;
using Managers;
using Data;

namespace GenomeEffects
{
    public class GemmationEffect : GenomeEffectBase, IOnDayStart
    {
        private int _daysLeft = 3;

        public GemmationEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnDayStart(int dayNumber)
        {
            _daysLeft--;
            if (_daysLeft <= 0)
            {
                _daysLeft = 3; // сброс для повторения
                // Находим владельца (растение) через PropertyResolverSystem
                var resolver = ServiceLocator.Get<PropertyResolverSystem>();
                var owner = resolver.GetOwner(this);
                if (owner == null || owner.CurrentCell == null) return;

                var board = ServiceLocator.Get<RunManager>().CurrentRunData.Board;
                var neighbors = board.GetNeighbors(owner.CurrentCell.X, owner.CurrentCell.Y, false);
                var freeCells = new List<Cell>();
                foreach (var cell in neighbors)
                {
                    if (cell.Plant == null) freeCells.Add(cell);
                }
                if (freeCells.Count > 0)
                {
                    var cell = freeCells[UnityEngine.Random.Range(0, freeCells.Count)];
                    var clone = new PlantInstance(owner.PlantData, owner.Genome.MaxCapacity);
                    // Копируем свойства (можно без них, для простоты) TODO:
                    if (board.PlacePlant(clone, cell.X, cell.Y))
                    {
                        clone.CurrentCell = cell;
                        resolver.RegisterPlant(clone);
                    }
                }
            }
        }
    }
}