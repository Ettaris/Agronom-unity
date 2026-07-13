using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnPlantPlaced
    {
        void OnPlantPlaced(PlantInstance plant, int x, int y, GridBoard board);
    }
}