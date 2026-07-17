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
    [SerializeField] private float _cardSpacing = 20f; // расстояние между карточками
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

        // Получаем RunData
        _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
        if (_runData != null)
        {
            _hand = _runData.Hand;
            RefreshHand();
        }
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Unsubscribe<OfferGeneratedEvent>(OnOfferGenerated);
    }

    // Обработчики событий
    private void OnHandUpdated(HandUpdatedEvent evt)
    {
        RefreshHand();
    }

    private void OnOfferGenerated(OfferGeneratedEvent evt)
    {
        // Включаем режим выбора: отображаем предложение вместо руки
        _isSelectionMode = true;
        RefreshHand(evt.Offer);
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
        // Удаляем все существующие карточки с анимацией исчезновения
        foreach (var card in _cardViews)
        {
            card.Hide();
            // После анимации можно вернуть в пул, но пока просто отключаем
            Destroy(card.gameObject, 0.5f);
        }
        _cardViews.Clear();

        // Создаём новые карточки
        int index = 0;
        foreach (var item in items)
        {
            if (index >= _maxCards) break; // ограничение
            CreateCard(item, index);
            index++;
        }

        // Анимация панели (если скрыта)
        _handAnimator.SetTrigger("Show");

        // Перестроение позиций
        RearrangeCards();
    }

    private void CreateCard(ItemInstance item, int index)
    {
        GameObject cardObj = Instantiate(_cardPrefab, _cardsContainer);
        CardView cardView = cardObj.GetComponent<CardView>();
        cardView.Setup(item);
        // Подписываемся на события перетаскивания
        cardView.OnDragStart = OnCardDragStart;
        cardView.OnDragEnd = OnCardDragEnd;
        cardView.OnCardClick += OnCardClicked;
        // Начальная позиция (с анимацией появления)
        cardObj.transform.localScale = Vector3.zero;
        cardObj.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack, _appearBounceAmplitude);
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

    // Реализация IDropHandler для приёма объектов на зону руки (не используется для дропа из руки, но может пригодиться)
    public void OnDrop(PointerEventData eventData)
    {
        // Если кто-то бросает объект на руку – можно добавить в руку, но пока игнорируем.
    }

    // Метод для дропа из руки на другие объекты (вызывается из CardView или через Raycast)
    public void HandleDrop(CardView card, GameObject target)
    {
        var item = card.Item;
        if (item == null) return;

        // Проверяем дроп на игровое поле
        if (target.TryGetComponent<BoardCellView>(out var cellView))
        {
            if (item is PlantInstance plant)
            {
                CommandProcessor.Execute(new PlacePlantCommand
                {
                    Plant = plant,
                    X = cellView.X,
                    Y = cellView.Y
                });
                
            }
            return;
        }

        // Проверяем дроп на лабораторию (любой слот)
        if (target.TryGetComponent<LaboratorySlotView>(out var slotView))
        {
            // Находим LaboratoryView через ServiceLocator
            var labView = ServiceLocator.Get<LaboratoryView>();
            if (labView != null)
            {
                bool placed = labView.OnItemDropped(item);
                if (placed)
                {
                    // Удаляем карточку из руки
                    RemoveCard(card);
                }
                else
                {
                    // Если не удалось разместить – возвращаем карточку (отрицательная анимация в CardView)
                    card.CancelDrop();
                }
            }
            return;
        }

        // Если дроп на пустое место – ничего не делаем (карточка вернётся сама)
    }

    private void RemoveCard(CardView card)
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
            EventBus.Publish(new CardSelectedEvent { Item = card.Item });
            // Также можно выделить карточку визуально (вызов card.Select())
        }
    }
}