using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;

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
            _runData = ServiceLocator.Get<RunManager>().CurrentRunData;
            if (_runData == null)
            {
                UnityEngine.Debug.LogError("RunData is null in ScoreSystem!");
                return;
            }

            // Подписываемся на событие начала дня для сброса квоты
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
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