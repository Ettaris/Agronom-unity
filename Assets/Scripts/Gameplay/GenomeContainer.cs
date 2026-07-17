using System.Collections.Generic;
using Data;
using Infrastructure;
using Infrastructure.Events;

namespace Gameplay
{
    public class GenomeContainer
    {
        public int MaxCapacity { get; set; }
        public int CurrentWeight { get; private set; }
        public List<GenomePropertyInstance> Properties { get; private set; }

        public GenomeContainer(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            CurrentWeight = 0;
            Properties = new List<GenomePropertyInstance>();
        }

        public bool CanAddProperty(GenomePropertyInstance property)
        {
            if (property == null) return false;
            return CurrentWeight + property.GetGenomeCost() <= MaxCapacity;
        }

        /// <summary>
        /// ƒобавл€ет свойство и публикует событие.
        /// </summary>
        public bool AddProperty(GenomePropertyInstance property, PlantInstance owner)
        {
            if (property == null || owner == null) return false;
            if (!CanAddProperty(property)) return false;

            Properties.Add(property);
            CurrentWeight += property.GetGenomeCost();

            // ѕубликуем событие об изменении генома
            EventBus.Publish(new GenomeChangedEvent
            {
                Plant = owner,
                Property = property,
                IsAdded = true
            });

            return true;
        }

        /// <summary>
        /// ”дал€ет свойство и публикует событие.
        /// </summary>
        public GenomePropertyInstance RemoveProperty(GenomePropertyData propertyData, PlantInstance owner)
        {
            if (propertyData == null || owner == null) return null;

            var prop = Properties.Find(p => p.Data == propertyData);
            if (prop != null)
            {
                Properties.Remove(prop);
                CurrentWeight -= prop.GetGenomeCost();

                // ѕубликуем событие об изменении генома
                EventBus.Publish(new GenomeChangedEvent
                {
                    Plant = owner,
                    Property = prop,
                    IsAdded = false
                });

                return prop;
            }
            return null;
        }

        public void Clear()
        {
            Properties.Clear();
            CurrentWeight = 0;
        }

        public int GetFillPercent()
        {
            if (MaxCapacity == 0) return 0;
            return (int)((float)CurrentWeight / MaxCapacity * 100f);
        }
    }
}