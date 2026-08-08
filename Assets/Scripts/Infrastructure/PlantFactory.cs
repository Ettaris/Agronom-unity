// Infrastructure/PlantFactory.cs
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

        // Получаем перманентный модификатор для этого типа
        GenomePropertyData permanentData = null;
        if (runData.PermanentModifiers != null && runData.PermanentModifiers.TryGetValue(plantData, out var perm))
            permanentData = perm;

        // Назначаем модификаторы
        ModifierAssigner.AssignModifiers(plant, random, config.modifierConfig, config.genomeRarityPool, permanentData);

        var resolver = ServiceLocator.Get<PropertyResolverSystem>();
        resolver.RegisterPlant(plant);
        return plant;
    }

    public static PlantInstance CreateWeed(PlantData weedData)
    {
        var weed = new PlantInstance(weedData, 0); // ёмкость 0 – нет свойств
        var resolver = ServiceLocator.Get<PropertyResolverSystem>();
        resolver.RegisterPlant(weed);
        return weed;
    }
}