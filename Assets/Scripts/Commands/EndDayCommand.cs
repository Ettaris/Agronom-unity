using Infrastructure;
using Infrastructure.Events;

namespace Commands
{
    public struct EndDayCommand : ICommand
    {
        public void Execute()
        {
            EventBus.Publish(new EndDayCommand()); // можно вызвать напр€мую, но дл€ команд используем CommandProcessor
        }
    }
}