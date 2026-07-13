using System.Collections.Generic;
using Data;

namespace Gameplay
{
    public class PlantInstance
    {
        public readonly PlantData Data;
        public float GrowthProgress { get; set; } // 0..1
        public bool IsGrown => GrowthProgress >= 1f;
        public List<PropertyInstance> Properties { get; private set; }
        public int MaxProperties { get; set; } // из конфига

        public PlantInstance(PlantData data, int maxProperties)
        {
            Data = data;
            GrowthProgress = 0f;
            Properties = new List<PropertyInstance>(maxProperties);
            MaxProperties = maxProperties;
        }

        public bool CanAddProperty() => Properties.Count < MaxProperties;

        public bool AddProperty(PropertyInstance property)
        {
            if (!CanAddProperty()) return false;
            Properties.Add(property);
            return true;
        }

        public bool RemoveProperty(PropertyInstance property)
        {
            return Properties.Remove(property);
        }

        public void ClearProperties()
        {
            Properties.Clear();
        }
    }
}