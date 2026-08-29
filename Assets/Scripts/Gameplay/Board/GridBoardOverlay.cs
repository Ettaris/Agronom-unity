using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// ќверлей над основным полем дл€ реализации подсчетов под PlacementPreview
    /// </summary>
    public class GridBoardOverlay : IGridBoard
    {
        private readonly IGridBoard _baseBoard;
        private readonly Dictionary<Vector2Int, PlantInstance> _overlayPlants = new Dictionary<Vector2Int, PlantInstance>();

        public GridBoardOverlay(IGridBoard baseBoard)
        {
            _baseBoard = baseBoard;
        }

        public int Width => _baseBoard.Width;
        public int Height => _baseBoard.Height;

        public Cell GetCell(int x, int y)
        {
            var pos = new Vector2Int(x, y);
            if (_overlayPlants.TryGetValue(pos, out var plant))
            {
                return new Cell(x, y) { Plant = plant };
            }
            return _baseBoard.GetCell(x, y);
        }

        public bool IsFree(int x, int y)
        {
            var pos = new Vector2Int(x, y);
            if (_overlayPlants.ContainsKey(pos)) return false;
            return _baseBoard.IsFree(x, y);
        }

        public bool CanPlace(Vector2Int position, Vector2Int size)
        {
            for (int dx = 0; dx < size.x; dx++)
                for (int dy = 0; dy < size.y; dy++)
                {
                    int cx = position.x + dx, cy = position.y + dy;
                    var pos = new Vector2Int(cx, cy);
                    if (_overlayPlants.ContainsKey(pos)) return false;
                    if (!_baseBoard.IsFree(cx, cy)) return false;
                }
            return true;
        }

        public List<Cell> GetNeighbors(int x, int y, bool includeDiagonals = false)
        {
            var result = new List<Cell>();
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (!includeDiagonals && (dx != 0 && dy != 0)) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= Width || ny < 0 || ny >= Height) continue;
                    result.Add(GetCell(nx, ny));
                }
            return result;
        }

        public List<PlantInstance> GetAllPlants()
        {
            var all = _baseBoard.GetAllPlants();
            foreach (var kvp in _overlayPlants)
            {
                var basePlant = _baseBoard.GetCell(kvp.Key.x, kvp.Key.y)?.Plant;
                if (basePlant != null)
                    all.Remove(basePlant);
                all.Add(kvp.Value);
            }
            return all;
        }

        public void Clear()
        {
            _overlayPlants.Clear();
            _baseBoard.Clear();
        }

        public void AddOverlayPlant(PlantInstance plant, Vector2Int position)
        {
            _overlayPlants[position] = plant;
            plant.Position = position;
            plant.CurrentCell = new Cell(position.x, position.y) { Plant = plant }; // фиктивный, только дл€ чтени€
        }

        public void RemoveOverlayPlant(Vector2Int position)
        {
            _overlayPlants.Remove(position);
        }

        public void ClearOverlay()
        {
            _overlayPlants.Clear();
        }
    }
}