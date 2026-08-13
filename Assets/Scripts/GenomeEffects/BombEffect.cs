// GenomeEffects/BombEffect.cs
using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Infrastructure.Events;
using UnityEngine;
using System.Collections.Generic;
using Data;
using Managers;
using Systems;

namespace GenomeEffects
{
    /// <summary>
    /// При посадке: взрывается, уничтожает себя и растения по кресту (слева, справа, сверху, снизу),
    /// собирая 50% их базовых калорий и своих.
    /// </summary>
    public class BombEffect : GenomeEffectBase, IOnPlantPlaced
    {
        public BombEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnPlantPlaced(PlantInstance plant, int x, int y, GridBoard board)
        {
            var runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (runData == null) return;

            var resolver = ServiceLocator.Get<PropertyResolverSystem>();
            var scoreSystem = ServiceLocator.Get<ScoreSystem>();

            // 1. Находим соседей по кресту
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(-1, 0), // слева
                new Vector2Int(1, 0),  // справа
                new Vector2Int(0, -1), // снизу
                new Vector2Int(0, 1)   // сверху
            };

            List<PlantInstance> neighbors = new List<PlantInstance>();
            foreach (var dir in directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                var cell = board.GetCell(nx, ny);
                if (cell != null && cell.Plant != null)
                {
                    neighbors.Add(cell.Plant);
                }
            }

            // 2. Уничтожаем владельца
            scoreSystem.AddCalories(Mathf.RoundToInt(plant.PlantData.baseCalories * 0.5f));
            board.RemovePlant(x, y);
            plant.CurrentCell = null;
            resolver.UnregisterPlant(plant);
            EventBus.Publish(new PlantKilledEvent { Plant = plant, X = x, Y = y, Reason = "Bomb" });

            // 3. Обрабатываем соседей
            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor.CurrentCell == null) continue;

                int nx = neighbor.CurrentCell.X;
                int ny = neighbor.CurrentCell.Y;

                // 50% базовых калорий
                int halfCalories = Mathf.RoundToInt(neighbor.PlantData.baseCalories * 0.5f);

                // Добавляем калории
                scoreSystem.AddCalories(halfCalories);

                // Удаляем соседа с поля
                board.RemovePlant(nx, ny);
                neighbor.CurrentCell = null;
                resolver.UnregisterPlant(neighbor);

                // Публикуем события для обновления UI
                EventBus.Publish(new PlantHarvestedEvent
                {
                    Plant = neighbor,
                    X = nx,
                    Y = ny,
                    CaloriesGained = halfCalories
                });
                EventBus.Publish(new PlantKilledEvent
                {
                    Plant = neighbor,
                    X = nx,
                    Y = ny,
                    Reason = "Bombed"
                });
            }

            // Обновляем руку (на случай, если что-то изменилось)
            EventBus.Publish(new HandUpdatedEvent());
        }
    }
}