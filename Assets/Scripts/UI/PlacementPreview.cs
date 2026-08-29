using UnityEngine;
using TMPro;
using DG.Tweening;
using Gameplay.Calculation;
using System.Collections.Generic;

public class PlacementPreview : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _caloriesText;
    [SerializeField] private TMP_Text _deltaText;
    [SerializeField] private Transform _contributionsContainer;
    [SerializeField] private GameObject _contributionPrefab;
    [SerializeField] private Vector2 _offset = new Vector2(0, 80);
    [SerializeField] private int _caloriesFontSize = 26;

    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 0.15f;
    [SerializeField] private float _entryDelay = 0.05f;

    private RectTransform _rect;
    private List<GameObject> _activeEntries = new List<GameObject>();
    private Queue<GameObject> _entryPool = new Queue<GameObject>();
    private CanvasGroup _canvasGroup;
    private bool _isVisible;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    public void Show(HarvestResult result, Vector2 screenPos, int currentCalories = -1)
    {
        if (result == null) return;

        gameObject.SetActive(true);
        _isVisible = true;

        
        _rect.position = screenPos + _offset;

        _caloriesText.fontSize = _caloriesFontSize;
        _caloriesText.text = $"{result.FinalCalories} ккал";

        if (currentCalories >= 0)
        {
            int delta = result.FinalCalories - currentCalories;
            _deltaText.text = delta >= 0 ? $"+{delta}" : $"{delta}";
            _deltaText.color = delta >= 0 ? Color.green : Color.red;
        }
        else
        {
            _deltaText.text = "";
        }

        foreach (var entry in _activeEntries)
        {
            entry.SetActive(false);
            _entryPool.Enqueue(entry);
        }
        _activeEntries.Clear();

        foreach (var contrib in result.Contributions)
        {
            if (contrib.Source == "Base") continue;
            var entry = GetEntryFromPool();
            var texts = entry.GetComponentsInChildren<TMP_Text>();
            string displayName = contrib.IsKnown ? contrib.ModifierName : "???";
            if (texts.Length >= 2)
            {
                texts[0].text = displayName;
                int val = contrib.ValueChange;
                texts[1].text = val >= 0 ? $"+{val}" : $"{val}";
                texts[1].color = val >= 0 ? Color.green : Color.red;
            }
            entry.transform.SetParent(_contributionsContainer, false);
            entry.SetActive(true);
            _activeEntries.Add(entry);
        }

        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1, _fadeDuration);

        for (int i = 0; i < _activeEntries.Count; i++)
        {
            var entry = _activeEntries[i];
            entry.transform.localScale = Vector3.zero;
            entry.transform.DOScale(Vector3.one, 0.15f)
                .SetDelay(i * _entryDelay)
                .SetEase(Ease.OutBack);
        }
    }

    public void Hide()
    {
        if (!_isVisible) return;
        _isVisible = false;
        _canvasGroup.DOFade(0, _fadeDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private GameObject GetEntryFromPool()
    {
        if (_entryPool.Count > 0)
        {
            var entry = _entryPool.Dequeue();
            return entry;
        }
        else
        {
            return Instantiate(_contributionPrefab, _contributionsContainer);
        }
    }
}