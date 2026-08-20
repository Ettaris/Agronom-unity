using System;

[Serializable]
public abstract class EventCondition
{
    public abstract void Subscribe(Action onComplete);
    public abstract void Unsubscribe();
    protected Action _onComplete;
    protected bool _isSubscribed;
}