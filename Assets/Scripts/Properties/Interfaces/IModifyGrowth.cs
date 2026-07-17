using Gameplay;

namespace Properties.Interfaces
{
    public interface IModifyGrowth
    {
        float ModifyGrowth(PlantInstance plant, float currentGrowth);
    }
}
