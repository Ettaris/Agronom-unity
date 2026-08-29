using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnNeighborHarvestCalculation
    {
        int CalculateNeighborHarvest(PlantInstance neighbor, int baseCalories, IGridBoard board);
    }
}