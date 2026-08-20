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
    [Header("Plant Mode")]
    [SerializeField] private Image _plantIcon;
    [SerializeField] private TMP_Text _plantName;
    [SerializeField] private Transform _propertiesContainer;
    [SerializeField] private GameObject _genomeIconPrefab;
    [SerializeField] private TMP_Text _plantCountText;

    [Header("Modifier Mode")]
    [SerializeField] private Image _modifierIcon;
    [SerializeField] private TMP_Text _modifierName;
    [SerializeField] private TMP_Text _modifierDescription;
    [SerializeField] private TMP_Text _modifierCost;
    [SerializeField] private GameObject _permanentBadge;

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

    public void Setup(IJournalEntryData data)
    {
        if (data == null) return;

        // Сброс состояния
        _plantIcon.gameObject.SetActive(false);
        _plantName.gameObject.SetActive(false);
        _propertiesContainer.gameObject.SetActive(false);
        _plantCountText.gameObject.SetActive(false);
        _modifierIcon.gameObject.SetActive(false);
        _modifierName.gameObject.SetActive(false);
        _modifierDescription.gameObject.SetActive(false);
        _modifierCost.gameObject.SetActive(false);
        _permanentBadge.SetActive(false);

        // Определяем тип записи
        if (data is JournalPlantEntryData plantData)
        {
            SetupPlant(plantData);
        }
        else if (data is JournalModifierEntryData modData)
        {
            SetupModifier(modData);
        }

        _isHidden = false;
        gameObject.SetActive(true);
    }

    private void SetupPlant(JournalPlantEntryData data)
    {
        _plantIcon.gameObject.SetActive(true);
        _plantName.gameObject.SetActive(true);
        _propertiesContainer.gameObject.SetActive(true);
        _plantCountText.gameObject.SetActive(true);

        _plantIcon.sprite = data.Icon;
        _plantName.text = data.Title;
        _plantCountText.text = $"×{data.Count}";

        // Очищаем старые иконки свойств
        foreach (Transform child in _propertiesContainer)
            Destroy(child.gameObject);

        // Добавляем иконки свойств
        foreach (var prop in data.Properties)
        {
            GameObject go = Instantiate(_genomeIconPrefab, _propertiesContainer);
            var icon = go.GetComponent<GenomeIconView>();
            if (icon != null) icon.Setup(prop);
        }
    }

    private void SetupModifier(JournalModifierEntryData data)
    {
        _modifierIcon.gameObject.SetActive(true);
        _modifierName.gameObject.SetActive(true);
        _modifierDescription.gameObject.SetActive(true);
        _modifierCost.gameObject.SetActive(true);
        if (data.IsPermanent)
            _permanentBadge.SetActive(true);

        _modifierIcon.sprite = data.Icon;
        _modifierName.text = data.Title;
        _modifierDescription.text = data.Description;
        _modifierCost.text = $"Стоимость: {data.Properties[0].genomeCost}";

        if (data.IsPermanent && !string.IsNullOrEmpty(data.PermanentFor))
        {
            // Можно добавить дополнительный текст: "Перманентный для: ..."
        }
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
}