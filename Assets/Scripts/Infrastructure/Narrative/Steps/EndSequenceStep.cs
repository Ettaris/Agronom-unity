using System;

[Serializable]
public class EndSequenceStep : NarrativeStep
{
    public override void Execute(Action onComplete)
    {
        onComplete?.Invoke();
    }
}