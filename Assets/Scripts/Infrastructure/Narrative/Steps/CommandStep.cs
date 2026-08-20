using System;
using Infrastructure;
using UnityEngine;

[Serializable]
public class CommandStep : NarrativeStep
{
    [SerializeReference] public ICommand command; // Нужно будет сериализовать через [SerializeReference] TODO:

    public override void Execute(Action onComplete)
    {
        if (command != null)
            CommandProcessor.Execute(command);
        onComplete?.Invoke();
    }
}