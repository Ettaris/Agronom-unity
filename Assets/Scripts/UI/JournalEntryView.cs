using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Data;
using Gameplay;

/// <summary>
/// Отдельная запись в журнале.
/// </summary>
public class JournalEntryView : MonoBehaviour
{
    [Header("Plant Info")]
    [SerializeField] private Image _plantIcon;
    [SerializeField] private TextMeshProUGUI _plantName;
    [SerializeField] private TextMeshProUGUI _baseCaloriesText;
    [SerializeField] private TextMeshProUGUI _growthTimeText;
    [SerializeField] private TextMeshProUGUI _discoveredCount;
    [SerializeField] private GameObject _permanentBadge;
    [SerializeField] private Image _rarityFrame;

    [SerializeField] private Transform _propertiesContainer;
    [SerializeField] private GameObject _genomeIconPrefab;

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

    public void Setup(JournalPlantEntry entry)
    {
        if (entry == null || entry.plantData == null) return;

        _plantIcon.sprite = entry.plantData.icon;
        _plantName.text = entry.plantData.itemName;
        _baseCaloriesText.text = "Calories: " + entry.plantData.baseCalories.ToString();
        _growthTimeText.text = "Growth Time: " + entry.plantData.growthTime.ToString();
        _discoveredCount.text = "x" + entry.discoveryCount.ToString();
        Debug.Log(entry.discoveryCount + " - disc count");

        foreach (Transform child in _propertiesContainer)
            Destroy(child.gameObject);

        foreach (var prop in entry.discoveredProperties)
        {
            GameObject go = Instantiate(_genomeIconPrefab, _propertiesContainer);
            var iconView = go.GetComponent<GenomeIconView>();
            iconView.Setup(prop);

            // Если это перманентное свойство, добавляем визуальную пометку (например, звёздочку). TODO: 
            if (entry.permanentProperty != null && prop == entry.permanentProperty)
            {
                // Добавляем бейдж (или меняем цвет рамки)
                var badge = go.transform.Find("PermanentBadge");
                if (badge != null) badge.gameObject.SetActive(true);
            }
        }

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
            transform.localScale = Vector3.one;
            CanvasGroup.alpha = 1f;
            onComplete?.Invoke();
        });
    }

    //TODO: create rarity realization.
    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.black;
            case Rarity.Uncommon: return Color.green;
            case Rarity.Rare: return Color.blue;
            case Rarity.Epic: return Color.magenta;
            case Rarity.Legendary: return Color.yellow;
            default: return Color.black;
        }
    }
}