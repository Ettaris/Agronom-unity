using Data;
using Gameplay;
using Infrastructure;
using Systems;

public static class PlantFactory
{
    public static PlantInstance CreatePlantWithProperties(PlantData plantData, SeedGenerator random, GameConfig config, RunData runData)
    {
        int maxCap = plantData.maxGenomeCapacity > 0 ? plantData.maxGenomeCapacity : config.defaultMaxGenomeCapacity;
        var plant = new PlantInstance(plantData, maxCap);

        GenomePropertyData permanentData = null;
        if (runData.PermanentModifiers != null && runData.PermanentModifiers.TryGetValue(plantData, out var perm))
            permanentData = perm;

        ModifierAssigner.AssignModifiers(plant, random, config.modifierConfig, config.genomeRarityPool, permanentData);

        var resolver = ServiceLocator.Get<PropertyResolverSystem>();
        resolver.RegisterPlant(plant);
        return plant;
    }

    public static PlantInstance ClonePlant(PlantInstance source, GameConfig config)
    {
        if (source == null) return null;

        int maxCap = source.PlantData.maxGenomeCapacity > 0 ? source.PlantData.maxGenomeCapacity : config.defaultMaxGenomeCapacity;
        var clone = new PlantInstance(source.PlantData, maxCap);

        foreach (var prop in source.Genome.Properties)
        {
            var newProp = prop.Data.CreateEffect(prop.Stacks);
            clone.AddGenomeProperty(newProp);
            if (source.PermanentModifier != null && source.PermanentModifier.Data == prop.Data)
            {
                clone.PermanentModifier = newProp;
            }
        }

        var resolver = ServiceLocator.Get<PropertyResolverSystem>();
        resolver.RegisterPlant(clone);

        return clone;
    }

    public static PlantInstance CreateWeed(PlantData weedData)
    {
        var weed = new PlantInstance(weedData, 0);
        var resolver = ServiceLocator.Get<PropertyResolverSystem>();
        resolver.RegisterPlant(weed);
        return weed;
    }
}