using Gameplay;
using System;
using UnityEngine;

namespace Properties.Interfaces
{
    [Obsolete]
    public interface IOnHarvest
    {
        int ModifyHarvest(PlantInstance plant, int baseCalories, IGridBoard board);
    }
}
