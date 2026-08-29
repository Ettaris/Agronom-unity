using Infrastructure;
using UnityEngine;

namespace Commands
{
    public struct HarvestCommand : ICommand
    {
        public int X;
        public int Y;
        public Vector2 ScreenPos;

        public void Execute()
        {
            var harvestSystem = ServiceLocator.Get<Systems.HarvestSystem>();
            harvestSystem.HarvestPlantAt(X, Y, ScreenPos);
        }
    }
}