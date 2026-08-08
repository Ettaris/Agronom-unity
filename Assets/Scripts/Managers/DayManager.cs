using Commands;
using Data;
using DG.Tweening;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using UnityEngine;

namespace Managers
{
    public class DayManager : IGameSystem, IRunAware
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

        public void OnRunDataSetup(RunData runData)
        {
            _runData = runData;
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _currentDay = 0;

            if (!evt.IsLoaded)
            {
                _currentDay = 1;
                _runData.CurrentDay = _currentDay;
                EventBus.Publish(new DayStartedEvent { DayNumber = _currentDay });
            }
            else
            {
                _currentDay = _runData.CurrentDay;
                EventBus.Publish(new DayStartedEvent { DayNumber = _currentDay });
            }

        }

        private void OnEndDayCommand(EndDayCommand command)
        {
            if (_runData == null) return;

            // Завершаем текущий день
            EventBus.Publish(new DayEndedEvent { DayNumber = _currentDay });

            // Проверяем, не закончился ли текущий этап
            var currentStage = _runData.GetCurrentStage();

            // Проверяем, что этапы вообще существуют
            if (_runData.Stages == null || _runData.Stages.Length == 0)
            {
                // Если этапов нет – завершаем забег (аварийно)
                ServiceLocator.Get<RunManager>().EndRun();
                return;
            }

            if (_currentDay >= currentStage.totalDays)
            {
                // Этап закончился – проверяем калории
                if (_runData.Inventory.Calories >= currentStage.requiredCalories)
                {
                    // Успешно! Переходим к следующему этапу
                    _runData.CurrentStageIndex++;
                    _runData.StageStartDay = _currentDay + 1;

                    if (_runData.IsAllStagesCompleted)
                    {
                        // Победа!
                        Debug.Log("Win event");
                        EventBus.Publish(new GameWinEvent());
                        ServiceLocator.Get<RunManager>().EndRun();
                        return;
                    }
                    else
                    {
                        EventBus.Publish(new StageChangedEvent { StageIndex = _runData.CurrentStageIndex, RunData = _runData });
                        // Переход к следующему дню уже будет ниже
                    }
                }
                else
                {
                    // Поражение – не выполнил цель этапа
                    Debug.Log("Stage Failed Event");
                    EventBus.Publish(new StageFailedEvent
                    {
                        StageIndex = _runData.CurrentStageIndex,
                        RequiredCalories = currentStage.requiredCalories,
                        CurrentCalories = _runData.Inventory.Calories
                    });
                    ServiceLocator.Get<RunManager>().EndRun();
                    return;
                }
            }

            // Переход к следующему дню (увеличиваем день только если забег ещё не завершён)
            _currentDay++;
            _runData.CurrentDay = _currentDay;
            EventBus.Publish(new DayStartedEvent { DayNumber = _currentDay });
        }


    }
}