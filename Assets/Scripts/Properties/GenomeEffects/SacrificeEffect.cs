using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Data;
using Systems;
using Infrastructure.Events;
using UnityEngine;

namespace GenomeEffects
{
    public class SacrificeEffect : GenomeEffectBase, IOnPlantPlaced
    {
        public SacrificeEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnPlantPlaced(PlantInstance plant, int x, int y, GridBoard board)
        {
            // 1. Проверяем клетку слева (x-1, y)
            var leftCell = board.GetCell(x - 1, y);
            if (leftCell != null && leftCell.Plant != null)
            {
                // Левое растение существует – делаем его зрелым мгновенно
                var leftPlant = leftCell.Plant;
                leftPlant.GrowthProgress = 1f;
                EventBus.Publish(new PlantGrownEvent { Plant = leftPlant });
                EventBus.Publish(new EffectAppliedEvent
                {
                    X = x - 1,
                    Y = y,
                    Type = EffectType.Grow,
                    Duration = 0.5f
                });
            }
            else { return; }

            // 2. Удаляем текущее растение (носитель) без выдачи калорий
            EventBus.Publish(new EffectAppliedEvent
            {
                X = x,
                Y = y,
                Type = EffectType.Sacrifice,
                Duration = 0.5f
            });
            EventBus.Publish(new PlantKilledEvent { Plant = plant, X = x, Y = y, Reason = "sacrifice" });
            board.RemovePlant(x, y);
            plant.CurrentCell = null;

            // Отписываем свойства от резолвера
            var resolver = ServiceLocator.Get<PropertyResolverSystem>();
            resolver.UnregisterPlant(plant);

            // Публикуем событие уничтожения
            EventBus.Publish(new PlantKilledEvent { Plant = plant, X = x, Y = y, Reason = "Sacrifice" });
        }
    }
}