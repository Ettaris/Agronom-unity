using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public interface IGridBoard
    {
        int Width { get; }
        int Height { get; }

        Cell GetCell(int x, int y);
        bool IsFree(int x, int y);
        bool CanPlace(Vector2Int position, Vector2Int size);
        List<Cell> GetNeighbors(int x, int y, bool includeDiagonals = false);
        List<PlantInstance> GetAllPlants();
        void Clear();
    }
}