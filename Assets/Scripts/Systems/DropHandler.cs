using UnityEngine;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;

namespace Systems
{
    public class DropHandler : IGameSystem
    {
        private HandView _handView;

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

        private void OnCardDrop(CardDropEvent evt)
        {
            var card = evt.Card;
            var target = evt.Target;
            var item = card.Item;

            if (item == null) return;

            var cellView = target.GetComponent<CellView>();
            if (cellView == null) cellView = target.GetComponentInParent<CellView>();
            if (cellView != null)
            {
                if (item is PlantInstance plant)
                {
                    CommandProcessor.Execute(new PlacePlantCommand
                    {
                        Plant = plant,
                        X = cellView.X,
                        Y = cellView.Y
                    });
                }
                return;
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