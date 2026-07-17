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

/// <summary>
/// Окно ежедневного выбора карточек.
/// </summary>
public class CardDrawView : MonoBehaviour
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
    [SerializeField] private float _cardAppearDelay = 0.1f;
    [SerializeField] private float _cardAppearDuration = 0.4f;
    [SerializeField] private float _cardBounceAmplitude = 0.2f;

    private RunData _runData;
    private CardDrawSystem _cardDrawSystem;
    private List<CardView> _cardViews = new List<CardView>();
    private List<CardView> _selectedCards = new List<CardView>();
    private int _maxSelectable;
    private bool _isProcessing;

    private void Awake()
    {
        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
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

        _confirmButton.onClick.AddListener(OnConfirmClicked);
        if (_skipButton != null) _skipButton.onClick.AddListener(OnSkipClicked);

        EventBus.Subscribe<OfferGeneratedEvent>(OnOfferGenerated);

        gameObject.SetActive(false);
        _isProcessing = false;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OfferGeneratedEvent>(OnOfferGenerated);
        _confirmButton.onClick.RemoveAllListeners();
        if (_skipButton != null) _skipButton.onClick.RemoveAllListeners();
    }

    private void OnOfferGenerated(OfferGeneratedEvent evt)
    {
        // Получаем конфиг
        var config = ServiceLocator.Get<GameConfig>();
        _maxSelectable = config.cardsToSelect; // или из evt, если там передаётся

        ShowOffer(evt.Offer, _maxSelectable);
    }

    /// <summary>
    /// Показывает окно с предложением.
    /// </summary>
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

        // Активируем окно с анимацией
        gameObject.SetActive(true);
        _windowAnimator.SetTrigger("Open");

        // Создаём карточки
        CreateCards(offer);

        // Обновляем счётчик
        UpdateSelectionCounter();

        // Кнопка подтверждения неактивна, пока не выбрано достаточно
        _confirmButton.interactable = false;
    }

    private void CreateCards(List<ItemInstance> offer)
    {
        // Удаляем старые карточки (если есть)
        foreach (var card in _cardViews)
        {
            if (card != null && card.gameObject != null)
                Destroy(card.gameObject);
        }
        _cardViews.Clear();

        int index = 0;
        foreach (var item in offer)
        {
            CardView card = Instantiate(_cardPrefab, _cardsContainer);
            card.Setup(item);

            // Начальное состояние для анимации
            card.transform.localScale = Vector3.zero;
            // Добавляем обработчик клика для выбора/отмены выбора
            card.OnCardClick += OnCardClicked;

            // Анимация появления с задержкой
            card.transform.DOScale(Vector3.one, _cardAppearDuration)
                .SetDelay(index * _cardAppearDelay)
                .SetEase(Ease.OutBack, _cardBounceAmplitude);

            _cardViews.Add(card);
            index++;
        }
    }

    private void OnCardClicked(CardView card)
    {
        if (_isProcessing) return;

        // Если карточка уже выбрана — снимаем выбор
        if (_selectedCards.Contains(card))
        {
            _selectedCards.Remove(card);
            card.Deselect();
        }
        else
        {
            // Если достигнут лимит — нельзя выбрать больше
            if (_selectedCards.Count >= _maxSelectable)
            {
                // Отрицательная обратная связь (встряхивание)
                card.transform.DOShakePosition(0.2f, 5f);
                return;
            }
            _selectedCards.Add(card);
            card.Select();
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

        // Собираем выбранные предметы
        List<ItemInstance> selectedItems = new List<ItemInstance>();
        foreach (var card in _selectedCards)
        {
            selectedItems.Add(card.Item);
        }

        // Отправляем команду выбора
        CommandProcessor.Execute(new SelectCardsCommand { SelectedItems = selectedItems });

        // Анимация закрытия
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

        // Если пропуск разрешён — просто закрываем без выбора
        _windowAnimator.SetTrigger("Close");
        DOVirtual.DelayedCall(0.5f, () =>
        {
            gameObject.SetActive(false);
        });
    }

    // Метод для принудительного закрытия (например, если рука полна)
    public void ForceClose()
    {
        if (gameObject.activeSelf)
        {
            _windowAnimator.SetTrigger("Close");
            DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
        }
    }
}