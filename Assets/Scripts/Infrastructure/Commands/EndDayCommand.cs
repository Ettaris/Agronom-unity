using Infrastructure;
using Infrastructure.Events;

namespace Commands
{
    public struct EndDayCommand : ICommand
    {
        public void Execute()
        {
            EventBus.Publish(new EndDayCommand());
        }
    }
}