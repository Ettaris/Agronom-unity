using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnNeighborHarvestApplication
    {
        void ApplyNeighborHarvest(PlantInstance neighbor, int baseCalories, IGridBoard board);
    }
}