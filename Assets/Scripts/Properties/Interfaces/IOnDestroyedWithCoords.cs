using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnDestroyedWithCoords
    {
        void OnDestroyed(PlantInstance plant, int x, int y, GridBoard board);
    }
}