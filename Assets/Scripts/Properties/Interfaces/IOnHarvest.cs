using Gameplay;
using UnityEngine;

namespace Properties.Interfaces
{
    public interface IOnHarvest
    {
        int ModifyHarvest(PlantInstance plant, int baseCalories, GridBoard board);
    }
}
