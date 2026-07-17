using Data;

namespace Gameplay
{
    public class ItemInstance
    {
        public ItemData Data { get; protected set; }
        public int Quantity { get; set; } // для стакаемости, но пока не используется

        public ItemInstance(ItemData data, int quantity = 1)
        {
            Data = data;
            Quantity = quantity;
        }

        // Можно добавить виртуальные методы, если потребуется переопределять поведение
    }
}