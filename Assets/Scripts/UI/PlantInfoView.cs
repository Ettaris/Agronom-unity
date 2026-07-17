using UnityEngine;
using TMPro;
using DG.Tweening;
using Gameplay;

/// <summary>
/// Отображение информации о растении (название, рост, геном, свойства).
/// </summary>
public class PlantInfoView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _plantName;
    [SerializeField] private TMP_Text _growthText;
    [SerializeField] private UnityEngine.UI.Slider _genomeSlider;
    [SerializeField] private TMP_Text _genomeWeightText;
    [SerializeField] private Transform _propertiesContainer;
    [SerializeField] private GameObject _propertyPrefab;

    [Header("Animator")]
    [SerializeField] private Animator _infoAnimator;

    private PlantInstance _currentPlant;

    public void ShowPlant(PlantInstance plant)
    {
        _currentPlant = plant;
        if (plant == null)
        {
            Clear();
            return;
        }

        _plantName.text = plant.PlantData.itemName;
        int growthPercent = Mathf.RoundToInt(plant.GrowthProgress * 100f);
        _growthText.text = growthPercent + "%";
        _genomeSlider.value = plant.GetGenomeFillPercent() / 100f;
        _genomeWeightText.text = $"{plant.Genome.CurrentWeight}/{plant.Genome.MaxCapacity}";

        // Очищаем старые свойства
        foreach (Transform child in _propertiesContainer)
            Destroy(child.gameObject);

        // Отображаем свойства
        foreach (var prop in plant.Genome.Properties)
        {
            GameObject go = Instantiate(_propertyPrefab, _propertiesContainer);
            TMP_Text txt = go.GetComponent<TMP_Text>();
            if (txt != null) txt.text = prop.Data.propertyName;
        }

        _infoAnimator.SetTrigger("Show");
    }

    public void Clear()
    {
        _currentPlant = null;
        _plantName.text = "";
        _growthText.text = "";
        _genomeSlider.value = 0;
        _genomeWeightText.text = "";
        foreach (Transform child in _propertiesContainer)
            Destroy(child.gameObject);
        _infoAnimator.SetTrigger("Hide");
    }
}