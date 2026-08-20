using Infrastructure;
using System;
using UnityEngine;

[Serializable]
public class DialogueStep : NarrativeStep
{
    public string speaker = "Guard";
    public string text = "";
    [Tooltip("Скорость печати (секунд на символ). 0 = мгновенно.")]
    public float typewriterSpeed = 0.05f;
    [NonSerialized] private Action _onComplete;

    public override void Execute(Action onComplete)
    {
        _onComplete = onComplete;
        var presenter = ServiceLocator.TryGet<IDialoguePresenter>(out var p) ? p : null;
        if (presenter == null)
        {
            UnityEngine.Debug.LogWarning("DialogueStep: IDialoguePresenter not found. Skipping.");
            onComplete?.Invoke();
            return;
        }
        presenter.ShowDialogue(speaker, text, () =>
        {
            presenter.HideDialogue();
            _onComplete?.Invoke();
        }, typewriterSpeed);
    }

    public override void Cancel()
    {
        var presenter = ServiceLocator.TryGet<IDialoguePresenter>(out var p) ? p : null;
        presenter?.HideDialogue();
    }
}