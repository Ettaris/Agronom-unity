using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Data;

/// <summary>
/// Отдельная запись в журнале.
/// </summary>
public class JournalEntryView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _rarityText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private Image _rarityFrame;

    private CanvasGroup _canvasGroup;
    private bool _isHidden;

    public CanvasGroup CanvasGroup
    {
        get
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }

    public void Setup(GenomePropertyData property, int discoveryCount)
    {
        _iconImage.sprite = property.icon;
        _nameText.text = property.propertyName;
        _rarityText.text = property.rarity.ToString();
        _costText.text = "Cost: " + property.genomeCost;
        _countText.text = "×" + discoveryCount;
        _rarityFrame.color = GetRarityColor(property.rarity);

        _isHidden = false;
        gameObject.SetActive(true);
    }

    public void Hide(System.Action onComplete = null)
    {
        if (_isHidden) return;
        _isHidden = true;

        transform.DOScaleX(0, 0.2f).SetEase(Ease.InQuad);
        CanvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

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
}