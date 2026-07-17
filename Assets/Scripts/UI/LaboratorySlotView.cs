using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Gameplay;

/// <summary>
/// Слот лаборатории для предмета.
/// </summary>
public class LaboratorySlotView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private GameObject _emptyIndicator;
    [SerializeField] private GameObject _occupiedIndicator;

    [Header("Animator")]
    [SerializeField] private Animator _slotAnimator;

    private ItemInstance _item;

    public bool IsEmpty => _item == null;
    public ItemInstance Item => _item;

    public void SetItem(ItemInstance item)
    {
        _item = item;
        if (item != null)
        {
            _iconImage.sprite = item.Data.icon;
            _nameText.text = item.Data.itemName;
            _emptyIndicator.SetActive(false);
            _occupiedIndicator.SetActive(true);
            _slotAnimator.SetTrigger("Occupied");
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        _item = null;
        _iconImage.sprite = null;
        _nameText.text = "";
        _emptyIndicator.SetActive(true);
        _occupiedIndicator.SetActive(false);
        _slotAnimator.SetTrigger("Empty");
    }

    public void Pulse()
    {
        transform.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutQuad)
            .OnComplete(() => transform.DOScale(Vector3.one, 0.2f));
        _slotAnimator.SetTrigger("Pulse");
    }
}