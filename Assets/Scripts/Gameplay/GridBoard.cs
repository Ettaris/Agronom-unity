using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// ’ранит массив клеток Cell и отвечает за их логику.
    /// </summary>
    public class GridBoard
    {
        private Cell[,] _cells;
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

        public bool CanPlace(Vector2Int position, Vector2Int size)
        {
            int x = position.x, y = position.y;
            for (int dx = 0; dx < size.x; dx++)
                for (int dy = 0; dy < size.y; dy++)
                {
                    int cx = x + dx, cy = y + dy;
                    if (cx < 0 || cx >= Width || cy < 0 || cy >= Height)
                    {
                        Debug.Log($"CanPlace: Out of bounds at ({cx},{cy})");
                        return false;
                    }
                    if (_cells[cx, cy].Plant != null)
                    {
                        Debug.Log($"CanPlace: Cell ({cx},{cy}) is occupied");
                        return false;
                    }
                }
            Debug.Log($"CanPlace: All cells free for position {position}, size {size}");
            return true;
        }

        public bool PlacePlant(PlantInstance plant, Vector2Int position)
        {
            Vector2Int size = plant.PlantData.size;
            if (!CanPlace(position, size)) return false;

            int x = position.x, y = position.y;
            for (int dx = 0; dx < size.x; dx++)
                for (int dy = 0; dy < size.y; dy++)
                    _cells[x + dx, y + dy].Plant = plant;

            // —охран€ем позицию в растении
            plant.Position = position;
            return true;
        }

        public void RemovePlant(PlantInstance plant)
        {
            if (plant == null) return;
            Vector2Int pos = plant.Position;
            Vector2Int size = plant.PlantData.size;
            for (int dx = 0; dx < size.x; dx++)
                for (int dy = 0; dy < size.y; dy++)
                {
                    int cx = pos.x + dx, cy = pos.y + dy;
                    if (cx >= 0 && cx < Width && cy >= 0 && cy < Height && _cells[cx, cy].Plant == plant)
                        _cells[cx, cy].Plant = null;
                }
            plant.Position = Vector2Int.zero;
            plant.CurrentCell = null; // или nullify
        }

        // ѕолучить все растени€ на поле. TODO: O(n2) - критично?
        public List<PlantInstance> GetAllPlants()
        {
            var result = new List<PlantInstance>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (_cells[x, y].Plant != null && !result.Contains(_cells[x, y].Plant))
                        result.Add(_cells[x, y].Plant);
            return result;
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
        //TODO: Duplicate
        public bool PlacePlant(PlantInstance plant, int x, int y)
        {
            if (!IsFree(x, y)) return false;
            var cell = GetCell(x, y);
            cell.Plant = plant;
            return true;
        }
        //TODO: Duplicate
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

        public void Clear()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    _cells[x, y].Plant = null;
        }
    }
}