using System.Collections.Generic;
using Gameplay;
using Gameplay.Calculation;
using Infrastructure;
using Managers;
using Systems;
using UnityEngine;

public class PlacementPreviewSystem : IGameSystem, IRunAware
{
    private HarvestCalculator _calculator;
    private readonly Dictionary<(PlantInstance plant, int x, int y), HarvestResult> _cache = new();
    private PlantInstance _draggedPlant;
    private GridBoardOverlay _overlay;
    private PreviewPropertyProvider _propertyProvider;
    private PlantInstance _hypotheticalPlant;
    private RunData _runData;

    public void StartDrag(PlantInstance plant)
    {
        _draggedPlant = plant;
        _cache.Clear();

        if (_runData == null) return;

        _overlay = new GridBoardOverlay(_runData.Board);

        _propertyProvider = new PreviewPropertyProvider();

        var resolver = ServiceLocator.Get<PropertyResolverSystem>();
        foreach (var original in _runData.Board.GetAllPlants())
        {
            var clonePlant = _overlay.GetCell(original.CurrentCell.X, original.CurrentCell.Y)?.Plant;
            if (clonePlant != null)
                _propertyProvider.AddOverride(clonePlant, resolver.GetPlantProperties(original));
        }

        _hypotheticalPlant = new PlantInstance(plant.PlantData, plant.Genome.MaxCapacity);
        foreach (var prop in plant.Genome.Properties)
            _hypotheticalPlant.AddGenomeProperty(prop.Data.CreateEffect(prop.Stacks));
    }

    public HarvestResult GetPreview(int cellX, int cellY)
    {
        if (_draggedPlant == null || _overlay == null || _hypotheticalPlant == null)
            return null;

        if (!_runData.Board.CanPlace(new Vector2Int(cellX, cellY), Vector2Int.one))
            return null;

        var key = (_draggedPlant, cellX, cellY);
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        if (_hypotheticalPlant.Position != Vector2Int.zero)
        {
            _overlay.RemoveOverlayPlant(_hypotheticalPlant.Position);
            _propertyProvider.RemoveOverride(_hypotheticalPlant);
        }

        var pos = new Vector2Int(cellX, cellY);
        _overlay.AddOverlayPlant(_hypotheticalPlant, pos);
        _propertyProvider.AddOverride(_hypotheticalPlant, _hypotheticalPlant.Genome.Properties);

        var context = new HarvestCalculationContext(
        _hypotheticalPlant,
        _overlay,
        true,
        _propertyProvider,
        _runData.DiscoveredGenomes
    );
        var result = _calculator.Calculate(context);

        _cache[key] = result;
        return result;
    }

    public void EndDrag()
    {
        _draggedPlant = null;
        _overlay?.ClearOverlay();
        _overlay = null;
        _propertyProvider = null;
        _hypotheticalPlant = null;
        _cache.Clear();
    }

    public void Initialize()
    {
        _calculator = ServiceLocator.Get<HarvestCalculator>();
    }

    public void Dispose()
    {
        EndDrag();
    }

    public void OnRunDataSetup(RunData runData)
    {
        _runData = runData;
    }
}