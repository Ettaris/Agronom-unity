using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnGenomeChanged
    {
        void OnGenomeChanged(PlantInstance plant, GenomePropertyInstance property, bool isAdded);
    }
}