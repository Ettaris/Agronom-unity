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

public class GameOverView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _dayText;
    [SerializeField] private TMP_Text _resultText;
    [SerializeField] private TMP_Text _caloriesText;
    [SerializeField] private TMP_Text _propertiesText;
    [SerializeField] private TMP_Text _plantsPlantedText;
    [SerializeField] private TMP_Text _plantsHarvestedText;
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

    private void Awake()
    {
        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
        if (_runData == null)
        {
            Debug.LogError("GameOverView: RunData is null!");
            return;
        }

        _journalSystem = ServiceLocator.Get<JournalSystem>();

        _restartButton.onClick.AddListener(OnRestartClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        EventBus.Subscribe<RunEndedEvent>(OnRunEnded);

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<RunEndedEvent>(OnRunEnded);
        _restartButton.onClick.RemoveAllListeners();
        _mainMenuButton.onClick.RemoveAllListeners();
    }

    private void OnRunEnded(RunEndedEvent evt)
    {
        ShowResults(evt.FinalRunData, evt.IsWin);
    }

    public void ShowResults(RunData runData, bool isWin)
    {
        if (runData == null)
        {
            Debug.LogError("GameOverView: RunData is null!");
            return;
        }

        gameObject.SetActive(true);
        _gameOverAnimator.SetTrigger("Open");

        // Собираем статистику
        int daysSurvived = runData.CurrentDay;
        int totalCalories = runData.Inventory.Calories;
        int discoveredProperties = _journalSystem.GetJournal().GetAllEntries().Count;

        // Подсчёт посаженных/собранных растений можно вести в RunData, но пока нет
        // Добавим заглушки (можно расширить RunData позже)
        int plantsPlanted = 0;
        int plantsHarvested = 0;

        // Заполняем тексты
        _dayText.text = "0";
        _caloriesText.text = "0";
        _propertiesText.text = "0";
        _plantsPlantedText.text = "0";
        _plantsHarvestedText.text = "0";

        _resultText.text = isWin ? "Победа!" : "Поражение!";
        _resultText.color = isWin ? Color.green : Color.red;

        // Анимируем числа (каждое с небольшой задержкой)
        AnimateNumber(_dayText, daysSurvived, 0f);
        AnimateNumber(_caloriesText, totalCalories, 0.2f);
        AnimateNumber(_propertiesText, discoveredProperties, 0.4f);
        AnimateNumber(_plantsPlantedText, plantsPlanted, 0.6f);
        AnimateNumber(_plantsHarvestedText, plantsHarvested, 0.8f);

        // Очищаем старые записи статистики
        foreach (var entry in _statEntries)
            Destroy(entry);
        _statEntries.Clear();

        // Можно добавить дополнительные записи (например, список открытых свойств)
        // Для простоты покажем только количество, но при желании можно вывести список
        // Добавим 2-3 примера записей
        AddStatEntry("Свойств открыто", discoveredProperties.ToString(), 0.4f);
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