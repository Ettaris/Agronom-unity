using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;

public class HUDView : MonoBehaviour
{
    [Header("UI References (TextMeshPro)")]
    [SerializeField] private TMP_Text _dayText;
    [SerializeField] private TMP_Text _caloriesText;
    [SerializeField] private TMP_Text _stageText; // опционально
    [SerializeField] private Slider _stageProgressSlider; // опционально
    [SerializeField] private Button _endDayButton;
    [SerializeField] private Button _labButton;

    [Header("Animator")]
    [SerializeField] private Animator _hudAnimator;

    [Header("Animation Settings")]
    [SerializeField] private float _numberChangeDuration = 0.5f;

    private RunData _runData;
    private int _currentDayInStage;
    private int _totalDaysInStage;
    private int _requiredCalories;
    private int _currentCalories;
    private Coroutine _caloriesCoroutine;

    private void Awake()
    {
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Subscribe<StageChangedEvent>(OnStageChanged);

        _endDayButton.onClick.AddListener(OnEndDayButtonClicked);
        _labButton.onClick.AddListener(OnLabButtonClicked);

        // Инициализация текстов
        _dayText.text = "0/0";
        _caloriesText.text = "0/0";
        if (_stageText != null) _stageText.text = "";
        if (_stageProgressSlider != null) _stageProgressSlider.value = 0;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Unsubscribe<StageChangedEvent>(OnStageChanged);
        _labButton.onClick.RemoveListener(OnLabButtonClicked);
    }

    private void OnLabButtonClicked()
    {
        var labView = ServiceLocator.TryGet<LaboratoryView>(out var lab) ? lab : null;
        if (labView == null)
        {
            // Если не зарегистрирован, пробуем найти на сцене
            labView = FindAnyObjectByType<LaboratoryView>(FindObjectsInactive.Include);
            if (labView != null)
            {
                ServiceLocator.Register(labView);
            }
            else
            {
                Debug.LogError("LaboratoryView not found in scene!");
                return;
            }
        }

        if (labView.gameObject.activeInHierarchy)
        {
            labView.CloseLab();
        }
        else
        {
            labView.OpenLab();
        }
    }

    private void OnRunStarted(RunStartedEvent evt)
    {
        _runData = evt.RunData;
        Debug.Log(_runData);
        Debug.Log(_runData.Stages[0].totalDays);
        Debug.Log(_runData.Stages[1].totalDays);
        Debug.Log(_runData.Stages[1].requiredCalories);
        UpdateHUD(instant: true);
    }

    private void OnDayStarted(DayStartedEvent evt)
    {
        if (_runData == null) return;
        UpdateHUD(instant: true);
        _hudAnimator.SetTrigger("DayChanged");
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        Debug.Log("score changed HUD");
        if (_runData == null) return;
        _currentCalories = evt.CurrentCalories;
        AnimateCalories(_currentCalories);
        UpdateStageProgress();
    }

    private void OnStageChanged(StageChangedEvent evt)
    {
        Debug.Log("stage changed HUD");
        if (_runData == null) return;
        UpdateHUD(instant: true);
        _hudAnimator.SetTrigger("StageChanged");
    }

    private void UpdateHUD(bool instant = false)
    {
        if (_runData == null)
        {
            Debug.LogWarning("HUDView: _runData is null, cannot update HUD.");
            return;
        }

        var stage = _runData.GetCurrentStage();
        if (_runData.Stages == null || _runData.Stages.Length == 0)
        {
            _dayText.text = "--/--";
            _caloriesText.text = "--/--";
            if (_stageText != null) _stageText.text = "No stages";
            if (_stageProgressSlider != null) _stageProgressSlider.value = 0;
            return;
        }

        if (stage.totalDays == 0)
        {
            Debug.LogWarning("HUDView: totalDays is 0, skipping update.");
            return;
        }

        // Количество завершённых дней = текущий день - 1 (в начале игры 0 завершённых дней)
        int completedDays = _runData.CurrentDay;
        int totalDays = stage.totalDays; // нарастающий итог

        _requiredCalories = stage.requiredCalories;
        _currentCalories = _runData.Inventory.Calories;

        Debug.Log($"HUDView: completedDays={completedDays}, totalDays={totalDays}, calories={_currentCalories}/{_requiredCalories}");

        if (instant)
        {
            _dayText.text = $"{completedDays}/{totalDays}";
            _caloriesText.text = $"{_currentCalories}/{_requiredCalories}";
            UpdateStageProgress();
        }
        else
        {
            _dayText.text = $"{completedDays}/{totalDays}";
            AnimateCalories(_currentCalories);
        }

        if (_stageText != null)
            _stageText.text = $"Этап {_runData.CurrentStageIndex + 1}";
    }

    private void UpdateStageProgress()
    {
        if (_stageProgressSlider != null)
        {
            float progress = _requiredCalories > 0 ? (float)_currentCalories / _requiredCalories : 0;
            _stageProgressSlider.value = Mathf.Clamp01(progress);
        }
    }

    private void AnimateCalories(int targetCalories)
    {
        if (_caloriesCoroutine != null) StopCoroutine(_caloriesCoroutine);
        _caloriesCoroutine = StartCoroutine(AnimateNumberChange(_caloriesText, targetCalories, _numberChangeDuration));
    }

    private System.Collections.IEnumerator AnimateNumberChange(TMP_Text textComponent, int targetValue, float duration)
    {
        int startValue = 0;
        if (textComponent.text.Contains("/"))
        {
            string[] parts = textComponent.text.Split('/');
            int.TryParse(parts[0], out startValue);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, smoothT));
            textComponent.text = $"{currentValue}/{_requiredCalories}";
            yield return null;
        }
        textComponent.text = $"{targetValue}/{_requiredCalories}";
        _caloriesCoroutine = null;
    }

    private void OnEndDayButtonClicked()
    {
        CommandProcessor.Execute(new EndDayCommand());
        _hudAnimator.SetTrigger("EndDayPressed");
    }

    // Метод для принудительной установки (можно использовать при загрузке)
    public void SetInitialValues(int day, int calories, int totalDays, int requiredCalories)
    {
        _dayText.text = $"{day}/{totalDays}";
        _caloriesText.text = $"{calories}/{requiredCalories}";
        UpdateStageProgress();
    }
}