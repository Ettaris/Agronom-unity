using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;

namespace Managers
{
    public class DayManager : IGameSystem
    {
        private RunData _runData;
        private int _currentDay;
        private int _totalDays;

        public void Initialize()
        {
            _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in DayManager!");
                return;
            }

            _totalDays = ServiceLocator.Get<GameConfig>().totalDays;
            _currentDay = 0;

            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<EndDayCommand>(OnEndDayCommand); // команда от игрока
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Unsubscribe<EndDayCommand>(OnEndDayCommand);
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _runData = evt.RunData;
            if (!evt.IsLoaded)
            {
                _currentDay = 1;
                _runData.CurrentDay = _currentDay;
                EventBus.Publish(new DayStartedEvent { DayNumber = _currentDay });
            }
            else
            {
                _currentDay = _runData.CurrentDay;
                // Публикуем событие загрузки дня для UI
                EventBus.Publish(new DayLoadedEvent { DayNumber = _currentDay });
            }
        }

        private void OnEndDayCommand(EndDayCommand command)
        {
            // Завершаем текущий день
            EventBus.Publish(new DayEndedEvent { DayNumber = _currentDay });

            // Проверяем, достигнута ли квота (это может делать ScoreSystem)
            // Если день завершён, переходим к следующему
            _currentDay++;

            if (_currentDay > _totalDays)
            {
                // Забег завершён
                ServiceLocator.Get<RunManager>().EndRun();
                return;
            }

            // Начинаем новый день
            _runData.CurrentDay = _currentDay;
            EventBus.Publish(new DayStartedEvent { DayNumber = _currentDay });
        }
    }

    // Команда для завершения дня (игрок нажимает кнопку)
    public struct EndDayCommand : ICommand
    {
        public void Execute()
        {
            EventBus.Publish(new EndDayCommand()); // можно вызвать напрямую, но для команд используем CommandProcessor
        }
    }
}