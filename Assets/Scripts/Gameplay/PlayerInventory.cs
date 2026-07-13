namespace Gameplay
{
    public class PlayerInventory
    {
        public int Calories { get; set; }
        public int Fertilizer { get; set; } // задел на будущее

        public PlayerInventory(int startingCalories = 0)
        {
            Calories = startingCalories;
        }
    }
}