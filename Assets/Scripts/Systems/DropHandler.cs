using UnityEngine;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;

namespace Systems
{
    public class DropHandler : IGameSystem, IRunAware
    {
        private HandView _handView;
        private RunData _runData;

        public void Initialize()
        {
            _handView = ServiceLocator.TryGet<HandView>(out var hv) ? hv : null;
            if (_handView == null)
                Debug.LogError("DropHandler: HandView not found in ServiceLocator!");

            EventBus.Subscribe<CardDropEvent>(OnCardDrop);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<CardDropEvent>(OnCardDrop);
        }

        public void OnRunDataSetup(RunData runData)
        {
            _runData = runData;
        }

        private void OnCardDrop(CardDropEvent evt)
        {
            var card = evt.Card;
            var target = evt.Target;
            var item = card.Item;

            if (target == null) { card.CancelDrop(); return; }

            if (item == null) return;

            var cellView = target.GetComponent<CellView>();
            if (cellView == null) cellView = target.GetComponentInParent<CellView>();
            if (cellView != null)
            {
                if (item is PlantInstance plant)
                {
                    Vector2Int pos = new Vector2Int(cellView.X, cellView.Y);
                    if (_runData.Board.CanPlace(pos, plant.PlantData.size))
                    {
                        CommandProcessor.Execute(new PlacePlantCommand
                        {
                            Plant = plant,
                            X = cellView.X,
                            Y = cellView.Y
                        });
                    }
                    else
                    {
                        card.CancelDrop();
                    }
                    return;
                }
            }

            // ---- Попытка использования в лаборатории ----
            var slotView = target.GetComponent<LaboratorySlotView>();
            if (slotView == null) slotView = target.GetComponentInParent<LaboratorySlotView>();
            if (slotView != null)
            {
                var labView = ServiceLocator.TryGet<LaboratoryView>(out var lv) ? lv : null;
                if (labView != null && labView.OnItemDropped(item))
                {
                    // Удаляем карточку через HandView
                    _handView?.RemoveCard(card);
                }
                else
                {
                    card.CancelDrop();
                }
                return;
            }

            card.CancelDrop();
        }


    }
}