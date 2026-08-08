using System;
using System.Collections.Generic;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using Properties.Interfaces;
using UnityEngine;

namespace Systems
{
    public class PropertyResolverSystem : IGameSystem, IRunAware
    {
        private readonly Dictionary<Type, List<GenomePropertyInstance>> _propertyCacheByInterface = new Dictionary<Type, List<GenomePropertyInstance>>();
        private readonly Dictionary<PlantInstance, List<GenomePropertyInstance>> _plantProperties = new Dictionary<PlantInstance, List<GenomePropertyInstance>>();
        private RunData _runData;
        private bool _isProcessingPlantPlaced = false;

        public void Initialize()
        {
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
            EventBus.Subscribe<PlantKilledEvent>(OnPlantKilled);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Subscribe<HarvestEvent>(OnHarvest);
            EventBus.Subscribe<PlantGrownEvent>(OnPlantGrown);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
            EventBus.Unsubscribe<PlantKilledEvent>(OnPlantKilled);
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Unsubscribe<HarvestEvent>(OnHarvest);
            EventBus.Unsubscribe<PlantGrownEvent>(OnPlantGrown);
            ClearCache();
        }

        public void OnRunDataSetup(RunData runData)
        {
            _runData = runData;
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in PropertyResolverSystem!");
                return;
            }
        }

        // ===== Регистрация =====

        public void RegisterPlant(PlantInstance plant)
        {
            if (plant == null) return;
            if (_plantProperties.ContainsKey(plant)) return;

            var list = new List<GenomePropertyInstance>();
            foreach (var prop in plant.Genome.Properties)
            {
                list.Add(prop);
                RegisterPropertyInCache(prop);
            }
            _plantProperties[plant] = list;
            UnityEngine.Debug.Log($"RegisterPlant: {plant.PlantData.itemName} with {list.Count} properties");
        }

        private void RegisterPropertyInCache(GenomePropertyInstance property)
        {
            if (property == null) return;
            foreach (var interfaceType in property.GetType().GetInterfaces())
            {
                if (interfaceType.Namespace == "Properties.Interfaces")
                {
                    if (!_propertyCacheByInterface.ContainsKey(interfaceType))
                        _propertyCacheByInterface[interfaceType] = new List<GenomePropertyInstance>();
                    _propertyCacheByInterface[interfaceType].Add(property);
                }
            }
        }

        public void UnregisterPlant(PlantInstance plant)
        {
            if (plant == null) return;
            if (_plantProperties.TryGetValue(plant, out var props))
            {
                foreach (var prop in props)
                {
                    foreach (var kvp in _propertyCacheByInterface)
                        kvp.Value.Remove(prop);
                }
                _plantProperties.Remove(plant);
                UnityEngine.Debug.Log($"UnregisterPlant: {plant.PlantData.itemName}");
            }
        }

        public void ClearCache()
        {
            _propertyCacheByInterface.Clear();
            _plantProperties.Clear();
        }

        // ===== Обработка событий =====

        private void OnPlantPlaced(PlantPlacedEvent evt)
        {
            RegisterPlant(evt.Plant);
            if (_isProcessingPlantPlaced) return;
            _isProcessingPlantPlaced = true;
            try
            {
                if (_propertyCacheByInterface.TryGetValue(typeof(IOnPlantPlaced), out var list))
                {
                    var copy = new List<GenomePropertyInstance>(list);
                    foreach (var prop in copy)
                    {
                        // Проверяем, принадлежит ли это свойство текущему растению
                        if (prop is IOnPlantPlaced handler &&
                            _plantProperties.TryGetValue(evt.Plant, out var props) &&
                            props.Contains(prop))
                        {
                            handler.OnPlantPlaced(evt.Plant, evt.X, evt.Y, _runData.Board);
                        }
                    }
                }
            }
            finally
            {
                _isProcessingPlantPlaced = false;
            }
        }

        private void OnPlantKilled(PlantKilledEvent evt)
        {
            OnPlantDestroyed(evt.Plant, evt.X, evt.Y);
            UnregisterPlant(evt.Plant);
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

        private void OnHarvest(HarvestEvent evt) { } // не используется

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

                // Получаем свойства соседа
                if (_plantProperties.TryGetValue(neighborPlant, out var props))
                {
                    foreach (var prop in props)
                    {
                        if (prop is IOnNeighborHarvest handler)
                        {
                            modified = handler.ModifyNeighborHarvest(plant, modified);
                        }
                    }
                }
            }
            return modified;
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

        // ===== Прямые вызовы =====

        public void OnPlantDestroyed(PlantInstance plant, int x, int y)
        {
            if (plant == null) return;
            UnityEngine.Debug.Log($"OnPlantDestroyed: {plant.PlantData.itemName} at ({x},{y})");

            if (_propertyCacheByInterface.TryGetValue(typeof(IOnDestroyedWithCoords), out var list))
            {
                var copy = new List<GenomePropertyInstance>(list);
                foreach (var prop in copy)
                {
                    if (prop is IOnDestroyedWithCoords handler)
                    {
                        // Проверяем, принадлежит ли свойство этому растению
                        if (_plantProperties.TryGetValue(plant, out var props) && props.Contains(prop))
                        {
                            handler.OnDestroyed(plant, x, y, _runData.Board);
                            UnityEngine.Debug.Log($"  Called {prop.GetType().Name} for {plant.PlantData.itemName}");
                        }
                    }
                }
            }
        }

        public int ModifyHarvest(PlantInstance plant, int baseCalories)
        {
            int modified = baseCalories;
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnHarvest), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnHarvest handler)
                    {
                        if (_plantProperties.TryGetValue(plant, out var props) && props.Contains(prop))
                            modified = handler.ModifyHarvest(plant, modified, _runData.Board);
                    }
                }
            }
            return modified;
        }

        public float ModifyGrowth(PlantInstance plant, float currentGrowth)
        {
            float result = currentGrowth;
            if (_propertyCacheByInterface.TryGetValue(typeof(IModifyGrowth), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IModifyGrowth handler)
                    {
                        if (_plantProperties.TryGetValue(plant, out var props) && props.Contains(prop))
                            result = handler.ModifyGrowth(plant, result);
                    }
                }
            }
            return result;
        }

        // ===== Вспомогательные =====

        public PlantInstance GetOwner(GenomePropertyInstance property)
        {
            foreach (var kvp in _plantProperties)
            {
                if (kvp.Value.Contains(property))
                    return kvp.Key;
            }
            return null;
        }
    }
}