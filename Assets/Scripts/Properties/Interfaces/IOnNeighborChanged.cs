

using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnNeighborChanged
    {
        void OnNeighborChanged(PlantInstance plant, Cell neighborCell, bool isAdded);
    }
}