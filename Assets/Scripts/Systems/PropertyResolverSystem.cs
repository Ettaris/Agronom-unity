using System;
using System.Collections.Generic;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using Properties.Interfaces;

namespace Systems
{
    public class PropertyResolverSystem : IGameSystem
    {
        private readonly Dictionary<Type, List<GenomePropertyInstance>> _propertyCacheByInterface = new Dictionary<Type, List<GenomePropertyInstance>>();
        private readonly Dictionary<GenomePropertyInstance, PlantInstance> _propertyOwner = new Dictionary<GenomePropertyInstance, PlantInstance>();
        private RunData _runData;

        public void Initialize()
        {

            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Subscribe<HarvestEvent>(OnHarvest);
            EventBus.Subscribe<PlantGrownEvent>(OnPlantGrown);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Unsubscribe<HarvestEvent>(OnHarvest);
            EventBus.Unsubscribe<PlantGrownEvent>(OnPlantGrown);
            ClearCache();
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _runData = evt.RunData;
        }

        public void RegisterProperty(GenomePropertyInstance property, PlantInstance owner)
        {
            if (property == null || owner == null) return;
            _propertyOwner[property] = owner;

            var type = property.GetType();
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.Namespace == "Properties.Interfaces")
                {
                    if (!_propertyCacheByInterface.ContainsKey(interfaceType))
                        _propertyCacheByInterface[interfaceType] = new List<GenomePropertyInstance>();
                    _propertyCacheByInterface[interfaceType].Add(property);
                }
            }
        }

        public void UnregisterProperty(GenomePropertyInstance property)
        {
            if (property == null) return;
            foreach (var kvp in _propertyCacheByInterface)
                kvp.Value.Remove(property);
            _propertyOwner.Remove(property);
        }

        public void RegisterPlant(PlantInstance plant)
        {
            if (plant == null) return;
            foreach (var prop in plant.Genome.Properties)
                RegisterProperty(prop, plant);
        }

        public void UnregisterPlant(PlantInstance plant)
        {
            if (plant == null) return;
            foreach (var prop in plant.Genome.Properties)
                UnregisterProperty(prop);
        }

        public void ClearCache()
        {
            _propertyCacheByInterface.Clear();
            _propertyOwner.Clear();
        }

        private void OnPlantPlaced(PlantPlacedEvent evt)
        {
            RegisterPlant(evt.Plant);
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnPlantPlaced), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnPlantPlaced handler)
                        handler.OnPlantPlaced(evt.Plant, evt.X, evt.Y, _runData.Board);
                }
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnDayStart), out var list))
            {
                foreach (var prop in list)
                    if (prop is IOnDayStart handler)
                        handler.OnDayStart(evt.DayNumber);
            }
        }

        private void OnDayEnded(DayEndedEvent evt)
        {
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnDayEnd), out var list))
            {
                foreach (var prop in list)
                    if (prop is IOnDayEnd handler)
                        handler.OnDayEnd(evt.DayNumber);
            }
        }

        private void OnHarvest(HarvestEvent evt)
        {
            int modified = evt.BaseCalories;
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnHarvest), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnHarvest handler)
                        modified = handler.ModifyHarvest(evt.Plant, modified, _runData.Board);
                }
            }
            EventBus.Publish(new HarvestModifiedEvent { Plant = evt.Plant, ModifiedCalories = modified });
        }

        private void OnPlantGrown(PlantGrownEvent evt)
        {
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnPlantGrown), out var list))
            {
                foreach (var prop in list)
                    if (prop is IOnPlantGrown handler)
                        handler.OnPlantGrown(evt.Plant);
            }
        }

        public float ModifyGrowth(PlantInstance plant, float currentGrowth)
        {
            float result = currentGrowth;
            if (_propertyCacheByInterface.TryGetValue(typeof(IModifyGrowth), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IModifyGrowth handler && _propertyOwner.TryGetValue(prop, out var owner) && owner == plant)
                        result = handler.ModifyGrowth(plant, result);
                }
            }
            return result;
        }

        /// <summary>
        /// Прямой метод для модификации калорий при сборе, без публикации события HarvestEvent.
        /// Используется HarvestSystem для мгновенного сбора.
        /// </summary>
        public int ModifyHarvest(PlantInstance plant, int baseCalories)
        {
            int modified = baseCalories;
            if (_propertyCacheByInterface.TryGetValue(typeof(Properties.Interfaces.IOnHarvest), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is Properties.Interfaces.IOnHarvest handler && _propertyOwner.TryGetValue(prop, out var owner) && owner == plant)
                    {
                        modified = handler.ModifyHarvest(plant, modified, _runData.Board);
                    }
                }
            }
            return modified;
        }

        /// <summary>
        /// Вызывается при сборе растения. Проверяет всех соседей и применяет их эффекты.
        /// </summary>
        public int ModifyHarvestByNeighbors(PlantInstance plant, int baseCalories)
        {
            if (plant == null || plant.CurrentCell == null) return baseCalories;

            int modified = baseCalories;
            var board = _runData.Board;
            var neighbors = board.GetNeighbors(plant.CurrentCell.X, plant.CurrentCell.Y, false);

            foreach (var cell in neighbors)
            {
                if (cell.Plant == null) continue;
                var neighborPlant = cell.Plant;
                // Ищем свойства у соседа, реализующие IOnNeighborHarvest
                foreach (var prop in neighborPlant.Genome.Properties)
                {
                    if (prop is IOnNeighborHarvest handler)
                    {
                        modified = handler.ModifyNeighborHarvest(plant, modified);
                    }
                }
            }
            return modified;
        }

        public void OnPlantDestroyed(PlantInstance plant, int x, int y)
        {
            // Вызов IOnDestroyedWithCoords
            if (_propertyCacheByInterface.TryGetValue(typeof(Properties.Interfaces.IOnDestroyedWithCoords), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnDestroyedWithCoords handler && _propertyOwner.TryGetValue(prop, out var owner) && owner == plant)
                    {
                        handler.OnDestroyed(plant, x, y, _runData.Board);
                    }
                }
            }
            // Вызов старого IOnDestroyed (если есть)
            if (_propertyCacheByInterface.TryGetValue(typeof(Properties.Interfaces.IOnDestroyed), out var oldList))
            {
                foreach (var prop in oldList)
                {
                    if (prop is IOnDestroyed handler && _propertyOwner.TryGetValue(prop, out var owner) && owner == plant)
                    {
                        handler.OnDestroyed(plant);
                    }
                }
            }
        }

        public PlantInstance GetOwner(GenomePropertyInstance property)
        {
            if (property == null) return null;
            _propertyOwner.TryGetValue(property, out var owner);
            return owner;
        }
    }
}