using System;

public interface IDialoguePresenter
{
    void ShowDialogue(string speaker, string text, Action onContinue, float typewriterSpeed = 0f);
    void HideDialogue();
}