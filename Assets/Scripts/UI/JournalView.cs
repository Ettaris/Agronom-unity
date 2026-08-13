using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Data;
using Systems;
using Gameplay;

/// <summary>
/// UI журнала открытых свойств.
/// Отображает список геномов, обнаруженных игроком.
/// Использует пул записей для минимизации аллокаций.
/// </summary>

public class JournalView : MonoBehaviour, IGameSystem
{

    //TODO: If there will be a good reason, make different interfaces for UI and Systems, or general like IInitializble and IDisposable.

    [Header("UI References")]
    [SerializeField] private Transform _entriesContainer;
    [SerializeField] private GameObject _entryPrefab;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _openButton;
    [SerializeField] private TMP_Text _emptyLabel;

    [Header("Pagination")]
    [SerializeField] private Button _prevButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private TMP_Text _pageText;
    [SerializeField] private int _entriesPerPage = 3;

    [Header("Animator")]
    [SerializeField] private Animator _journalAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _entryAppearDuration = 0.3f;
    [SerializeField] private float _entryAppearDelayOffset = 0.05f;
    [SerializeField] private float _entrySlideDistance = 50f;

    private JournalSystem _journalSystem;
    private List<JournalEntryView> _activeEntries = new List<JournalEntryView>();
    private Queue<JournalEntryView> _entryPool = new Queue<JournalEntryView>();
    private bool _isOpen;
    private int _currentPage = 0;
    private int _totalPages = 0;
    private List<JournalPlantEntry> _allEntries = new List<JournalPlantEntry>();


    public void Initialize()
    {
        EventBus.Subscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
        _closeButton.onClick.AddListener(CloseJournal);
        _prevButton.onClick.AddListener(PreviousPage);
        _nextButton.onClick.AddListener(NextPage);
        _openButton.onClick.AddListener(OpenJournal);

        gameObject.SetActive(false);
        _isOpen = false;

        _journalSystem = ServiceLocator.Get<JournalSystem>();
        if (_journalSystem == null)
            Debug.LogError("JournalSystem not found!");
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
        _closeButton.onClick.RemoveAllListeners();
        _prevButton.onClick.RemoveAllListeners();
        _nextButton.onClick.RemoveAllListeners();
    }

    private void OnGenomeDiscovered(GenomeDiscoveredEvent evt)
    {
        if (_isOpen)
        {
            RefreshJournal();
        }
    }

    public void OpenJournal()
    {
        gameObject.SetActive(true);
        _journalAnimator.SetTrigger("Open");
        _isOpen = true;
        RefreshJournal();
    }

    public void CloseJournal()
    {
        _journalAnimator.SetTrigger("Close");
        _isOpen = false;
        DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
    }

    private void RefreshJournal()
    {
        var journalData = _journalSystem.GetJournal();
        _allEntries = journalData.plantEntries;
        _totalPages = Mathf.CeilToInt((float)_allEntries.Count / _entriesPerPage);
        if (_totalPages == 0) _totalPages = 1;

        if (_currentPage >= _totalPages)
            _currentPage = 0;

        foreach (var entry in _activeEntries)
        {
            entry.Hide(() => ReturnEntryToPool(entry));
        }
        _activeEntries.Clear();

        _emptyLabel.gameObject.SetActive(_allEntries.Count == 0);
        if (_allEntries.Count == 0)
        {
            UpdatePaginationButtons();
            return;
        }

        int startIndex = _currentPage * _entriesPerPage;
        int endIndex = Mathf.Min(startIndex + _entriesPerPage, _allEntries.Count);

        int displayIndex = 0;
        for (int i = startIndex; i < endIndex; i++)
        {
            JournalEntryView entry = GetEntryFromPool();
            entry.Setup(_allEntries[i]);
            entry.transform.SetParent(_entriesContainer, false);

            entry.transform.localPosition = new Vector3(_entrySlideDistance, 0, 0);
            entry.CanvasGroup.alpha = 0;

            entry.transform.DOLocalMoveX(0, _entryAppearDuration)
                .SetDelay(displayIndex * _entryAppearDelayOffset)
                .SetEase(Ease.OutQuad);
            entry.CanvasGroup.DOFade(1, _entryAppearDuration)
                .SetDelay(displayIndex * _entryAppearDelayOffset);

            _activeEntries.Add(entry);
            displayIndex++;
        }

        UpdatePaginationButtons();
    }

    private void UpdatePaginationButtons()
    {
        _prevButton.interactable = _currentPage > 0;
        _nextButton.interactable = _currentPage < _totalPages - 1 && _allEntries.Count > 0;
        _pageText.text = $"{_currentPage + 1}/{_totalPages}";
    }

    private void PreviousPage() { if (_currentPage > 0) { _currentPage--; RefreshJournal(); } }
    private void NextPage() { if (_currentPage < _totalPages - 1) { _currentPage++; RefreshJournal(); } }

    private JournalEntryView GetEntryFromPool()
    {
        if (_entryPool.Count > 0)
        {
            var entry = _entryPool.Dequeue();
            entry.gameObject.SetActive(true);
            return entry;
        }
        else
        {
            GameObject obj = Instantiate(_entryPrefab, _entriesContainer);
            return obj.GetComponent<JournalEntryView>();
        }
    }

    private void ReturnEntryToPool(JournalEntryView entry)
    {
        entry.gameObject.SetActive(false);
        entry.transform.localScale = Vector3.one;
        entry.CanvasGroup.alpha = 1f;
        _entryPool.Enqueue(entry);
    }

}