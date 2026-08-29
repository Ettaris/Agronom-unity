using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Infrastructure;
using Infrastructure.Events;
using Data;
using Systems;

public class JournalView : MonoBehaviour, IGameSystem
{
    public enum Tab { Plants, Modifiers }

    [Header("UI References")]
    [SerializeField] private Transform _entriesContainer;
    [SerializeField] private GameObject _entryPrefab;
    [SerializeField] private Button _openButton;
    [SerializeField] private TMP_Text _emptyLabel;
    [SerializeField] private GameObject _backgroundBlockRaycast;

    [Header("Tabs")]
    [SerializeField] private Button _plantsTabButton;
    [SerializeField] private Button _modifiersTabButton;

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
    private List<IJournalEntryData> _allEntries = new List<IJournalEntryData>();
    private Tab _currentTab = Tab.Plants;

    public void Initialize()
    {
        _journalSystem = ServiceLocator.Get<JournalSystem>();
        if (_journalSystem == null) Debug.LogError("JournalSystem not found!");

        _prevButton.onClick.AddListener(PreviousPage);
        _nextButton.onClick.AddListener(NextPage);
        _openButton.onClick.AddListener(OpenJournal);
        _plantsTabButton.onClick.AddListener(() => SwitchTab(Tab.Plants));
        _modifiersTabButton.onClick.AddListener(() => SwitchTab(Tab.Modifiers));

        gameObject.SetActive(false);
        _isOpen = false;
        SwitchTab(Tab.Plants);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
        _prevButton.onClick.RemoveAllListeners();
        _nextButton.onClick.RemoveAllListeners();
    }

    private void OnGenomeDiscovered(GenomeDiscoveredEvent evt)
    {
        if (_isOpen) RefreshJournal();
    }

    private IEnumerator SetActiveFalseByEndOfAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        float length = _journalAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length - 0.2f);
        gameObject.SetActive(false);
    }

    public void OpenJournal()
    {
        gameObject.SetActive(true);
        _journalAnimator.SetTrigger("Open");
        _isOpen = true;
        RefreshJournal();
        _backgroundBlockRaycast.SetActive(true);
    }

    public void CloseJournal()
    {
        if (_isOpen)
        {
            _journalAnimator.SetTrigger("Close");
            _isOpen = false;
            StartCoroutine(SetActiveFalseByEndOfAnimation());
        }
    }

    private void SwitchTab(Tab tab)
    {
        _currentTab = tab;
        _currentPage = 0;
        RefreshJournal();
    }

    private void RefreshJournal()
    {
        if (_currentTab == Tab.Plants)
            _allEntries = _journalSystem.GetPlantEntries();
        else
            _allEntries = _journalSystem.GetModifierEntries();

        _totalPages = Mathf.CeilToInt((float)_allEntries.Count / _entriesPerPage);
        if (_totalPages == 0) _totalPages = 1;
        if (_currentPage >= _totalPages) _currentPage = 0;

        foreach (var entry in _activeEntries)
            entry.Hide(() => ReturnEntryToPool(entry));
        _activeEntries.Clear();

        _emptyLabel.gameObject.SetActive(_allEntries.Count == 0);
        if (_allEntries.Count == 0)
        {
            UpdatePaginationButtons();
            return;
        }

        int startIndex = _currentPage * _entriesPerPage;
        int endIndex = Mathf.Min(startIndex + _entriesPerPage, _allEntries.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            JournalEntryView entry = GetEntryFromPool();
            entry.Setup(_allEntries[i]);
            entry.transform.SetParent(_entriesContainer, false);

            entry.transform.localPosition = new Vector3(_entrySlideDistance, 0, 0);
            entry.CanvasGroup.alpha = 0;

            entry.transform.DOLocalMoveX(0, _entryAppearDuration)
                .SetDelay((i - startIndex) * _entryAppearDelayOffset)
                .SetEase(Ease.OutQuad);
            entry.CanvasGroup.DOFade(1, _entryAppearDuration)
                .SetDelay((i - startIndex) * _entryAppearDelayOffset);

            _activeEntries.Add(entry);
        }

        UpdatePaginationButtons();
    }

    private void UpdatePaginationButtons()
    {
        _prevButton.interactable = _currentPage > 0;
        _nextButton.interactable = _currentPage < _totalPages - 1 && _allEntries.Count > 0;
        _pageText.text = $"{_currentPage + 1}/{_totalPages}";
    }

    private void PreviousPage()
    {
        if (_currentPage > 0) { _currentPage--; RefreshJournal(); }
        AudioService.Instance.PlaySfx(AudioService.Instance.Config.journalPageSwitchSfx);
    }

    private void NextPage()
    {
        if (_currentPage < _totalPages - 1) { _currentPage++; RefreshJournal(); }
    }

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