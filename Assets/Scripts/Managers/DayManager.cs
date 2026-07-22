using Commands;
using Data;
using DG.Tweening;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using UnityEngine;

namespace Managers
{
    public class DayManager : IGameSystem
    {
        private RunData _runData;
        private int _currentDay;
        private int _totalDays;

        public void Initialize()
        {

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

            _totalDays = ServiceLocator.Get<GameConfig>().totalDays;
            _currentDay = 0;

            if (!evt.IsLoaded)
            {
                _currentDay = 1;
                _runData.CurrentDay = _currentDay;
                DOVirtual.DelayedCall(0.1f, () => EventBus.Publish(new DayStartedEvent { DayNumber = _currentDay }));
                DOVirtual.DelayedCall(0.1f, () => Debug.Log("Publish OnDayStarted from OnRunStarted(DayManager) - new game"));

            }
            else
            {
                _currentDay = _runData.CurrentDay;
                // Публикуем событие загрузки дня для UI
                Debug.Log("Publish OnDayStarted from OnRunStarted(DayManager) - loaded game");
                DOVirtual.DelayedCall(0.01f, () => EventBus.Publish(new DayStartedEvent { DayNumber = _currentDay }));
            }

            Debug.Log(_runData + " - DayManager run data from OnRunStarted");
        }

        private void OnEndDayCommand(EndDayCommand command)
        {
            // Завершаем текущий день

            Debug.Log("OnEndDay from daymanager");
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
            Debug.Log("Publish onDayStarted from OnEndDayCommand");
            EventBus.Publish(new DayStartedEvent { DayNumber = _currentDay });
        }
    }

    // Команда для завершения дня (игрок нажимает кнопку)
}