using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Calculation;
using Gameplay;

public class HarvestBreakdownView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _plantNameText;
    [SerializeField] private Transform _contributionsContainer;
    [SerializeField] private GameObject _contributionPrefab;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private int _finalTextFontSize = 20;
    [SerializeField] private int _commonFontSize = 18;

    [Header("Animations")]
    [SerializeField] private float _fadeDuration = 0.3f;
    [SerializeField] private float _entryDelay = 0.1f;
    [SerializeField] private float _scaleDuration = 0.2f;
    [SerializeField] private Ease _entryEase = Ease.OutBack;
    [SerializeField] private float _autoHideDelay = 3f;

    private RectTransform _rect;
    private Sequence _showSequence;
    private List<GameObject> _activeEntries = new List<GameObject>();
    private Queue<GameObject> _entryPool = new Queue<GameObject>();
    private Coroutine _autoHideCoroutine;
    private bool _isShowing;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    public void Show(HarvestResult result, PlantInstance plant, Vector2 screenPos)
    {
        if (result == null || plant == null) return;
        gameObject.SetActive(true);
        _isShowing = true;

        _rect.position = screenPos + new Vector2(0, 120);
        Vector2 clamped = _rect.position;
        clamped.x = Mathf.Clamp(clamped.x, _rect.rect.width / 2, Screen.width - _rect.rect.width / 2);
        clamped.y = Mathf.Clamp(clamped.y, _rect.rect.height / 2, Screen.height - _rect.rect.height / 2);
        _rect.position = clamped;

        _plantNameText.text = plant.PlantData.itemName;

        foreach (var entry in _activeEntries)
        {
            entry.SetActive(false);
            _entryPool.Enqueue(entry);
        }
        _activeEntries.Clear();

        AddEntry("Базовые калории", $"+{result.BaseCalories}", Color.green, _commonFontSize);

        foreach (var contrib in result.Contributions)
        {
            int val = contrib.ValueChange;
            Color color = val >= 0 ? Color.green : Color.red;
            string displayName = contrib.IsKnown ? contrib.ModifierName : "???";
            AddEntry(displayName, (val >= 0 ? "+" : "") + val, color, _commonFontSize);
        }

        var finalGo = AddEntry("Итог", $"{result.FinalCalories}", Color.white, _finalTextFontSize);
        var texts = finalGo.GetComponentsInChildren<TMP_Text>();
        if (texts.Length >= 2) texts[1].fontStyle = TMPro.FontStyles.Bold;



        _showSequence?.Kill();
        _showSequence = DOTween.Sequence();
        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.zero;
        _showSequence.Append(_canvasGroup.DOFade(1, _fadeDuration));
        _showSequence.Join(transform.DOScale(Vector3.one, _scaleDuration).SetEase(_entryEase));

        for (int i = 0; i < _activeEntries.Count; i++)
        {
            var entry = _activeEntries[i];
            entry.transform.localScale = Vector3.zero;
            _showSequence.Append(entry.transform.DOScale(Vector3.one, 0.2f)
                .SetDelay(i * _entryDelay)
                .SetEase(_entryEase));
        }

        _showSequence.AppendInterval(_autoHideDelay);
        _showSequence.Play();
        _showSequence.OnComplete(() => Hide());
    }

    private GameObject AddEntry(string label, string value, Color color, int fontSize)
    {
        GameObject go = GetEntryFromPool();
        go.transform.SetParent(_contributionsContainer, false);
        go.transform.localScale = Vector3.one;

        var texts = go.GetComponentsInChildren<TMP_Text>();
        if (texts.Length >= 2)
        {
            texts[0].text = label;
            texts[0].fontSize = fontSize;
            texts[1].text = value;
            texts[1].fontSize = fontSize;
            texts[1].color = color;
        }

        _activeEntries.Add(go);
        return go;
    }

    private GameObject GetEntryFromPool()
    {
        if (_entryPool.Count > 0)
        {
            var entry = _entryPool.Dequeue();
            entry.SetActive(true);
            return entry;
        }
        else
        {
            return Instantiate(_contributionPrefab, _contributionsContainer);
        }
    }

    public void Hide()
    {
        if (!_isShowing) return;
        _isShowing = false;
        if (_autoHideCoroutine != null) StopCoroutine(_autoHideCoroutine);
        _showSequence?.Kill();
        _canvasGroup.DOFade(0, _fadeDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);
            HarvestBreakdownPool.Instance?.ReturnToPool(this);
        });
    }

    private void OnDestroy()
    {
        _showSequence?.Kill();
    }
}