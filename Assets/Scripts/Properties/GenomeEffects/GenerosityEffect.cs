using Data;
using Gameplay;
using Infrastructure;
using Properties.Interfaces;
using Systems;
using UnityEngine;

namespace GenomeEffects
{
    public class GenerosityEffect : GenomeEffectBase, IOnNeighborHarvestCalculation
    {
        public GenerosityEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        public int CalculateNeighborHarvest(PlantInstance neighbor, int baseCalories, IGridBoard board)
        {
            // Получаем владельца свойства (растение снизу)
            var resolver = ServiceLocator.Get<PropertyResolverSystem>();
            var owner = resolver.GetOwner(this);
            if (owner == null || owner.CurrentCell == null) return baseCalories;

            // Проверяем, что neighbour находится СВЕРХУ от owner
            if (neighbor.CurrentCell != null &&
                neighbor.CurrentCell.X == owner.CurrentCell.X &&
                neighbor.CurrentCell.Y == owner.CurrentCell.Y - 1)
            {
                // Увеличиваем калории на 20%
                return Mathf.RoundToInt(baseCalories * 1.2f);
            }
            return baseCalories;
        }
    }
}