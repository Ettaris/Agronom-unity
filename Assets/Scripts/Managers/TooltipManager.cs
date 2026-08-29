using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Data;
using Gameplay;
using Systems;
using Infrastructure;
using Managers;

public class TooltipManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("GenomeTooltip")]
    [SerializeField] private GameObject _genomeTooltipObject;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Image _iconImage;

    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 0.15f;
    [SerializeField] private Vector2 _showGenomeOffset = new Vector2(15, -15);
    [SerializeField] private Vector2 _showCardOffset = new Vector2(15, -15);

    private static TooltipManager _instance;
    private RunData _runData;
    private bool _isVisible;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _isVisible = false;
    }

    public static void Show(GenomePropertyData data, Vector2 position)
    {
        if (_instance == null) return;
        _instance.ShowInternal(data, position);
    }

    public static void ShowCardTooltip(PlantInstance plant, Vector2 position)
    {
        if (_instance == null) return;
        _instance.ShowCardTooltipInternal(plant, position);
    }

    public static void Hide()
    {
        if (_instance == null) return;
        _instance.HideInternal();
    }

    public static void UpdatePosition(Vector2 position)
    {
        if (_instance == null || !_instance._isVisible) return;
        Vector2 finalPos = position + _instance._showGenomeOffset;
        RectTransform rect = _instance.GetComponent<RectTransform>();
        Vector2 size = rect.rect.size;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        finalPos.x = Mathf.Clamp(finalPos.x, size.x / 2, screenSize.x - size.x / 2);
        finalPos.y = Mathf.Clamp(finalPos.y, size.y / 2, screenSize.y - size.y / 2);
        rect.position = finalPos;
    }

    // ---------- Приватные методы ----------

    private void ShowInternal(GenomePropertyData data, Vector2 position)
    {
        if (data == null) { HideInternal(); return; }

        _titleText.text = data.propertyName;
        _descriptionText.text = data.description;
        _costText.text = $"Цена: {data.genomeCost}";
        if (_iconImage != null) _iconImage.sprite = data.icon;

        SetPosition(position, _showGenomeOffset);
        FadeIn();
    }

    private void ShowCardTooltipInternal(PlantInstance plant, Vector2 position)
    {
        if (plant == null) return;

        if (_runData == null) _runData = ServiceLocator.Get<RunManager>().CurrentRunData;

        bool isStudied = _runData.Journal.IsPlantStudied(plant.PlantData);
        Debug.Log($"{_runData.Journal} - journal + isStudied: {isStudied}, plant - {plant.PlantData}");

        string tooltipText = "";

        // Информация о растении
        if (isStudied)
        {
            tooltipText += $"Калории: {plant.PlantData.baseCalories}\n";
            tooltipText += $"Рост: {plant.PlantData.growthTime} дн.\n";
        }
        else
        {
            tooltipText += "???\n";
            tooltipText += "???\n";
        }

        // Геномы
        if (_runData != null && _runData.DiscoveredGenomes.TryGetValue(plant.PlantData, out var discovered))
        {
            foreach (var prop in plant.Genome.Properties)
            {
                bool isKnown = discovered.Contains(prop.Data);
                if (isKnown)
                {
                    tooltipText += $"{prop.Data.propertyName}: {prop.Data.description}\n";
                }
                else
                {
                    tooltipText += "???\n";
                }
            }
        }
        else
        {
            foreach (var prop in plant.Genome.Properties)
            {
                tooltipText += "???\n";
            }
        }

        tooltipText = tooltipText.TrimEnd('\n');

        _titleText.text = plant.PlantData.itemName;
        _descriptionText.text = tooltipText;
        _costText.text = "";
        if (_iconImage != null) _iconImage.sprite = plant.PlantData.icon;

        SetPosition(position, _showCardOffset);
        FadeIn();
    }

    private void SetPosition(Vector2 position, Vector2 offset)
    {
        Vector2 finalPos = position + offset;
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 size = rect.rect.size;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        finalPos.x = Mathf.Clamp(finalPos.x, size.x / 2, screenSize.x - size.x / 2);
        finalPos.y = Mathf.Clamp(finalPos.y, size.y / 2, screenSize.y - size.y / 2);
        rect.position = finalPos;
    }

    private void FadeIn()
    {
        if (!_isVisible)
        {
            _isVisible = true;
            _canvasGroup.DOFade(1, _fadeDuration);
        }
        else
        {
            _canvasGroup.alpha = 1;
        }
    }

    private void HideInternal()
    {
        if (!_isVisible) return;
        _isVisible = false;
        _canvasGroup.DOFade(0, _fadeDuration).OnComplete(() =>
        {
            _canvasGroup.alpha = 0;
        });
    }
}