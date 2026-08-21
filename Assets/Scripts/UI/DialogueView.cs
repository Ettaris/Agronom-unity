using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Infrastructure;
using System;

public class DialogueView : MonoBehaviour, IDialoguePresenter
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _speakerText;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private Button _continueButton;

    private Action _onContinue;
    private Tween _typewriterTween;
    private bool _isShowing;

    private void Awake()
    {
        ServiceLocator.Register(this as IDialoguePresenter);
        _panel.SetActive(false);
        _continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDestroy()
    {
        _continueButton.onClick.RemoveAllListeners();
        _typewriterTween?.Kill();
    }

    public void ShowDialogue(string speaker, string text, Action onContinue, float typewriterSpeed = 0f, AudioData voice = null)
    {
        _speakerText.text = speaker;
        _dialogueText.text = text;
        _onContinue = onContinue;
        _isShowing = true;

        _dialogueText.maxVisibleCharacters = 0;
        _panel.SetActive(true);

        if (voice != null )
        {
            ServiceLocator.Get<AudioService>().PlayVoice(voice);
        }

        if (typewriterSpeed <= 0f)
        {
            _dialogueText.maxVisibleCharacters = text.Length;
            _continueButton.interactable = true;
        }
        else
        {
            _continueButton.interactable = false;
            // Анимируем печать через DOTween
            int totalChars = text.Length;
            _typewriterTween = DOTween.To(
                () => 0,
                x => _dialogueText.maxVisibleCharacters = Mathf.FloorToInt(x),
                totalChars,
                totalChars * typewriterSpeed
            )
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                // Если текст уже полностью напечатан, разрешаем кнопку
                if (_dialogueText.maxVisibleCharacters >= totalChars)
                {
                    _continueButton.interactable = true;
                }
            })
            .OnComplete(() =>
            {
                _continueButton.interactable = true;
            });
        }
    }

    public void HideDialogue()
    {
        _isShowing = false;
        _typewriterTween?.Kill();
        _panel.SetActive(false);
        _dialogueText.maxVisibleCharacters = 0;
        _onContinue = null;
    }

    private void OnContinueClicked()
    {
        if (!_isShowing) return;

        // Если анимация ещё идёт, мгновенно показываем весь текст и завершаем
        if (_typewriterTween != null && _typewriterTween.IsActive() && _typewriterTween.IsPlaying())
        {
            _typewriterTween.Kill();
            _dialogueText.maxVisibleCharacters = _dialogueText.text.Length;
            _continueButton.interactable = true;
        }

        // Вызываем колбэк (закроет диалог через HideDialogue)
        _onContinue?.Invoke();
    }
}