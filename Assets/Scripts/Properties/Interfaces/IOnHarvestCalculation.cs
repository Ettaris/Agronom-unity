using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnHarvestCalculation
    {
        int CalculateHarvest(PlantInstance plant, int baseCalories, IGridBoard board);
    }
}