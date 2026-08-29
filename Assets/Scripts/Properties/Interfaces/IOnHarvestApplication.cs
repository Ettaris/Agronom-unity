using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnHarvestApplication
    {
        void ApplyHarvest(PlantInstance plant, int baseCalories, IGridBoard board);
    }
}