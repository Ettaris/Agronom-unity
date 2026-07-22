using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Система учёта калорий и проверки выполнения дневной квоты.
    /// </summary>
    public class ScoreSystem : IGameSystem
    {
        private RunData _runData;

        public void Initialize()
        {
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            _runData = evt.RunData;
            Debug.Log(_runData + " - score system run data");
        }

        /// <summary>
        /// Добавляет калории к общему счёту и проверяет выполнение квоты.
        /// </summary>
        public void AddCalories(int amount)
        {
            if (amount <= 0) return;

            _runData.Inventory.Calories += amount;

            // Проверка дневной квоты (оставляем как есть)
            if (!_runData.IsQuotaReached && _runData.Inventory.Calories >= _runData.DailyQuota)
            {
                _runData.IsQuotaReached = true;
                EventBus.Publish(new QuotaReachedEvent { DayNumber = _runData.CurrentDay });
            }

            // Проверка общей цели
            if (!_runData.IsTotalGoalReached && _runData.Inventory.Calories >= _runData.TotalCaloriesGoal)
            {
                _runData.IsTotalGoalReached = true;
                EventBus.Publish(new TotalGoalReachedEvent());
            }

            EventBus.Publish(new ScoreChangedEvent
            {
                CurrentCalories = _runData.Inventory.Calories,
                DailyQuota = _runData.DailyQuota
            });
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            Debug.Log("Score system onDayStarted");
            if (_runData != null)
            {
                // Сбрасываем флаг выполнения квоты в начале дня
                _runData.IsQuotaReached = false;
                // Публикуем событие для обновления UI
                EventBus.Publish(new ScoreChangedEvent
                {
                    CurrentCalories = _runData.Inventory.Calories,
                    DailyQuota = _runData.DailyQuota
                });
            }
        }
    }
}