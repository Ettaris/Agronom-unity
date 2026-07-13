using System;
using System.Collections.Generic;
using Gameplay;
using Infrastructure;
using Properties.Interfaces;

namespace Systems
{
    /// <summary>
    /// Центральная система, управляющая вызовом эффектов свойств.
    /// </summary>
    public class PropertyResolverSystem : IGameSystem
    {
        // Кэши: все свойства, сгруппированные по интерфейсам для быстрого доступа
        private readonly Dictionary<Type, List<PropertyInstance>> _propertyCacheByInterface = new Dictionary<Type, List<PropertyInstance>>();

        // Словарь для быстрого поиска растения по свойству (если нужно)
        private readonly Dictionary<PropertyInstance, PlantInstance> _propertyOwner = new Dictionary<PropertyInstance, PlantInstance>();

        private RunData _runData;

        public void Initialize()
        {
            _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in PropertyResolverSystem!");
                return;
            }

            // Подписка на события
            EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Subscribe<HarvestEvent>(OnHarvest);
            EventBus.Subscribe<PlantGrownEvent>(OnPlantGrown);
            // Можно добавить другие события по мере необходимости
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Unsubscribe<HarvestEvent>(OnHarvest);
            EventBus.Unsubscribe<PlantGrownEvent>(OnPlantGrown);

            ClearCache();
        }

        /// <summary>
        /// Регистрирует свойство для отслеживания.
        /// Вызывается при создании растения или добавлении свойства.
        /// </summary>
        public void RegisterProperty(PropertyInstance property, PlantInstance owner)
        {
            if (property == null || owner == null) return;

            // Сохраняем владельца
            _propertyOwner[property] = owner;

            // Проверяем все интерфейсы, которые реализует свойство
            var type = property.GetType();
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.Namespace == "Properties.Interfaces") // только наши интерфейсы
                {
                    if (!_propertyCacheByInterface.ContainsKey(interfaceType))
                        _propertyCacheByInterface[interfaceType] = new List<PropertyInstance>();
                    _propertyCacheByInterface[interfaceType].Add(property);
                }
            }
        }

        /// <summary>
        /// Удаляет свойство из кэша (при уничтожении растения или извлечении свойства).
        /// </summary>
        public void UnregisterProperty(PropertyInstance property)
        {
            if (property == null) return;

            // Удаляем из всех кэшей
            foreach (var kvp in _propertyCacheByInterface)
            {
                kvp.Value.Remove(property);
            }
            _propertyOwner.Remove(property);
        }

        /// <summary>
        /// Регистрирует все свойства растения.
        /// </summary>
        public void RegisterPlant(PlantInstance plant)
        {
            if (plant == null) return;
            foreach (var prop in plant.Properties)
            {
                RegisterProperty(prop, plant);
            }
        }

        /// <summary>
        /// Удаляет все свойства растения.
        /// </summary>
        public void UnregisterPlant(PlantInstance plant)
        {
            if (plant == null) return;
            foreach (var prop in plant.Properties)
            {
                UnregisterProperty(prop);
            }
        }

        /// <summary>
        /// Очищает весь кэш (при завершении забега).
        /// </summary>
        public void ClearCache()
        {
            _propertyCacheByInterface.Clear();
            _propertyOwner.Clear();
        }

        // ------------------ Обработчики событий ------------------

        private void OnPlantPlaced(PlantPlacedEvent evt)
        {
            var plant = evt.Plant;
            // Регистрируем все свойства растения (если ещё не зарегистрированы)
            RegisterPlant(plant);

            // Вызываем интерфейс IOnPlantPlaced
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnPlantPlaced), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnPlantPlaced handler)
                    {
                        handler.OnPlantPlaced(plant, evt.X, evt.Y, _runData.Board);
                    }
                }
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnDayStart), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnDayStart handler)
                    {
                        handler.OnDayStart(evt.DayNumber);
                    }
                }
            }
        }

        private void OnDayEnded(DayEndedEvent evt)
        {
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnDayEnd), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnDayEnd handler)
                    {
                        handler.OnDayEnd(evt.DayNumber);
                    }
                }
            }
        }

        private void OnHarvest(HarvestEvent evt)
        {
            var plant = evt.Plant;
            int modifiedCalories = evt.BaseCalories;

            if (_propertyCacheByInterface.TryGetValue(typeof(IOnHarvest), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnHarvest handler)
                    {
                        // Каждое свойство может модифицировать калории, передаём текущее значение
                        modifiedCalories = handler.ModifyHarvest(plant, modifiedCalories, _runData.Board);
                    }
                }
            }

            // Публикуем событие с изменёнными калориями (можно использовать для обновления UI)
            EventBus.Publish(new HarvestModifiedEvent { Plant = plant, ModifiedCalories = modifiedCalories });
        }

        private void OnPlantGrown(PlantGrownEvent evt)
        {
            var plant = evt.Plant;

            if (_propertyCacheByInterface.TryGetValue(typeof(IOnPlantGrown), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnPlantGrown handler)
                    {
                        handler.OnPlantGrown(plant);
                    }
                }
            }
        }

        // ------------------ Дополнительные публичные методы ------------------

        /// <summary>
        /// Вычисляет модификацию роста для конкретного растения.
        /// </summary>
        public float ModifyGrowth(PlantInstance plant, float currentGrowth)
        {
            float result = currentGrowth;
            if (_propertyCacheByInterface.TryGetValue(typeof(IModifyGrowth), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IModifyGrowth handler)
                    {
                        // Проверяем, принадлежит ли свойство этому растению
                        if (_propertyOwner.TryGetValue(prop, out var owner) && owner == plant)
                        {
                            result = handler.ModifyGrowth(plant, result);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Вызывает событие изменения соседа (если нужно).
        /// </summary>
        public void NotifyNeighborChanged(PlantInstance plant, Cell neighborCell, bool isAdded)
        {
            if (_propertyCacheByInterface.TryGetValue(typeof(IOnNeighborChanged), out var list))
            {
                foreach (var prop in list)
                {
                    if (prop is IOnNeighborChanged handler)
                    {
                        if (_propertyOwner.TryGetValue(prop, out var owner) && owner == plant)
                        {
                            handler.OnNeighborChanged(plant, neighborCell, isAdded);
                        }
                    }
                }
            }
        }
    }

    // Дополнительное событие для уведомления об изменённых калориях
    public struct HarvestModifiedEvent
    {
        public PlantInstance Plant;
        public int ModifiedCalories;
    }
}