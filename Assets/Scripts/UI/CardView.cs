using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using Infrastructure;
using Commands;
using Gameplay;
using Data;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Универсальная карточка предмета. Использует Animator для состояний и DOTween для процедурных анимаций.
/// </summary>
public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References (TextMeshPro)")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _genomeWeightText;
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private UnityEngine.UI.Image _iconImage;
    [SerializeField] private UnityEngine.UI.Image _rarityFrame;
    [SerializeField] private UnityEngine.UI.Slider _genomeSlider;

    [Header("Animator")]
    [SerializeField] private Animator _cardAnimator;

    [Header("DOTween Animation Settings")]
    [SerializeField] private float _appearDuration = 0.5f;
    [SerializeField] private float _bounceAmplitude = 0.3f;
    [SerializeField] private float _dragScale = 1.1f;
    [SerializeField] private float _dragHeightOffset = 50f;
    [SerializeField] private float _returnDuration = 0.3f;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private float _shakeStrength = 5f;

    private ItemInstance _item;
    private bool _isDragging;
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Tween _currentTween;
    private Coroutine _returnCoroutine;

    public ItemInstance Item => _item;

    public System.Action<CardView> OnDragStart;
    public System.Action<CardView> OnDragEnd;
    public System.Action<CardView> OnCardClick;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _originalPosition = transform.position;
        _isDragging = false;
    }

    private void OnDestroy()
    {
        _currentTween?.Kill();
    }

    /// <summary>
    /// Настройка карточки на основе предмета с анимацией появления (bounce).
    /// </summary>
    public void Setup(ItemInstance item)
    {
        _item = item;
        if (_item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        _titleText.text = item.Data.itemName;
        _descriptionText.text = item.Data.description;
        _iconImage.sprite = item.Data.icon;
        _rarityFrame.color = GetRarityColor(item.Data.rarity);

        // Настройка в зависимости от типа
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

        // Сброс анимаций
        _cardAnimator.Rebind();
        _cardAnimator.SetTrigger("Show");

        // Анимация появления (bounce)
        transform.localScale = Vector3.zero;
        _currentTween = transform.DOScale(_originalScale, _appearDuration)
            .SetEase(Ease.OutBack, _bounceAmplitude);
    }

    /// <summary>
    /// Установить количество (для стакаемых предметов).
    /// </summary>
    public void SetQuantity(int quantity)
    {
        _quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }

    // ------------------ Анимации состояний (Animator) ------------------

    public void Select() => _cardAnimator.SetTrigger("Select");
    public void Deselect() => _cardAnimator.SetTrigger("Deselect");
    public void Highlight() => _cardAnimator.SetTrigger("Highlight");
    public void Unhighlight() => _cardAnimator.SetTrigger("Unhighlight");
    public void Use() => _cardAnimator.SetTrigger("Use");
    public void Receive() => _cardAnimator.SetTrigger("Receive");
    public void Hide() => _cardAnimator.SetTrigger("Hide");

    // ------------------ Drag & Drop (DOTween) ------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null) return;
        _isDragging = true;
        _originalPosition = transform.position;
        _originalScale = transform.localScale;

        // Увеличиваем и поднимаем
        _currentTween = transform.DOScale(_originalScale * _dragScale, 0.2f);
        transform.DOJump(transform.position + new Vector3(0, _dragHeightOffset, 0), 0.5f, 1, 0.2f);

        _cardAnimator.SetBool("IsDragging", true);
        OnDragStart?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
        mousePos.z = 0;
        transform.position = mousePos + new Vector3(0, _dragHeightOffset, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;
        _cardAnimator.SetBool("IsDragging", false);

        bool validDrop = CheckDropTarget(eventData);

        if (validDrop)
        {
            // Анимация успешного использования (играть с эффектом)
            _cardAnimator.SetTrigger("Used");
            // Легкий bounce на месте
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
            OnDragEnd?.Invoke(this);
        }
        else
        {
            // Возврат с анимацией bounce + встряхивание при ошибке
            _currentTween = transform.DOMove(_originalPosition, _returnDuration)
                .SetEase(Ease.OutBack, _bounceAmplitude)
                .OnComplete(() => {
                    // Встряхивание (отрицательная обратная связь)
                    transform.DOShakePosition(_shakeDuration, _shakeStrength);
                    _cardAnimator.SetTrigger("Cancel");
                });
            transform.DOScale(_originalScale, _returnDuration).SetEase(Ease.OutBack);
        }
    }

    // ------------------ Обработка наведения (Animator + DOTween) ------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            _cardAnimator.SetBool("IsHovered", true);
            // Легкое увеличение при наведении (DOTween)
            transform.DOScale(_originalScale * 1.05f, 0.15f);
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

    public void OnPointerDown(PointerEventData eventData)
    {
        // При клике можно выделить
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging && !eventData.dragging)
        {
            // Выделение карточки (визуально)
            Select();
            // Анимация клика (пульс)
            transform.DOPunchScale(Vector3.one * 0.1f, 0.15f);
            // Вызываем событие клика
            OnCardClick?.Invoke(this);
        }
    }

    private IEnumerator ReturnToPosition(Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, target, smoothT);
            yield return null;
        }
        transform.position = target;
        _returnCoroutine = null;
    }

    // Также метод CancelDrop, который её вызывает:
    public void CancelDrop()
    {
        _isDragging = false;
        _cardAnimator.SetBool("IsDragging", false);
        if (_returnCoroutine != null) StopCoroutine(_returnCoroutine);
        _returnCoroutine = StartCoroutine(ReturnToPosition(_originalPosition, _returnDuration));
        _cardAnimator.SetTrigger("Cancel");
    }

    private bool CheckDropTarget(PointerEventData eventData)
    {
        // Выполняем Raycast
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var cellView = result.gameObject.GetComponent<BoardCellView>();
            if (cellView != null)
            {
                // Нашли клетку – сообщаем HandView
                var handView = ServiceLocator.Get<HandView>(); // нужна ссылка
                handView.HandleDrop(this, result.gameObject);
                return true;
            }
            // Можно также проверить на LaboratorySlotView и т.д.
        }
        return false;
    }

    // ------------------ Вспомогательные методы ------------------

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
    }
}