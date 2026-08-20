using System;
using DG.Tweening;

[Serializable]
public class DelayStep : NarrativeStep
{
    public float seconds = 0.5f;

    public override void Execute(Action onComplete)
    {
        DOVirtual.DelayedCall(seconds, () =>
        {
            onComplete?.Invoke();
        });
    }
}