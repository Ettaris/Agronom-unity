namespace Infrastructure
{
    public class CommandProcessor : IGameSystem
    {
        public void Initialize()
        {
            // Никакой инициализации не требуется
        }

        public void Dispose()
        {
            // Очистка, если есть очередь
        }

        public void ExecuteCommand(ICommand command)
        {
            command?.Execute();
        }
    }
}