using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using TMPro;
using Data;
using Managers;
using Systems;
using System;

/// <summary>
/// Окно ежедневного выбора карточек.
/// </summary>
public class CardDrawView : MonoBehaviour, IGameSystem, IRunAware
{
    [Header("UI References")]
    [SerializeField] private Transform _cardsContainer;
    [SerializeField] private CardView _cardPrefab; // используем тот же префаб, что в HandView
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _skipButton; // опционально
    [SerializeField] private TMP_Text _selectionCounterText;

    [Header("Animator")]
    [SerializeField] private Animator _windowAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _cardSpacing = 1f;
    [SerializeField] private float _cardAppearDelay = 0.1f;
    [SerializeField] private float _cardAppearDuration = 0.4f;
    [SerializeField] private float _cardBounceAmplitude = 0.2f;

    private RunData _runData;
    private CardDrawSystem _cardDrawSystem;
    private List<CardView> _cardViews = new List<CardView>();
    private List<CardView> _selectedCards = new List<CardView>();
    private int _maxSelectable;
    private bool _isProcessing;

    public void Initialize()
    {
        _confirmButton.onClick.AddListener(OnConfirmClicked);
        if (_skipButton != null) _skipButton.onClick.AddListener(OnSkipClicked);

        EventBus.Subscribe<OfferGeneratedEvent>(OnOfferGenerated);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<OfferGeneratedEvent>(OnOfferGenerated);
        _confirmButton.onClick.RemoveAllListeners();
        if (_skipButton != null) _skipButton.onClick.RemoveAllListeners();
    }

    public void OnRunDataSetup(RunData runData)
    {
        _runData = runData;
        if (_runData == null)
        {
            Debug.LogError("CardDrawView: RunData is null");
            return;
        }

        _cardDrawSystem = ServiceLocator.Get<CardDrawSystem>();
        if (_cardDrawSystem == null)
        {
            Debug.LogError("CardDrawView: CardDrawSystem not found");
            return;
        }
        gameObject.SetActive(false);
        _isProcessing = false;
        Debug.Log("CardDrawView OnRunStarted");
    }

    private void OnOfferGenerated(OfferGeneratedEvent evt)
    {
        Debug.Log($"CardDrawView: OfferGenerated received, count={evt.Offer.Count}, maxSelectable={evt.MaxSelectable}");
        ShowOffer(evt.Offer, evt.MaxSelectable);
    }

    public void ShowOffer(List<ItemInstance> offer, int maxSelectable)
    {
        if (offer == null || offer.Count == 0)
        {
            Debug.LogWarning("CardDrawView: Offer is empty");
            return;
        }

        _maxSelectable = maxSelectable;
        _selectedCards.Clear();
        _isProcessing = false;

        gameObject.SetActive(true);
        _windowAnimator.SetTrigger("Open");

        CreateCards(offer);
        UpdateSelectionCounter();
        _confirmButton.interactable = false;
    }

    private void CreateCards(List<ItemInstance> offer)
    {
        // Удаляем старые карточки
        foreach (var card in _cardViews)
        {
            if (card != null) Destroy(card.gameObject);
        }
        _cardViews.Clear();

        // Создаём новые
        foreach (var item in offer)
        {
            CardView card = Instantiate(_cardPrefab, _cardsContainer);
            card.Setup(item);
            card.OnCardClick += OnCardClicked;
            _cardViews.Add(card);
        }

        // Расставляем карточки
        RearrangeCards();

        // Анимация появления с задержкой
        for (int i = 0; i < _cardViews.Count; i++)
        {
            var card = _cardViews[i];
            card.transform.localScale = Vector3.zero;
            card.transform.DOScale(Vector3.one, _cardAppearDuration)
                .SetDelay(i * _cardAppearDelay)
                .SetEase(Ease.OutBack, _cardBounceAmplitude);
        }
    }

    private void RearrangeCards()
    {
        int count = _cardViews.Count;
        if (count == 0) return;

        float totalWidth = (count - 1) * _cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector2 targetPos = new Vector2(startX + i * _cardSpacing, 0);
            _cardViews[i].GetComponent<RectTransform>().anchoredPosition = targetPos;
        }
    }

    private void OnCardClicked(CardView card)
    {
        if (_isProcessing) return;

        if (_selectedCards.Contains(card))
        {
            _selectedCards.Remove(card);
            card.Deselect();
            Debug.Log($"Card deselected, selected count: {_selectedCards.Count}");
        }
        else
        {
            if (_selectedCards.Count >= _maxSelectable)
            {
                // Отрицательная обратная связь – встряхиваем карточку
                card.transform.DOShakePosition(0.2f, 5f);
                return;
            }
            _selectedCards.Add(card);
            card.Select();
            Debug.Log($"Card selected, selected count: {_selectedCards.Count}");
        }

        UpdateSelectionCounter();
        _confirmButton.interactable = (_selectedCards.Count == _maxSelectable);
    }

    private void UpdateSelectionCounter()
    {
        if (_selectionCounterText != null)
            _selectionCounterText.text = $"{_selectedCards.Count}/{_maxSelectable}";
    }

    private void OnConfirmClicked()
    {
        if (_isProcessing) return;
        if (_selectedCards.Count != _maxSelectable) return;

        _isProcessing = true;

        List<ItemInstance> selectedItems = new List<ItemInstance>();
        foreach (var card in _selectedCards)
        {
            selectedItems.Add(card.Item);
        }

        CommandProcessor.Execute(new SelectCardsCommand { SelectedItems = selectedItems });

        _windowAnimator.SetTrigger("Close");
        DOVirtual.DelayedCall(0.5f, () =>
        {
            gameObject.SetActive(false);
            _isProcessing = false;
        });
    }

    private void OnSkipClicked()
    {
        if (_isProcessing) return;
        _windowAnimator.SetTrigger("Close");
        DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
    }

    public void ForceClose()
    {
        if (gameObject.activeSelf)
        {
            _windowAnimator.SetTrigger("Close");
            DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
        }
    }

}