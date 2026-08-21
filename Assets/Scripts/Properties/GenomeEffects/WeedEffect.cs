using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using Infrastructure.Events;
using Data;
using UnityEngine;

namespace GenomeEffects
{
    /// <summary>
    /// При сборе растения оставляет на его месте сорняк (weedPlantData).
    /// </summary>
    public class WeedEffect : GenomeEffectBase, IOnDestroyedWithCoords
    {
        public WeedEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public void OnDestroyed(PlantInstance plant, int x, int y, GridBoard board)
        {
            var config = ServiceLocator.Get<GameConfig>();
            if (config == null || config.weedPlantData == null)
            {
                Debug.LogWarning("WeedEffect: weedPlantData is not set in GameConfig!");
                return;
            }

            // Создаём сорняк через фабрику (без свойств)
            var weed = PlantFactory.CreateWeed(config.weedPlantData);

            // Размещаем на том же месте
            if (board.PlacePlant(weed, new Vector2Int(x, y)))
            {
                weed.CurrentCell = board.GetCell(x, y);
                EventBus.Publish(new PlantPlacedEvent { Plant = weed, X = x, Y = y });
                Debug.Log($"WeedEffect: Weed placed at ({x},{y})");
            }
            else
            {
                Debug.LogWarning($"WeedEffect: Cannot place weed at ({x},{y}) – cell may be occupied or invalid.");
            }
        }
    }
}