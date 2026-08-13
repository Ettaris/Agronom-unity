using UnityEngine;
using TMPro;
using DG.Tweening;
using Gameplay;
using System.Collections.Generic;

/// <summary>
/// Отображение информации о растении (название, рост, геном, свойства). Используется в анализаторе и центрифуге.
/// </summary>
public class PlantInfoView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _plantName;
    [SerializeField] private TMP_Text _growthTimeText;
    [SerializeField] private TMP_Text _genomeWeightText;
    [SerializeField] private TMP_Text _baseCaloriesText;
    [SerializeField] private TMP_Text _modifiedCaloriesText;
    [SerializeField] private Transform _propertiesContainer;
    [SerializeField] private GameObject _genomeIconPrefab; 

    [Header("Animator")]
    [SerializeField] private Animator _infoAnimator;

    private List<GenomeIconView> _propertyIcons = new List<GenomeIconView>();

    public void ShowPlant(PlantInstance plant)
    {
        gameObject.SetActive(true);

        Debug.Log(plant);
        Debug.Log(plant.PlantData.itemName);
        if (plant == null)
        {
            Debug.LogError("Plant = null in PlantInfoView");
            Clear();
            return;
        }

        _plantName.text = plant.PlantData.itemName;
        _growthTimeText.text = plant.PlantData.growthTime.ToString();
        _genomeWeightText.text = $"{plant.Genome.CurrentWeight}/{plant.Genome.MaxCapacity}";

        foreach (var icon in _propertyIcons)
            Destroy(icon.gameObject);
        _propertyIcons.Clear();

        foreach (var prop in plant.Genome.Properties)
        {
            GameObject go = Instantiate(_genomeIconPrefab, _propertiesContainer);
            var icon = go.GetComponent<GenomeIconView>();
            icon.Setup(prop.Data);
            _propertyIcons.Add(icon);
        }

        _infoAnimator.SetTrigger("Show");

        Invoke(nameof(Clear), 5f);
    }

    public void ShowPlantName(PlantInstance plant)
    {
        gameObject.SetActive(true);
        if (plant == null)
        {
            Clear();
            return;
        }
        _plantName.text = plant.PlantData.itemName;
    }

    public void Clear()
    {
        //TODO: анимация потери энергии для показа инфы. Типа бррр и отключение.
        _plantName.text = "";
        _growthTimeText.text = "";
        _baseCaloriesText.text = "";
        _modifiedCaloriesText.text = "";
        _genomeWeightText.text = "";
        foreach (var icon in _propertyIcons)
            Destroy(icon.gameObject);
        _propertyIcons.Clear();
        _infoAnimator.SetTrigger("Hide");
        TooltipManager.Hide();
    }
}