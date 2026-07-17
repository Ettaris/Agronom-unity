using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Infrastructure;
using Commands;
using Managers;
using System.Threading.Tasks;

public class MainMenuView : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    [Header("Animator")]
    [SerializeField] private Animator _mainMenuAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _buttonAppearDelay = 0.2f;
    [SerializeField] private float _buttonAppearDuration = 0.5f;
    [SerializeField] private float _buttonBounceAmplitude = 0.2f;

    [Header("Version Info")]
    [SerializeField] private TMP_Text _versionText;

    private bool _hasSave;

    private void Awake()
    {
        // Подписка на кнопки
        _newGameButton.onClick.AddListener(OnNewGameClicked);
        _continueButton.onClick.AddListener(OnContinueClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);

        // Проверяем наличие сохранения
        CheckSaveExists();

        // Версия
        if (_versionText != null)
            _versionText.text = "v" + Application.version;

        // Анимация появления кнопок
        AnimateButtons();
    }

    private void OnDestroy()
    {
        _newGameButton.onClick.RemoveAllListeners();
        _continueButton.onClick.RemoveAllListeners();
        _settingsButton.onClick.RemoveAllListeners();
        _quitButton.onClick.RemoveAllListeners();
    }

    private void CheckSaveExists()
    {
        var saveManager = ServiceLocator.TryGet<SaveManager>(out var sm) ? sm : null;
        if (saveManager != null)
        {
            // Проверяем наличие сохранения (например, наличие файла журнала или состояния забега)
            _hasSave = saveManager.HasSave;
            _continueButton.interactable = _hasSave;
        }
        else
        {
            _continueButton.interactable = false;
        }
    }

    private void AnimateButtons()
    {
        // Список кнопок для анимации
        Button[] buttons = { _newGameButton, _continueButton, _settingsButton, _quitButton };
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            var btn = buttons[i];
            // Начальное состояние (масштаб 0, прозрачность 0)
            btn.transform.localScale = Vector3.zero;
            btn.GetComponent<CanvasGroup>().alpha = 0;

            // Анимация появления с задержкой
            btn.transform.DOScale(Vector3.one, _buttonAppearDuration)
                .SetDelay(i * _buttonAppearDelay)
                .SetEase(Ease.OutBack, _buttonBounceAmplitude);
            btn.GetComponent<CanvasGroup>().DOFade(1, _buttonAppearDuration)
                .SetDelay(i * _buttonAppearDelay);
        }
    }

    private void OnNewGameClicked()
    {
        _mainMenuAnimator.SetTrigger("StartGame");
        DOVirtual.DelayedCall(0.5f, () =>
        {
            CommandProcessor.Execute(new StartNewGameCommand());
        });
    }

    private async void OnContinueClicked()
    {
        if (!_hasSave) return;
        _mainMenuAnimator.SetTrigger("Continue");
        await Task.Delay(500);
        CommandProcessor.Execute(new ContinueGameCommand());
    }

    private void OnSettingsClicked()
    {
        // Можно открыть панель настроек (если есть)
        // Пока просто отправляем команду (или напрямую вызываем открытие)
        Debug.Log("Settings clicked");
        // Пример: ServiceLocator.Get<SettingsView>().Open();
    }

    private void OnQuitClicked()
    {
        _mainMenuAnimator.SetTrigger("Quit");
        DOVirtual.DelayedCall(0.5f, () =>
        {
            CommandProcessor.Execute(new QuitGameCommand());
        });
    }

    // Метод для переключения видимости меню (например, из скрипта перехода)
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        if (visible)
        {
            _mainMenuAnimator.SetTrigger("Open");
            AnimateButtons();
        }
    }
}