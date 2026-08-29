using Gameplay;
using System.Collections.Generic;
using Systems;

public class ResolverPropertyProvider : IPropertyProvider
{
    private readonly PropertyResolverSystem _resolver;
    public ResolverPropertyProvider(PropertyResolverSystem resolver) => _resolver = resolver;
    public List<GenomePropertyInstance> GetProperties(PlantInstance plant) => _resolver.GetPlantProperties(plant);
}