using Infrastructure;

namespace Commands
{
    public struct HarvestCommand : ICommand
    {
        public int X;
        public int Y;

        public void Execute()
        {
            var harvestSystem = ServiceLocator.Get<Systems.HarvestSystem>();
            harvestSystem.HarvestPlantAt(X, Y);
        }
    }
}