using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Data;
using Systems;

/// <summary>
/// UI журнала открытых свойств.
/// ќтображает список геномов, обнаруженных игроком.
/// »спользует пул записей дл€ минимизации аллокаций.
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

    public void Initialize()
    {
        _closeButton.onClick.AddListener(CloseJournal);
        _openButton.onClick.AddListener(OpenJournal);
        EventBus.Subscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);

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
        var entries = journalData.GetAllEntries();
        

        // —крываем все активные записи с анимацией
        foreach (var entry in _activeEntries)
        {
            entry.Hide(() => ReturnEntryToPool(entry));
        }
        _activeEntries.Clear();

        _emptyLabel.gameObject.SetActive(entries.Count == 0);
        if (entries.Count == 0) return;

        int index = 0;
        foreach (var kvp in entries)
        {
            JournalEntryView entry = GetEntryFromPool();
            entry.Setup(kvp.Key, kvp.Value);
            entry.transform.SetParent(_entriesContainer, false);

            // Ќачальное состо€ние дл€ анимации
            entry.transform.localPosition = new Vector3(_entrySlideDistance, 0, 0);
            entry.CanvasGroup.alpha = 0;

            // јнимаци€ по€влени€ с задержкой
            entry.transform.DOLocalMoveX(0, _entryAppearDuration)
                .SetDelay(index * _entryAppearDelayOffset)
                .SetEase(Ease.OutQuad);
            entry.CanvasGroup.DOFade(1, _entryAppearDuration)
                .SetDelay(index * _entryAppearDelayOffset);

            _activeEntries.Add(entry);
            index++;
        }
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
        _entryPool.Enqueue(entry);
    }


}