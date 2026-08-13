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
    public class CentrifugeSystem : IGameSystem, IRunAware
    {
        private Hand _hand;

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void OnRunDataSetup(RunData runData)
        {
            if (runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in CentrifugeSystem!");
                return;
            }
            _hand = runData.Hand;
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

            if (donor.CurrentCell != null || target.CurrentCell != null)
            {
                UnityEngine.Debug.LogWarning("TransferGenome: Both plants must be cards (not planted).");
                return false;
            }

            if (donor == target)
            {
                UnityEngine.Debug.LogWarning("TransferGenome: donor and target cannot be the same plant.");
                return false;
            }

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

            if (donor.Genome.Properties.Count == 0)
            {
                UnityEngine.Debug.Log($"Donor plant {donor.PlantData.itemName} has no properties to transfer.");
                return false;
            }

            var propertyToTransfer = donor.Genome.Properties[0];

            bool canAccept = target.CanAddGenomeProperty(propertyToTransfer);

            donor.Genome.RemoveProperty(propertyToTransfer.Data, donor);

            ServiceLocator.Get<PropertyResolverSystem>().UnregisterPlant(donor);
            _hand.Remove(donor);

            EventBus.Publish(new PlantKilledEvent
            {
                Plant = donor,
                Reason = "Donor extracted"
            });

            //TODO: лишние события
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
                ServiceLocator.Get<PropertyResolverSystem>().UnregisterPlant(target);
                _hand.Remove(target);
                EventBus.Publish(new GenomeTransferFailedEvent { Donor = donor, Target = target });
                UnityEngine.Debug.Log($"Target plant {target.PlantData.itemName} died due to genome overload. Both plants are destroyed.");
            }

            _hand.Remove(batteryItem);

            EventBus.Publish(new HandUpdatedEvent());
            return true;
        }
    }
}