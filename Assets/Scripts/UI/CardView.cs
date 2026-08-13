using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Infrastructure;
using Commands;
using System.Collections.Generic;
using System.Collections;
using Data;
using Gameplay;
using Infrastructure.Events;

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
    [SerializeField] private float _dragLift = 0.3f;
    [SerializeField] private float _returnDuration = 0.3f;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _shakeStrength = 5f;

    private ItemInstance _item;
    private bool _isDragging;
    private RectTransform _rectTransform;
    private Vector2 _originalAnchoredPosition;
    private Vector3 _originalScale;
    private Tween _currentTween;
    private Canvas _canvas;
    private Coroutine _returnCoroutine;
    private BoardView _boardView;
    private bool _servicesReady;

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

    private void Start()
    {
        _servicesReady = true;
        _boardView = ServiceLocator.TryGet<BoardView>(out var bv) ? bv : null;
        if (_boardView == null)
            Debug.LogWarning("CardView: BoardView not found!");
    }


    private void OnDestroy()
    {
        _currentTween?.Kill();
    }

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

        transform.localScale = Vector3.zero;
        if (animateAppear)
        {
            _currentTween = transform.DOScale(_originalScale, _appearDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            transform.localScale = _originalScale;
        }
    }

    // ----- Анимации состояний -----
    public void Select()
    {
        KillCurrentTween();
        transform.localScale = _originalScale;
        _currentTween = transform.DOScale(_originalScale * 1.1f, 0.15f).SetEase(Ease.OutQuad);
        _cardAnimator.SetTrigger("Select");
    }

    public void Deselect()
    {
        KillCurrentTween();
        _currentTween = transform.DOScale(_originalScale, 0.15f).SetEase(Ease.OutQuad);
        _cardAnimator.SetTrigger("Deselect");
    }

    public void Highlight()
    {
        if (!_isDragging)
        {
            KillCurrentTween();
            transform.localScale = _originalScale;
            _currentTween = transform.DOScale(_originalScale * _hoverScale, 0.15f).SetEase(Ease.OutQuad);
        }
        _cardAnimator.SetBool("IsHovered", true);
    }

    public void Unhighlight()
    {
        if (!_isDragging)
        {
            KillCurrentTween();
            transform.localScale = _originalScale * _hoverScale;
            _currentTween = transform.DOScale(_originalScale, 0.15f).SetEase(Ease.OutQuad);
        }
        _cardAnimator.SetBool("IsHovered", false);
    }

    public void Use()
    {
        _cardAnimator.SetTrigger("Use");
    }

    public void Receive()
    {
        _cardAnimator.SetTrigger("Receive");
    }

    public void Hide()
    {
        _cardAnimator.SetTrigger("Hide");
    }

    public void CancelDrop()
    {
        _cardAnimator.SetTrigger("Cancel");
        if (_returnCoroutine != null) StopCoroutine(_returnCoroutine);
        _returnCoroutine = StartCoroutine(ReturnToPosition(_originalAnchoredPosition, _returnDuration));
    }

    private void KillCurrentTween()
    {
        _currentTween?.Kill();
        _currentTween = null;
        DOTween.Kill(transform);
        DOTween.Kill(_rectTransform);
    }

    // ----- Drag & Drop -----
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null) return;

        KillCurrentTween();
        DOTween.Kill(transform);
        DOTween.Kill(_rectTransform);

        _isDragging = true;
        _originalAnchoredPosition = _rectTransform.anchoredPosition;
        _originalScale = transform.localScale;

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

        // Обновляем превью на поле
        if (_servicesReady && _item is PlantInstance plant && _boardView != null)
        {
            _boardView.UpdatePreviewFromScreen(eventData.position, plant.PlantData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;
        _cardAnimator.SetBool("IsDragging", false);

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                continue;

            EventBus.Publish(new CardDropEvent { Card = this, Target = result.gameObject });
            break;
        }
    }

    private IEnumerator ReturnToPosition(Vector2 target, float duration)
    {
        Vector2 startPos = _rectTransform.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            _rectTransform.anchoredPosition = Vector2.Lerp(startPos, target, smoothT);
            yield return null;
        }
        _rectTransform.anchoredPosition = target;
        _returnCoroutine = null;
    }

    // ----- События указателя -----
    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Unhighlight();
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging && !eventData.dragging)
        {
            Select();
            transform.DOPunchScale(Vector3.one * 0.1f, 0.15f);
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
        KillCurrentTween();
        DOTween.Kill(transform);
        DOTween.Kill(_rectTransform);
        transform.localScale = _originalScale;
        _rectTransform.anchoredPosition = _originalAnchoredPosition;
    }
}