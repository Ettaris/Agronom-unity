// Infrastructure/PlantFactory.cs
using Data;
using Gameplay;
using Infrastructure;
using Systems;

public static class PlantFactory
{
    public static PlantInstance CreatePlantWithProperties(PlantData plantData, SeedGenerator random, GenomePool genomePool, int maxPropertiesPerPlant)
    {
        int maxCap = plantData.maxGenomeCapacity > 0 ? plantData.maxGenomeCapacity : maxPropertiesPerPlant;
        var plant = new PlantInstance(plantData, maxCap);
        PlantGeneratorHelper.AssignRandomProperties(plant, random, genomePool, maxPropertiesPerPlant);

        var resolver = ServiceLocator.Get<PropertyResolverSystem>();
        resolver.RegisterPlant(plant);
        return plant;
    }

    public static PlantInstance CreateWeed(PlantData weedData)
    {
        var weed = new PlantInstance(weedData, 0); // Ємкость 0 Ц нет свойств
        var resolver = ServiceLocator.Get<PropertyResolverSystem>();
        resolver.RegisterPlant(weed);
        return weed;
    }
}