using Gameplay;

namespace Properties.Interfaces
{
    /// <summary>
    /// Интерфейс для свойств, которые влияют на сбор урожая соседних растений.
    /// </summary>
    public interface IOnNeighborHarvest
    {
        /// <summary>
        /// Модифицирует калории для растения-соседа при его сборе.
        /// </summary>
        /// <param name="neighbor">Растение-сосед, которое собирается</param>
        /// <param name="baseCalories">Базовые калории соседа</param>
        /// <returns>Модифицированные калории</returns>
        int ModifyNeighborHarvest(PlantInstance neighbor, int baseCalories);
    }
}