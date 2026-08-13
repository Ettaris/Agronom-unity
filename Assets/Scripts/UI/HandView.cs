using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using Infrastructure;
using Infrastructure.Events;
using Commands;
using Gameplay;
using Managers;

public class HandView : MonoBehaviour, IGameSystem
{
    [Header("UI References")]
    [SerializeField] private RectTransform _cardsContainer; // родительский RectTransform дл€ карточек
    [SerializeField] private GameObject _cardPrefab; // префаб CardView
    [SerializeField] private int _maxCards = 10; // максимальное количество карточек в руке

    [Header("Animator")]
    [SerializeField] private Animator _handAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _cardSpacing = 100f; // рассто€ние между карточками
    [SerializeField] private float _moveDuration = 0.3f;
    [SerializeField] private float _appearBounceAmplitude = 0.3f;

    private List<CardView> _cardViews = new List<CardView>();
    private Hand _hand;


    public void Initialize()
    {
        EventBus.Subscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Subscribe<OfferGeneratedEvent>(OnOfferGenerated);
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<HandUpdatedEvent>(OnHandUpdated);
        EventBus.Unsubscribe<OfferGeneratedEvent>(OnOfferGenerated);
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
    }

    private void OnRunStarted(RunStartedEvent evt)
    {
        _hand = evt.RunData.Hand;
        RefreshHand();
    }

    // ќбработчики событий
    private void OnHandUpdated(HandUpdatedEvent evt)
    {
        Debug.Log("HandView OnHandUpdated");
        RefreshHand();
    }

    private void OnOfferGenerated(OfferGeneratedEvent evt)
    {

    }

    private void RefreshHand()
    {
        if (_hand == null) return;
        RefreshHand(_hand.GetAll());
    }

    // ќбновление с переданным списком предметов (дл€ режима выбора)
    private void RefreshHand(IEnumerable<ItemInstance> items)
    {
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
            CreateCard(item, index, false);
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
            // ѕлавное перемещение
            _cardViews[i].transform.DOLocalMove(targetPos, _moveDuration).SetEase(Ease.OutQuad);
        }
    }

    // ќбработчики Drag&Drop
    private void OnCardDragStart(CardView card)
    {
        // ѕри начале перетаскивани€ можно подсветить карточку или заблокировать взаимодействие
        _handAnimator.SetBool("Dragging", true);
    }

    private void OnCardDragEnd(CardView card)
    {
        // ѕри завершении перетаскивани€ провер€ем, куда упала карточка
        // ќбработка дропа происходит в OnDrop (реализован ниже)
        _handAnimator.SetBool("Dragging", false);
    }

    private void OnCardDragCancel(CardView card)
    {
        // ѕросто сбрасываем состо€ние перетаскивани€ (анимаци€ возврата уже проигрываетс€ в CardView)
        _handAnimator.SetBool("Dragging", false);
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

    // ¬спомогательный метод дл€ получени€ предмета по индексу (дл€ UI)
    public ItemInstance GetItemAt(int index)
    {
        if (index < 0 || index >= _cardViews.Count) return null;
        return _cardViews[index].Item;
    }

    // ћетод дл€ очистки руки (например, при завершении забега)
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
            foreach (var c in _cardViews)
            {
                if (c != card) c.Deselect();
            }
        }
    }

}