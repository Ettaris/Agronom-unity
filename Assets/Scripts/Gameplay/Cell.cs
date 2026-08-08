namespace Gameplay
{
    /// <summary>
    /// Данные логической клетки(Позиция, растение, оккупировано)
    /// </summary>
    public class Cell
    {
        public int X { get; }
        public int Y { get; }
        public PlantInstance Plant { get; set; }
        public bool IsOccupied => Plant != null;

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
            Plant = null;
        }
    }
}