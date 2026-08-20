using System;

[Serializable]
public abstract class NarrativeStep
{
    public abstract void Execute(Action onComplete);
    public virtual void Cancel() { }
}
