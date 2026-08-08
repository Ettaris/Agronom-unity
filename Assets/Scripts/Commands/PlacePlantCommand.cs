using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using UnityEngine;

namespace Commands
{
    public struct PlacePlantCommand : ICommand
    {
        public PlantInstance Plant;
        public int X;
        public int Y;

        public void Execute()
        {
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (runData == null) return;

            Vector2Int pos = new Vector2Int(X, Y);
            if (runData.Board.CanPlace(pos, Plant.PlantData.size))
            {
                runData.Board.PlacePlant(Plant, pos);
                runData.Hand.Remove(Plant);
                EventBus.Publish(new PlantPlacedEvent { Plant = Plant, X = X, Y = Y });
                EventBus.Publish(new HandUpdatedEvent());
            }
        }
    }
}