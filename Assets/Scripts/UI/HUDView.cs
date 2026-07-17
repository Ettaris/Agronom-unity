using UnityEngine;
using TMPro; // TextMeshPro
using Infrastructure;
using Infrastructure.Events;
using Commands;
using System.Collections;

public class HUDView : MonoBehaviour
{
    [Header("UI References (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private TextMeshProUGUI _caloriesText;
    [SerializeField] private TextMeshProUGUI _quotaText;
    [SerializeField] private GameObject _quotaReachedIndicator;
    [SerializeField] private UnityEngine.UI.Button _endDayButton;

    [Header("Animator")]
    [SerializeField] private Animator _hudAnimator;

    [Header("Animation Settings")]
    [SerializeField] private float _numberChangeDuration = 0.5f;

    private int _currentCalories;
    private int _currentDay;
    private int _dailyQuota;
    private Coroutine _caloriesCoroutine;
    private Coroutine _dayCoroutine;

    private void Awake()
    {
        // Подписка на события
        EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Subscribe<QuotaReachedEvent>(OnQuotaReached);
        EventBus.Subscribe<DayLoadedEvent>(OnDayLoaded);      // НОВОЕ
        EventBus.Subscribe<RunLoadedEvent>(OnRunLoaded);      // НОВОЕ

        // Кнопка завершения дня
        _endDayButton.onClick.AddListener(OnEndDayButtonClicked);

        // Инициализация значений
        _dayText.text = "Day 0";
        _caloriesText.text = "0";
        _quotaText.text = "0";
        _quotaReachedIndicator.SetActive(false);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        EventBus.Unsubscribe<DayLoadedEvent>(OnDayLoaded);
        EventBus.Unsubscribe<RunLoadedEvent>(OnRunLoaded);
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Unsubscribe<QuotaReachedEvent>(OnQuotaReached);
    }

    private void OnDayStarted(DayStartedEvent evt)
    {
        _currentDay = evt.DayNumber;
        if (_dayCoroutine != null) StopCoroutine(_dayCoroutine);
        _dayCoroutine = StartCoroutine(AnimateNumberChange(_dayText, _currentDay, _numberChangeDuration));
        _hudAnimator.SetTrigger("DayChanged");
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        _currentCalories = evt.CurrentCalories;
        _dailyQuota = evt.DailyQuota;
        if (_caloriesCoroutine != null) StopCoroutine(_caloriesCoroutine);
        _caloriesCoroutine = StartCoroutine(AnimateNumberChange(_caloriesText, _currentCalories, _numberChangeDuration));
        _quotaText.text = _dailyQuota.ToString();
        _hudAnimator.SetTrigger("CaloriesUpdated");
    }

    private void OnQuotaReached(QuotaReachedEvent evt)
    {
        _quotaReachedIndicator.SetActive(true);
        _hudAnimator.SetTrigger("QuotaReached");
    }

    private void OnDayLoaded(DayLoadedEvent evt)
    {
        _currentDay = evt.DayNumber;
        if (_dayCoroutine != null) StopCoroutine(_dayCoroutine);
        _dayCoroutine = StartCoroutine(AnimateNumberChange(_dayText, _currentDay, _numberChangeDuration));
        _hudAnimator.SetTrigger("DayChanged");
    }

    // Обработчик загрузки забега (восстанавливаем все значения)
    private void OnRunLoaded(RunLoadedEvent evt)
    {
        var runData = evt.RunData;
        _currentDay = runData.CurrentDay;
        _currentCalories = runData.Inventory.Calories;
        _dailyQuota = runData.DailyQuota;

        _dayText.text = _currentDay.ToString();
        _caloriesText.text = _currentCalories.ToString();
        _quotaText.text = _dailyQuota.ToString();
        _quotaReachedIndicator.SetActive(runData.IsQuotaReached);

        // Можно также запустить анимацию обновления
        _hudAnimator.SetTrigger("CaloriesUpdated");
        _hudAnimator.SetTrigger("DayChanged");
    }

    private void OnEndDayButtonClicked()
    {
        CommandProcessor.Execute(new EndDayCommand());
        _hudAnimator.SetTrigger("EndDayPressed");
    }

    private IEnumerator AnimateNumberChange(TMP_Text textComponent, int targetValue, float duration)
    {
        int startValue;
        if (!int.TryParse(textComponent.text, out startValue))
            startValue = 0;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, smoothT));
            textComponent.text = currentValue.ToString();
            yield return null;
        }
        textComponent.text = targetValue.ToString();
    }

    public void SetInitialValues(int day, int calories, int quota)
    {
        _currentDay = day;
        _currentCalories = calories;
        _dailyQuota = quota;
        _dayText.text = day.ToString();
        _caloriesText.text = calories.ToString();
        _quotaText.text = quota.ToString();
        _quotaReachedIndicator.SetActive(false);
    }
}