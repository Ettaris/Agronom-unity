// GenomeEffects/SociabilityEffect.cs
using Gameplay;
using Properties.Interfaces;
using Infrastructure;
using UnityEngine;
using Data;
using Systems;
using Managers;

namespace GenomeEffects
{
    /// <summary>
    /// Напуганный: если слева/справа есть растение того же типа → +15% калорий себе.
    /// Если нет → все соседи (8 клеток) получают -15% калорий при сборе.
    /// </summary>
    public class ScaredEffect : GenomeEffectBase, IOnHarvest, IOnNeighborHarvest
    {
        public ScaredEffect(GenomePropertyData data, int stacks = 1) : base(data, stacks) { }

        // ---- IOnHarvest: применяется к самому растению при его сборе ----
        public int ModifyHarvest(PlantInstance plant, int baseCalories, GridBoard board)
        {
            if (plant == null || plant.CurrentCell == null) return baseCalories;

            bool hasSameTypeNeighbor = CheckHorizontalSameType(plant, board);
            if (hasSameTypeNeighbor)
            {
                // +15% к себе
                return Mathf.RoundToInt(baseCalories * 1.15f);
            }
            else
            {
                // Штраф не применяем к себе, оставляем базовые калории
                return baseCalories;
            }
        }

        // ---- IOnNeighborHarvest: применяется к соседям при их сборе ----
        public int ModifyNeighborHarvest(PlantInstance neighbor, int baseCalories)
        {
            // Получаем владельца свойства (растение, на котором висит этот эффект)
            var resolver = ServiceLocator.Get<PropertyResolverSystem>();
            var owner = resolver.GetOwner(this);
            if (owner == null || owner.CurrentCell == null) return baseCalories;

            // Проверяем, является ли neighbor соседом owner (по 8 клеткам)
            if (!IsNeighbor(owner, neighbor)) return baseCalories;

            // Проверяем, есть ли у owner сосед того же типа слева/справа
            var board = ServiceLocator.Get<RunManager>().CurrentRunData.Board;
            bool hasSameTypeNeighbor = CheckHorizontalSameType(owner, board);

            if (!hasSameTypeNeighbor)
            {
                // Штраф -15% к соседу
                return Mathf.RoundToInt(baseCalories * 0.85f);
            }
            else
            {
                // Условие выполнено – штраф не применяем
                return baseCalories;
            }
        }

        // ---- Вспомогательные методы ----
        private bool CheckHorizontalSameType(PlantInstance plant, GridBoard board)
        {
            if (plant == null || plant.CurrentCell == null) return false;
            int x = plant.CurrentCell.X;
            int y = plant.CurrentCell.Y;

            // Проверяем слева
            var leftCell = board.GetCell(x - 1, y);
            if (leftCell != null && leftCell.Plant != null && leftCell.Plant.PlantData == plant.PlantData)
                return true;

            // Проверяем справа
            var rightCell = board.GetCell(x + 1, y);
            if (rightCell != null && rightCell.Plant != null && rightCell.Plant.PlantData == plant.PlantData)
                return true;

            return false;
        }

        private bool IsNeighbor(PlantInstance owner, PlantInstance neighbor)
        {
            if (owner == null || neighbor == null || owner.CurrentCell == null || neighbor.CurrentCell == null)
                return false;

            int dx = Mathf.Abs(owner.CurrentCell.X - neighbor.CurrentCell.X);
            int dy = Mathf.Abs(owner.CurrentCell.Y - neighbor.CurrentCell.Y);
            return dx <= 1 && dy <= 1 && (dx + dy) > 0; // сосед в 8 клетках, но не сама клетка
        }
    }
}