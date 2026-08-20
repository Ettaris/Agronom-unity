using System;
using UnityEngine;

[Serializable]
public class WaitForEventStep : NarrativeStep
{
    [SerializeReference] public EventCondition condition;
    [NonSerialized] private Action _onComplete;

    public override void Execute(Action onComplete)
    {
        _onComplete = onComplete;
        if (condition == null)
        {
            UnityEngine.Debug.LogWarning("WaitForEventStep: condition is null. Skipping.");
            onComplete?.Invoke();
            return;
        }
        condition.Subscribe(() => _onComplete?.Invoke());
    }

    public override void Cancel()
    {
        condition?.Unsubscribe();
    }
}