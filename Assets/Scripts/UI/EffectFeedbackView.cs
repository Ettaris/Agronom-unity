using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Events;
using Gameplay;

public class EffectFeedbackView : MonoBehaviour
{
    [SerializeField] private BoardRoot _boardRoot;

    private Dictionary<EffectType, System.Action<CellView, float>> _effectHandlers;

    private void Start()
    {
        if (_boardRoot == null) _boardRoot = ServiceLocator.Get<BoardRoot>();
        if (_boardRoot == null)
        {
            Debug.LogError("EffectFeedbackView: BoardRoot not found!");
            return;
        }

        InitializeHandlers();
        EventBus.Subscribe<EffectAppliedEvent>(OnEffectApplied);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<EffectAppliedEvent>(OnEffectApplied);
    }

    private void InitializeHandlers()
    {
        _effectHandlers = new Dictionary<EffectType, System.Action<CellView, float>>()
        {
            { EffectType.Grow, (cell, dur) => PlayGrowEffect(cell, dur) },
            { EffectType.Sacrifice, (cell, dur) => PlaySacrificeEffect(cell, dur) },
            { EffectType.Bomb, (cell, dur   ) => PlayBombEffect(cell, dur) },
            { EffectType.Weed, (cell, dur) => PlayWeedEffect(cell, dur) },
            { EffectType.Boost, (cell, dur) => PlayBoostEffect(cell, dur) },
            { EffectType.Debuff, (cell, dur) => PlayDebuffEffect(cell, dur) },
        };
    }

    private void OnEffectApplied(EffectAppliedEvent evt)
    {
        var cellView = _boardRoot.GetCellView(evt.X, evt.Y);
        if (cellView == null) return;

        if (_effectHandlers.TryGetValue(evt.Type, out var handler))
            handler(cellView, evt.Duration);
    }

    // ----- Анимации через EffectOverlay -----

    private void PlayGrowEffect(CellView cell, float duration)
    {
        var overlay = cell.EffectOverlay;
        if (overlay == null) return;

        overlay.gameObject.SetActive(true);
        overlay.color = Color.green;
        overlay.transform.localScale = Vector3.one;

        overlay.DOFade(0.8f, duration / 2f);
        overlay.transform.DOScale(Vector3.one * 1.3f, duration).OnComplete(() =>
        {
            overlay.DOFade(0f, 0.1f).OnComplete(() => overlay.gameObject.SetActive(false));
            overlay.transform.localScale = Vector3.one;
        });
    }

    private void PlaySacrificeEffect(CellView cell, float duration)
    {
        var overlay = cell.EffectOverlay;
        if (overlay == null) return;

        overlay.gameObject.SetActive(true);
        overlay.color = Color.red;
        overlay.DOFade(0.6f, duration).OnComplete(() =>
        {
            overlay.DOFade(0f, 0.1f).OnComplete(() => overlay.gameObject.SetActive(false));
        });
    }

    private void PlayBombEffect(CellView cell, float duration)
    {
        var overlay = cell.EffectOverlay;
        if (overlay == null) return;

        overlay.gameObject.SetActive(true);
        overlay.color = new Color(1f, 0.5f, 0f);
        overlay.DOFade(0.9f, duration / 2f);
        overlay.transform.DOScale(Vector3.one * 1.5f, duration / 2f).OnComplete(() =>
        {
            overlay.DOFade(0f, duration / 2f).OnComplete(() =>
            {
                overlay.gameObject.SetActive(false);
                overlay.transform.localScale = Vector3.one;
            });
        });
    }

    private void PlayWeedEffect(CellView cell, float duration)
    {
        var overlay = cell.EffectOverlay;
        if (overlay == null) return;

        overlay.gameObject.SetActive(true);
        overlay.color = Color.green;
        overlay.DOFade(0.5f, duration).OnComplete(() =>
        {
            overlay.DOFade(0f, 0.1f).OnComplete(() => overlay.gameObject.SetActive(false));
        });
    }

    private void PlayBoostEffect(CellView cell, float duration)
    {
        var overlay = cell.EffectOverlay;
        if (overlay == null) return;

        overlay.gameObject.SetActive(true);
        overlay.color = Color.yellow;
        overlay.DOFade(0.6f, duration / 2f);
        overlay.DOColor(Color.white, duration / 2f).OnComplete(() =>
        {
            overlay.DOFade(0f, 0.1f).OnComplete(() => overlay.gameObject.SetActive(false));
        });
    }

    private void PlayDebuffEffect(CellView cell, float duration)
    {
        var overlay = cell.EffectOverlay;
        if (overlay == null) return;

        overlay.gameObject.SetActive(true);
        overlay.color = new Color(0.6f, 0.2f, 0.6f);
        overlay.DOFade(0.7f, duration).OnComplete(() =>
        {
            overlay.DOFade(0f, 0.1f).OnComplete(() => overlay.gameObject.SetActive(false));
        });
    }
}