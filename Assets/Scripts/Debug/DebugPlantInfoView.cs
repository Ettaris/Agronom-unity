using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay;
using Data;
using Infrastructure;
using Systems;
using GenomeEffects;

public class DebugPlantInfoView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _plantNameText;
    [SerializeField] private TMP_Text _plantRarityText;
    [SerializeField] private TMP_Text _growthText;
    [SerializeField] private TMP_Text _genomeWeightText;
    [SerializeField] private TMP_Text _baseCaloriesText;
    [SerializeField] private TMP_Text _modifiedCaloriesText;
    [SerializeField] private Transform _propertiesContainer;
    [SerializeField] private GameObject _propertyEntryPrefab;

    [Header("Buttons")]
    [SerializeField] private Button _closeButton;

    private PlantInstance _currentPlant;
    private List<GameObject> _propertyEntries = new List<GameObject>();

    private void Awake()
    {
        _closeButton.onClick.AddListener(Close);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveListener(Close);
    }

    public void ShowInfo(PlantInstance plant)
    {
        if (plant == null) return;

        _currentPlant = plant;
        gameObject.SetActive(true);

        // Основные данные
        _plantNameText.text = plant.PlantData.itemName;
        _plantRarityText.text = plant.PlantData.rarity.ToString();
        _growthText.text = $"Рост: {Mathf.RoundToInt(plant.GrowthProgress * 100)}%";
        _genomeWeightText.text = $"Геном: {plant.Genome.CurrentWeight}/{plant.Genome.MaxCapacity} ({plant.GetGenomeFillPercent()}%)";
        _baseCaloriesText.text = $"Базовые калории: {plant.PlantData.baseCalories}";

        // Модифицированные калории через PropertyResolverSystem
        int modified = 0;
        var resolver = ServiceLocator.TryGet<PropertyResolverSystem>(out var pr) ? pr : null;
        if (resolver != null)
        {
            modified = resolver.ModifyHarvest(plant, plant.PlantData.baseCalories);
        }
        _modifiedCaloriesText.text = $"Модифицированные калории: {modified}";

        // Свойства
        ClearProperties();
        foreach (var prop in plant.Genome.Properties)
        {
            GameObject entry = Instantiate(_propertyEntryPrefab, _propertiesContainer);
            var label = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                string effectInfo = prop.Data.propertyName;
                if (prop is GenomeEffectBase effect)
                {
                    // Можно добавить дополнительную информацию об эффекте
                    effectInfo += $" (Cost: {prop.Data.genomeCost})";
                }
                label.text = effectInfo;
            }
            _propertyEntries.Add(entry);
        }
    }

    private void ClearProperties()
    {
        foreach (var entry in _propertyEntries)
            Destroy(entry);
        _propertyEntries.Clear();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _currentPlant = null;
        ClearProperties();
    }

    // Метод для обновления информации в реальном времени (можно вызывать по таймеру или событиям)
    public void Refresh()
    {
        if (_currentPlant != null)
            ShowInfo(_currentPlant);
    }
}