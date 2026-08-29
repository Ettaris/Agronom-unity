using Gameplay;
using System.Collections.Generic;

public class PreviewPropertyProvider : IPropertyProvider
{
    private readonly Dictionary<PlantInstance, List<GenomePropertyInstance>> _overrides = new();
    public void AddOverride(PlantInstance plant, List<GenomePropertyInstance> props) => _overrides[plant] = props;
    public void RemoveOverride(PlantInstance plant) => _overrides.Remove(plant);
    public List<GenomePropertyInstance> GetProperties(PlantInstance plant) =>
        _overrides.TryGetValue(plant, out var props) ? props : new List<GenomePropertyInstance>();
}