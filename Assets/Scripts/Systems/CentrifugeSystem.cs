using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Data;
using Managers;

namespace Systems
{
    /// <summary>
    /// Система центрифуги для переноса генома с использованием батарейки.
    /// Работает ТОЛЬКО с карточками растений (непосаженными) в лаборатории.
    /// Если у получателя превышен лимит генома — оба растения уничтожаются, свойство исчезает.
    /// </summary>
    public class CentrifugeSystem : IGameSystem
    {
        private RunData _runData;
        private Hand _hand;

        public void Initialize()
        {
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);

           
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in CentrifugeSystem!");
                return;
            }

            _hand = _runData.Hand;
        }

        /// <summary>
        /// Переносит первое свойство с донора на получателя с использованием батарейки.
        /// Оба растения должны быть карточками (не посажены на поле).
        /// </summary>
        /// <param name="donor">Растение-донор (карточка в руке)</param>
        /// <param name="target">Растение-получатель (карточка в руке)</param>
        /// <param name="battery">Используемая батарейка (в руке)</param>
        /// <returns>true, если операция выполнена (даже если оба погибли)</returns>
        public bool TransferGenome(PlantInstance donor, PlantInstance target, BatteryData battery)
        {
            if (donor == null || target == null || battery == null)
            {
                UnityEngine.Debug.LogWarning("TransferGenome: invalid arguments.");
                return false;
            }

            // Проверяем, что оба растения — карточки (не посажены)
            if (donor.CurrentCell != null || target.CurrentCell != null)
            {
                UnityEngine.Debug.LogWarning("TransferGenome: Both plants must be cards (not planted).");
                return false;
            }

            // Проверяем, что донор и получатель — разные растения
            if (donor == target)
            {
                UnityEngine.Debug.LogWarning("TransferGenome: donor and target cannot be the same plant.");
                return false;
            }

            // Проверяем, есть ли батарейка в руке
            ItemInstance batteryItem = null;
            foreach (var item in _hand.GetAll())
            {
                if (item.Data is BatteryData && item.Data == battery)
                {
                    batteryItem = item;
                    break;
                }
            }

            if (batteryItem == null)
            {
                UnityEngine.Debug.LogWarning("TransferGenome: Battery not found in hand.");
                return false;
            }

            // Проверяем, есть ли у донора свойства
            if (donor.Genome.Properties.Count == 0)
            {
                UnityEngine.Debug.Log($"Donor plant {donor.PlantData.itemName} has no properties to transfer.");
                return false;
            }

            // Берём первое свойство донора
            var propertyToTransfer = donor.Genome.Properties[0];

            // Проверяем, может ли получатель принять это свойство
            bool canAccept = target.CanAddGenomeProperty(propertyToTransfer);

            // Удаляем свойство у донора (даже если не примет получатель, донор теряет свойство)
            donor.Genome.RemoveProperty(propertyToTransfer.Data, donor);

            // Удаляем донора из руки (он уничтожается)
            ServiceLocator.Get<PropertyResolverSystem>().UnregisterPlant(donor);
            _hand.Remove(donor);

            // Публикуем событие об уничтожении донора
            EventBus.Publish(new PlantKilledEvent
            {
                Plant = donor,
                Reason = "Donor extracted"
            });

            // Если получатель может принять свойство — добавляем
            if (canAccept)
            {
                target.AddGenomeProperty(propertyToTransfer);
                EventBus.Publish(new GenomeTransferredEvent
                {
                    Donor = donor,
                    Target = target,
                    Property = propertyToTransfer
                });
                UnityEngine.Debug.Log($"Property {propertyToTransfer.Data.propertyName} transferred to {target.PlantData.itemName}.");
            }
            else
            {
                // Получатель не может принять свойство — он погибает
                // Свойство уже удалено у донора и не добавлено получателю -> исчезает
                ServiceLocator.Get<PropertyResolverSystem>().UnregisterPlant(target);
                _hand.Remove(target);
                EventBus.Publish(new PlantKilledEvent
                {
                    Plant = target,
                    Reason = "Genome overload"
                });
                UnityEngine.Debug.Log($"Target plant {target.PlantData.itemName} died due to genome overload. Both plants are destroyed.");
            }

            // Удаляем батарейку из руки
            _hand.Remove(batteryItem);

            // Публикуем событие об использовании батарейки
            EventBus.Publish(new BatteryUsedEvent
            {
                Donor = donor,
                Target = target,
                Battery = battery
            });

            return true;
        }
    }
}