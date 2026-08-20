using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class FadeStep : NarrativeStep
{
    [Header("Target")]
    [Tooltip("ID of the GameObject to fade (must be registered in UIReferenceManager).")]
    public string targetId;

    [Header("Fade Settings")]
    [Range(0f, 1f)]
    public float startAlpha = 0f;
    [Range(0f, 1f)]
    public float endAlpha = 1f;
    public float duration = 1f;

    [Header("Behaviour")]
    public bool disableOnComplete = false;
    public bool activateBeforeStart = false;

    [NonSerialized] private Tween _tween;
    [NonSerialized] private CanvasGroup _canvasGroup;

    public override void Execute(Action onComplete)
    {
        var target = UIReferenceManager.Instance?.GetObject(targetId);
        if (target == null)
        {
            Debug.LogWarning($"FadeStep: target '{targetId}' not found.");
            onComplete?.Invoke();
            return;
        }

        if (activateBeforeStart && !target.activeSelf)
            target.SetActive(true);

        _canvasGroup = target.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = target.AddComponent<CanvasGroup>();
        }

        _canvasGroup.alpha = startAlpha;
        _tween = _canvasGroup.DOFade(endAlpha, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (disableOnComplete)
                    target.SetActive(false);
                onComplete?.Invoke();
            });
    }

    public override void Cancel()
    {
        _tween?.Kill();
        _tween = null;
    }
}