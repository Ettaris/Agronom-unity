using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;

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

            if (runData.Board.PlacePlant(Plant, X, Y))
            {
                Plant.CurrentCell = runData.Board.GetCell(X, Y);
                // Удаляем из руки
                runData.Hand.Remove(Plant);
                EventBus.Publish(new PlantPlacedEvent { Plant = Plant, X = X, Y = Y });
                EventBus.Publish(new HandUpdatedEvent());
            }
        }
    }
}