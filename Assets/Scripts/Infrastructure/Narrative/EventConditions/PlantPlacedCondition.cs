using System;
using Infrastructure;
using Infrastructure.Events;

[Serializable]
public class PlantPlacedCondition : EventCondition
{
    public override void Subscribe(Action onComplete)
    {
        _onComplete = onComplete;
        _isSubscribed = true;
        EventBus.Subscribe<PlantPlacedEvent>(OnEvent);
    }

    private void OnEvent(PlantPlacedEvent evt)
    {
        if (_isSubscribed)
        {
            _isSubscribed = false;
            EventBus.Unsubscribe<PlantPlacedEvent>(OnEvent);
            _onComplete?.Invoke();
        }
    }

    public override void Unsubscribe()
    {
        if (_isSubscribed)
        {
            _isSubscribed = false;
            EventBus.Unsubscribe<PlantPlacedEvent>(OnEvent);
        }
    }
}