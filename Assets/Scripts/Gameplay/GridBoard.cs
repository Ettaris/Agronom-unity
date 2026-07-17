using System.Collections.Generic;

namespace Gameplay
{
    public class GridBoard
    {
        private readonly Cell[,] _cells;
        public int Width { get; }
        public int Height { get; }

        public GridBoard(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new Cell[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _cells[x, y] = new Cell(x, y);
        }

        public Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return _cells[x, y];
        }

        public bool IsFree(int x, int y)
        {
            var cell = GetCell(x, y);
            return cell != null && !cell.IsOccupied;
        }

        public bool PlacePlant(PlantInstance plant, int x, int y)
        {
            if (!IsFree(x, y)) return false;
            var cell = GetCell(x, y);
            cell.Plant = plant;
            return true;
        }

        public PlantInstance RemovePlant(int x, int y)
        {
            var cell = GetCell(x, y);
            if (cell == null) return null;
            var plant = cell.Plant;
            cell.Plant = null;
            return plant;
        }

        public List<Cell> GetNeighbors(int x, int y, bool includeDiagonals = false)
        {
            var result = new List<Cell>(includeDiagonals ? 8 : 4);
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (!includeDiagonals && (dx != 0 && dy != 0)) continue;
                    var cell = GetCell(x + dx, y + dy);
                    if (cell != null) result.Add(cell);
                }
            }
            return result;
        }

        public List<PlantInstance> GetAllPlants()
        {
            var plants = new List<PlantInstance>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var plant = _cells[x, y].Plant;
                    if (plant != null) plants.Add(plant);
                }
            return plants;
        }

        public List<PlantInstance> GetPlantsInRectangle(int x1, int y1, int x2, int y2)
        {
            int minX = UnityEngine.Mathf.Min(x1, x2);
            int maxX = UnityEngine.Mathf.Max(x1, x2);
            int minY = UnityEngine.Mathf.Min(y1, y2);
            int maxY = UnityEngine.Mathf.Max(y1, y2);

            var plants = new List<PlantInstance>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var cell = GetCell(x, y);
                    if (cell != null && cell.Plant != null)
                        plants.Add(cell.Plant);
                }
            }
            return plants;
        }
    }
}