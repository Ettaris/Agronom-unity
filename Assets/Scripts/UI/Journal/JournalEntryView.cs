using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Data;

public class JournalEntryView : MonoBehaviour
{
    [Header("Plant Mode")]
    [SerializeField] private Image _plantIcon;
    [SerializeField] private TMP_Text _plantName;
    [SerializeField] private TMP_Text _plantDescription; // калории и рост

    [Header("Modifier Mode")]
    [SerializeField] private Image _modifierIcon;
    [SerializeField] private TMP_Text _modifierName;
    [SerializeField] private TMP_Text _modifierDescription;
    //[SerializeField] private TMP_Text _modifierCost;
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

        _plantIcon.gameObject.SetActive(false);
        _plantName.gameObject.SetActive(false);
        _plantDescription.gameObject.SetActive(false);
        _modifierIcon.gameObject.SetActive(false);
        _modifierName.gameObject.SetActive(false);
        _modifierDescription.gameObject.SetActive(false);
        //_modifierCost.gameObject.SetActive(false);
        _permanentBadge.SetActive(false);

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
        _plantDescription.gameObject.SetActive(true);

        _plantIcon.sprite = data.Icon;
        _plantName.text = data.Title;
        _plantDescription.text = data.Description;
    }

    private void SetupModifier(JournalModifierEntryData data)
    {
        _modifierIcon.gameObject.SetActive(true);
        _modifierName.gameObject.SetActive(true);
        _modifierDescription.gameObject.SetActive(true);
        //_modifierCost.gameObject.SetActive(true);
        if (data.IsPermanent)
            _permanentBadge.SetActive(true);

        _modifierIcon.sprite = data.Icon;
        _modifierName.text = data.Title;
        _modifierDescription.text = data.Description;
        //_modifierCost.text = $"Стоимость: {data.Properties[0]?.genomeCost ?? 0}";
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