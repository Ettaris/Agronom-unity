using Gameplay;

namespace Properties.Interfaces
{
    public interface IOnBatteryUsed
    {
        void OnBatteryUsed(PlantInstance donor, PlantInstance target, GenomePropertyInstance property);
    }
}