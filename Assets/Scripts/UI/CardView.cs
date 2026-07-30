using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Infrastructure;
using Gameplay;
using Data;

public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _genomeWeightText;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _rarityFrame;
    [SerializeField] private Slider _genomeSlider;

    [Header("Animator")]
    [SerializeField] private Animator _cardAnimator;

    [Header("DOTween Settings")]
    [SerializeField] private float _appearDuration = 0.4f;
    [SerializeField] private float _hoverScale = 1.05f;
    [SerializeField] private float _dragScale = 1.1f;
    [SerializeField] private float _dragLift = 0.3f;   // относительный подъём (30% от высоты)
    [SerializeField] private float _returnDuration = 0.3f;

    private ItemInstance _item;
    private bool _isDragging;
    private RectTransform _rectTransform;
    private Vector2 _originalAnchoredPosition;
    private Vector3 _originalScale;
    private Tween _currentTween;
    private Canvas _canvas;

    public System.Action<CardView> OnCardClick;
    public System.Action<CardView> OnDragStart;
    public System.Action<CardView> OnDragEnd;
    public System.Action<CardView> OnDragCancel;

    public ItemInstance Item => _item;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = transform.localScale;
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null) Debug.LogError("CardView: Canvas not found!");
    }

    private void OnDestroy() => _currentTween?.Kill();

    public void Setup(ItemInstance item, bool animateAppear = true)
    {
        _item = item;
        if (_item == null) { gameObject.SetActive(false); return; }

        gameObject.SetActive(true);
        _titleText.text = item.Data.itemName;
        _descriptionText.text = item.Data.description;
        _iconImage.sprite = item.Data.icon;
        _rarityFrame.color = GetRarityColor(item.Data.rarity);

        if (item is PlantInstance plant)
        {
            int fillPercent = plant.GetGenomeFillPercent();
            _genomeSlider.gameObject.SetActive(true);
            _genomeSlider.value = fillPercent / 100f;
            _genomeWeightText.text = $"{plant.Genome.CurrentWeight}/{plant.Genome.MaxCapacity}";
            _quantityText.text = "";
        }
        else
        {
            _genomeSlider.gameObject.SetActive(false);
            _genomeWeightText.text = "";
            _quantityText.text = item.Quantity > 1 ? item.Quantity.ToString() : "";
        }

        _cardAnimator.Rebind();
        _cardAnimator.SetTrigger("Show");

        if (animateAppear)
        {
            transform.localScale = Vector3.zero;
            _currentTween = transform.DOScale(_originalScale, _appearDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            transform.localScale = _originalScale;
        }
    }

    // ----- Анимации состояний (Animator) -----
    public void Select() => _cardAnimator.SetTrigger("Select");
    public void Deselect() => _cardAnimator.SetTrigger("Deselect");
    public void Highlight() => _cardAnimator.SetTrigger("Highlight");
    public void Unhighlight() => _cardAnimator.SetTrigger("Unhighlight");
    public void Use() => _cardAnimator.SetTrigger("Use");
    public void Receive() => _cardAnimator.SetTrigger("Receive");
    public void Hide() => _cardAnimator.SetTrigger("Hide");
    public void CancelDrop() => _cardAnimator.SetTrigger("Cancel");

    // ----- Drag & Drop -----
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null) return;

        _currentTween?.Kill();
        DOTween.Kill(transform);
        DOTween.Kill(_rectTransform);

        _isDragging = true;
        _originalAnchoredPosition = _rectTransform.anchoredPosition;
        _originalScale = transform.localScale;

        // Поднимаем карточку (без изменения масштаба)
        float lift = _rectTransform.rect.height * _dragLift;
        _rectTransform.DOAnchorPosY(_originalAnchoredPosition.y + lift, 0.15f);
        _rectTransform.SetAsLastSibling();

        _cardAnimator.SetBool("IsDragging", true);
        OnDragStart?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || _canvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        float lift = _rectTransform.rect.height * _dragLift;
        _rectTransform.anchoredPosition = new Vector2(localPoint.x, localPoint.y + lift);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;
        _cardAnimator.SetBool("IsDragging", false);

        bool validDrop = CheckDropTarget(eventData);

        if (validDrop)
        {
            _cardAnimator.SetTrigger("Used");
            OnDragEnd?.Invoke(this);
        }
        else
        {
            // Возврат на место без прыжков
            _rectTransform.DOAnchorPos(_originalAnchoredPosition, _returnDuration)
                .SetEase(Ease.OutQuad);
            transform.DOScale(_originalScale, _returnDuration).SetEase(Ease.OutQuad);
            _cardAnimator.SetTrigger("Cancel");
            OnDragCancel?.Invoke(this);
        }
    }
    private bool CheckDropTarget(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        Debug.Log($"Raycast results count: {results.Count}");
        foreach (var result in results)
        {
            Debug.Log($"Hit: {result.gameObject.name}");
            if (result.gameObject.GetComponent<BoardCellView>() != null ||
            result.gameObject.GetComponent<LaboratorySlotView>() != null)
            {
                var handView = ServiceLocator.TryGet<HandView>(out var hv) ? hv : null;
                Debug.Log(handView + " - hand view");
                if (handView == null)
                {
                    handView = FindAnyObjectByType<HandView>();
                    ServiceLocator.Register(handView);
                }
                if (handView != null)
                {
                    handView.HandleDrop(this, result.gameObject);
                    return true;
                }
            }
        }
        return false;
    }

    // ----- События указателя -----
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            _cardAnimator.SetBool("IsHovered", true);
            transform.DOScale(_originalScale * _hoverScale, 0.15f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            _cardAnimator.SetBool("IsHovered", false);
            transform.DOScale(_originalScale, 0.15f);
        }
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging && !eventData.dragging)
        {
            Select();
            // Лёгкая анимация нажатия – просто уменьшение и возврат (без прыжка)
            transform.DOScale(_originalScale * 0.9f, 0.1f).OnComplete(() =>
            {
                if (!_isDragging) transform.DOScale(_originalScale, 0.1f);
            });
            OnCardClick?.Invoke(this);
        }
    }

    // ----- Вспомогательные -----
    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.white;
            case Rarity.Uncommon: return Color.green;
            case Rarity.Rare: return Color.blue;
            case Rarity.Epic: return Color.magenta;
            case Rarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }

    public void Clear()
    {
        _item = null;
        gameObject.SetActive(false);
        _currentTween?.Kill();
        DOTween.Kill(transform);
        DOTween.Kill(_rectTransform);
    }
}