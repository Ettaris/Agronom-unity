using System;

public class NarrativeRunner
{
    private NarrativeSequence _sequence;
    private int _currentIndex;
    private bool _isRunning;
    private bool _isCancelled;
    private Action _onComplete;
    private Action _onCancel;

    public bool IsRunning => _isRunning;

    public void Start(NarrativeSequence sequence, Action onComplete, Action onCancel = null)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        _sequence = sequence;
        _currentIndex = 0;
        _isRunning = true;
        _isCancelled = false;
        _onComplete = onComplete;
        _onCancel = onCancel;

        ExecuteCurrentStep();
    }

    public void Stop()
    {
        if (!_isRunning) return;
        _isCancelled = true;
        CancelCurrentStep();
        _isRunning = false;
        _onCancel?.Invoke();
    }

    private void ExecuteCurrentStep()
    {
        if (!_isRunning || _isCancelled || _currentIndex >= _sequence.steps.Count)
        {
            CompleteSequence();
            return;
        }

        var step = _sequence.steps[_currentIndex];
        if (step == null)
        {
            GoToNextStep();
            return;
        }

        step.Execute(() =>
        {
            if (_isCancelled) return;
            GoToNextStep();
        });
    }

    private void GoToNextStep()
    {
        _currentIndex++;
        ExecuteCurrentStep();
    }

    private void CompleteSequence()
    {
        if (_isRunning)
        {
            _isRunning = false;
            _onComplete?.Invoke();
        }
    }

    private void CancelCurrentStep()
    {
        if (_sequence == null || _currentIndex >= _sequence.steps.Count) return;
        var step = _sequence.steps[_currentIndex];
        step?.Cancel();
    }
}