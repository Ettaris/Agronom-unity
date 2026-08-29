using System.Collections.Generic;
using Data;
using Gameplay;
using Infrastructure;
using Systems;

namespace Gameplay.Calculation
{
    /// <summary>
    /// Неизменяемый контекст для расчета урожая.
    /// </summary>
    public class HarvestCalculationContext
    {
        public PlantInstance Plant { get; }
        public IGridBoard Board { get; }
        public bool IsPreview { get; }
        public IPropertyProvider PropertyProvider { get; }
        public IReadOnlyDictionary<PlantData, List<GenomePropertyData>> DiscoveredGenomes { get; }

        public HarvestCalculationContext(
            PlantInstance plant,
            IGridBoard board,
            bool isPreview = false,
            IPropertyProvider propertyProvider = null,
            IReadOnlyDictionary<PlantData, List<GenomePropertyData>> discoveredGenomes = null)
        {
            Plant = plant;
            Board = board;
            IsPreview = isPreview;
            PropertyProvider = propertyProvider ?? new ResolverPropertyProvider(ServiceLocator.Get<PropertyResolverSystem>());
            DiscoveredGenomes = discoveredGenomes ?? new Dictionary<PlantData, List<GenomePropertyData>>();
        }
    }
}