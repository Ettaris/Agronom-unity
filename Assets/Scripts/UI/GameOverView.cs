using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using Data;
using Managers;
using Systems;

public class GameOverView : MonoBehaviour, IGameSystem
{
    //TODO: clear dependencies and make API for getting all needed stats.

    [Header("UI References")]
    [SerializeField] private TMP_Text _dayText;
    [SerializeField] private TMP_Text _caloriesText;
    [SerializeField] private TMP_Text _propertiesText;
    [SerializeField] private TMP_Text _resultText;
    [SerializeField] private Transform _statsContainer;
    [SerializeField] private GameObject _statEntryPrefab;

    [Header("Buttons")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("Animator")]
    [SerializeField] private Animator _gameOverAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _numberAnimationDuration = 1.0f;
    [SerializeField] private float _statEntryDelay = 0.1f;

    private RunData _runData;
    private JournalSystem _journalSystem;
    private List<GameObject> _statEntries = new List<GameObject>();

    public void Initialize()
    {
        _restartButton.onClick.AddListener(OnRestartClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        EventBus.Subscribe<RunEndedEvent>(OnRunEnded);
        EventBus.Subscribe<StageFailedEvent>(OnStageFailed);
        EventBus.Subscribe<GameWinEvent>(OnGameWin);

        _journalSystem = ServiceLocator.Get<JournalSystem>();
        gameObject.SetActive(false);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<RunEndedEvent>(OnRunEnded);
        EventBus.Unsubscribe<StageFailedEvent>(OnStageFailed);
        EventBus.Unsubscribe<GameWinEvent>(OnGameWin);
        _restartButton.onClick.RemoveAllListeners();
        _mainMenuButton.onClick.RemoveAllListeners();
    }

    private void OnRunEnded(RunEndedEvent evt)
    {
        _runData = evt.FinalRunData;
        ShowResults(_runData, evt.IsWin ? "Победа!" : "Поражение!");
    }

    private void OnStageFailed(StageFailedEvent evt)
    {
        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
        string message = $"Не удалось набрать {evt.RequiredCalories} калорий\nСобрано: {evt.CurrentCalories}";
        ShowResults(_runData, message);
        _resultText.color = Color.red;
    }

    private void OnGameWin(GameWinEvent evt)
    {
        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
        ShowResults(_runData, "Победа! Вы прошли все этапы!");
        _resultText.color = Color.green;
    }

    public void ShowResults(RunData runData, string resultMessage)
    {
        if (runData == null)
        {
            Debug.LogError("GameOverView: RunData is null!");
            return;
        }

        gameObject.SetActive(true);
        _gameOverAnimator.SetTrigger("Open");

        int daysSurvived = runData.CurrentDay;
        int totalCalories = runData.Inventory.Calories;

        // Заглушки (можно расширить RunData позже)
        int plantsPlanted = 0;
        int plantsHarvested = 0;

        _dayText.text = "0";
        _caloriesText.text = "0";
        _propertiesText.text = "0";

        _resultText.text = resultMessage;
        _resultText.color = Color.black; // будет переопределён в обработчиках

        AnimateNumber(_dayText, daysSurvived, 0f);
        AnimateNumber(_caloriesText, totalCalories, 0.2f);

        foreach (var entry in _statEntries)
            Destroy(entry);
        _statEntries.Clear();

        AddStatEntry("Посажено растений", plantsPlanted.ToString(), 0.6f);
        AddStatEntry("Собрано растений", plantsHarvested.ToString(), 0.8f);
    }

    private void AnimateNumber(TMP_Text textComponent, int targetValue, float delay)
    {
        int startValue = 0;
        textComponent.text = "0";
        float duration = _numberAnimationDuration;

        DOVirtual.Float(startValue, targetValue, duration, (value) =>
        {
            textComponent.text = Mathf.RoundToInt(value).ToString();
        }).SetDelay(delay).SetEase(Ease.OutQuad);
    }

    private void AddStatEntry(string label, string value, float delay)
    {
        GameObject entryObj = Instantiate(_statEntryPrefab, _statsContainer);
        var labelText = entryObj.transform.Find("Label")?.GetComponent<TMP_Text>();
        var valueText = entryObj.transform.Find("Value")?.GetComponent<TMP_Text>();

        if (labelText != null) labelText.text = label;
        if (valueText != null) valueText.text = value;

        entryObj.transform.localScale = Vector3.zero;
        entryObj.transform.DOScale(Vector3.one, 0.3f).SetDelay(delay).SetEase(Ease.OutBack);
        _statEntries.Add(entryObj);
    }

    private void OnRestartClicked()
    {
        _gameOverAnimator.SetTrigger("Close");
        DOVirtual.DelayedCall(0.5f, () =>
        {
            CommandProcessor.Execute(new RestartRunCommand());
            gameObject.SetActive(false);
        });
    }

    private void OnMainMenuClicked()
    {
        _gameOverAnimator.SetTrigger("Close");
        DOVirtual.DelayedCall(0.5f, () =>
        {
            CommandProcessor.Execute(new GoToMainMenuCommand());
            gameObject.SetActive(false);
        });
    }


}