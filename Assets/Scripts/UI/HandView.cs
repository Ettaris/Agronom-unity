using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using Managers;

public class HandView : MonoBehaviour, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform _cardsContainer; // родительский RectTransform для карточек
    [SerializeField] private GameObject _cardPrefab; // префаб CardView
    [SerializeField] private int _maxCards = 10; // максимальное количество карточек в руке

    [Header("Animator")]
    [SerializeField] private Animator _handAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _cardSpacing = 100f; // расстояние между карточками
    [SerializeField] private float _moveDuration = 0.3f;
    [SerializeField] private float _appearBounceAmplitude = 0.3f;

    private List<CardView> _cardViews = new List<CardView>();
    private RunData _runData;
    private Hand _hand;
    private bool _isSelectionMode; // режим выбора карточек (для CardDrawSystem)


    private void Awake()
    {
        // Подписываемся на события обновления руки
        EventBus.Subscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Subscribe<OfferGeneratedEvent>(OnOfferGenerated); // для режима выбора
        EventBus.Subscribe<ServicesInitializedEvent>(OnServicesInitialized);
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);

    }

    private void OnServicesInitialized(ServicesInitializedEvent evt)
    {

    }

    private void OnRunStarted(RunStartedEvent evt)
    {
        Debug.Log("HandView OnRunStarted");
        _runData = evt.RunData;
        if (_runData != null)
        {
            _hand = _runData.Hand;
            RefreshHand();
        }
        Debug.Log(_runData + " - HandView run data");
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Unsubscribe<OfferGeneratedEvent>(OnOfferGenerated);
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
    }

    // Обработчики событий
    private void OnHandUpdated(HandUpdatedEvent evt)
    {
        Debug.Log("HandView OnHandUpdated");
        RefreshHand();
    }

    private void OnOfferGenerated(OfferGeneratedEvent evt)
    {
        // Включаем режим выбора: отображаем предложение вместо руки
        _isSelectionMode = true;
        //RefreshHand(evt.Offer);
    }

    // Обновление руки (из Hand)
    private void RefreshHand()
    {
        if (_hand == null) return;
        RefreshHand(_hand.GetAll());
    }

    // Обновление с переданным списком предметов (для режима выбора)
    private void RefreshHand(IEnumerable<ItemInstance> items)
    {
        // Удаляем старые карточки
        foreach (var card in _cardViews)
        {
            card.Clear();
            Destroy(card.gameObject, 0.5f);
        }
        _cardViews.Clear();

        int index = 0;
        foreach (var item in items)
        {
            if (index >= _maxCards) break;
            CreateCard(item, index, false); // без анимации появления
            index++;
        }

        _handAnimator.SetTrigger("Show");
        RearrangeCards();
    }

    private void CreateCard(ItemInstance item, int index, bool animateAppear = true)
    {
        GameObject cardObj = Instantiate(_cardPrefab.gameObject, _cardsContainer);
        CardView cardView = cardObj.GetComponent<CardView>();
        cardView.Setup(item, animateAppear);
        cardView.OnCardClick += OnCardClicked;
        cardView.OnDragStart += OnCardDragStart;
        cardView.OnDragEnd += OnCardDragEnd;
        cardView.OnDragCancel += OnCardDragCancel;
        _cardViews.Add(cardView);
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
            // Плавное перемещение
            _cardViews[i].transform.DOLocalMove(targetPos, _moveDuration).SetEase(Ease.OutQuad);
        }
    }

    // Обработчики Drag&Drop
    private void OnCardDragStart(CardView card)
    {
        // При начале перетаскивания можно подсветить карточку или заблокировать взаимодействие
        _handAnimator.SetBool("Dragging", true);
    }

    private void OnCardDragEnd(CardView card)
    {
        // При завершении перетаскивания проверяем, куда упала карточка
        // Обработка дропа происходит в OnDrop (реализован ниже)
        _handAnimator.SetBool("Dragging", false);
    }

    private void OnCardDragCancel(CardView card)
    {
        // Просто сбрасываем состояние перетаскивания (анимация возврата уже проигрывается в CardView)
        _handAnimator.SetBool("Dragging", false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Если кто-то бросает объект на руку – можно добавить в руку, но пока игнорируем.
    }

    // Метод для дропа из руки на другие объекты (вызывается из CardView или через Raycast)
    public void HandleDrop(CardView card, GameObject target)
    {
        var item = card.Item;
        if (item == null) return;

        // Посадка на поле
        if (target.TryGetComponent<BoardCellView>(out var cellView))
        {
            if (item is PlantInstance plant)
            {
                CommandProcessor.Execute(new PlacePlantCommand { Plant = plant, X = cellView.X, Y = cellView.Y });
                // Карточка будет удалена через HandUpdatedEvent
            }
            return;
        }

        // Дроп в лабораторию (только если окно открыто)
        if (target.TryGetComponent<LaboratorySlotView>(out var slotView))
        {
            var labView = ServiceLocator.Get<LaboratoryView>();
            if (labView != null && labView.gameObject.activeInHierarchy)
            {
                bool placed = labView.OnItemDropped(item);
                if (placed)
                {
                    RemoveCard(card);
                }
                else
                {
                    card.CancelDrop(); // анимация возврата
                }
            }
            return;
        }

        // Если дроп на пустое место – карточка вернётся сама (в CardView)
    }

    public void RemoveCard(CardView card)
    {
        if (_cardViews.Contains(card))
        {
            _cardViews.Remove(card);
            card.Hide();
            Destroy(card.gameObject, 0.5f);
            RearrangeCards();
        }
    }

    // Вспомогательный метод для получения предмета по индексу (для UI)
    public ItemInstance GetItemAt(int index)
    {
        if (index < 0 || index >= _cardViews.Count) return null;
        return _cardViews[index].Item;
    }

    // Метод для очистки руки (например, при завершении забега)
    public void ClearHand()
    {
        foreach (var card in _cardViews)
        {
            card.Hide();
            Destroy(card.gameObject, 0.5f);
        }
        _cardViews.Clear();
    }

    private void OnCardClicked(CardView card)
    {
        if (card.Item != null)
        {
            // Публикуем событие о выборе карточки
            EventBus.Publish(new CardSelectedEvent { Item = card.Item });
            // Снимаем выделение с других карточек
            foreach (var c in _cardViews)
            {
                if (c != card) c.Deselect();
            }
        }
    }
}