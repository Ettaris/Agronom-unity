using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using System.Collections.Generic;
using Systems;
using Managers;
using Data;
using Infrastructure.Events;
using UnityEngine;

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
                _daysLeft = 30; 
                var resolver = ServiceLocator.Get<PropertyResolverSystem>();
                var owner = resolver.GetOwner(this);

                var config = ServiceLocator.Get<GameConfig>();
                var runData = ServiceLocator.Get<RunManager>().CurrentRunData;

                var board = ServiceLocator.Get<RunManager>().CurrentRunData.Board;
                var freeCells = new List<Cell>();
                for (int i = 0; i < board.Width; i++)
                    for (int j = 0; j < board.Height; j++)
                        if (board.IsFree(i, j)) freeCells.Add(board.GetCell(i, j));

                if (freeCells.Count > 0)
                {
                    var cell = freeCells[Random.Range(0, freeCells.Count)];
                    var clone = PlantFactory.CreatePlantWithProperties(owner.PlantData, runData.Random, config, runData);
                    if (board.PlacePlant(clone, cell.X, cell.Y))
                    {
                        clone.CurrentCell = cell;
                        EventBus.Publish(new PlantPlacedEvent { Plant = clone, X = cell.X, Y = cell.Y });
                        Debug.Log($"Gemmation effect done for {clone}");
                    }
                }
            }
        }
    }
}