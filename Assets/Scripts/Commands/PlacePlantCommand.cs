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
            Debug.Log($"PlacePlantCommand: planting {Plant.PlantData.itemName} at ({X},{Y})");
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (runData == null) { Debug.LogError("RunData is null!"); return; }

            if (runData.Board.PlacePlant(Plant, X, Y))
            {
                Plant.CurrentCell = runData.Board.GetCell(X, Y);
                bool removed = runData.Hand.Remove(Plant);
                Debug.Log($"PlacePlantCommand: Plant removed from hand: {removed}");
                EventBus.Publish(new PlantPlacedEvent { Plant = Plant, X = X, Y = Y });
                EventBus.Publish(new HandUpdatedEvent());
                Debug.Log("PlacePlantCommand: Success");
            }
            else
            {
                Debug.LogWarning($"PlacePlantCommand: Failed to place plant at ({X},{Y})");
            }
        }
    }
}