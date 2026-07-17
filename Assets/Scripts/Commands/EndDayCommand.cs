using Infrastructure;
using Infrastructure.Events;

namespace Commands
{
    public struct EndDayCommand : ICommand
    {
        public void Execute()
        {
            // Публикуем событие, на которое подписан DayManager
            EventBus.Publish(new DayEndRequestedEvent());
        }
    }
}