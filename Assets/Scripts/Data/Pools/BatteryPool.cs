using Data;
using System.Collections.Generic;
using UnityEngine;


namespace Data
{
    [CreateAssetMenu(fileName = "BatteryPool", menuName = "Game/Battery Pool")]
    public class BatteryPool : ScriptableObject
    {
        public List<BatteryData> batteries = new List<BatteryData>();
    }
}