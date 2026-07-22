using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using UnityEngine;

namespace Managers
{
    public class RunManager : IGameSystem
    {
        public RunData CurrentRunData { get; set; }
        private bool _isRunActive;

        public void Initialize()
        {
            _isRunActive = false;
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<RunEndedEvent>(OnRunEnded);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Unsubscribe<RunEndedEvent>(OnRunEnded);
        }

        public void LoadRunData(RunData runData)
        {
            CurrentRunData = runData;
            EventBus.Publish(new RunLoadedEvent { RunData = runData });
            // Публикуем RunStartedEvent, чтобы остальные системы (например, CardDrawSystem) не генерировали предложение
            EventBus.Publish(new RunStartedEvent { Seed = runData.Seed, RunData = runData, IsLoaded = true });
        }

        public void StartNewRun(int seed)
        {
            // Публикуем событие запроса генерации забега
            EventBus.Publish(new RunGenerationRequestedEvent { Seed = seed });
        }

        public void EndRun()
        {
            if (!_isRunActive || CurrentRunData == null) return;
            bool isWin = CurrentRunData.IsTotalGoalReached;
            EventBus.Publish(new RunEndedEvent { FinalRunData = CurrentRunData, IsWin = isWin });
            _isRunActive = false;
        }

        private void OnRunStarted(RunStartedEvent evt)
        {
            CurrentRunData = evt.RunData;
            _isRunActive = true;
            Debug.Log("OnRunStarted runManager");
            // Можно также инициализировать первый день
        }

        private void OnRunEnded(RunEndedEvent evt)
        {
            _isRunActive = false;
            // Сохраняем мета-прогресс
            ServiceLocator.Get<SaveManager>().SaveJournal(CurrentRunData.Journal);
        }
    }

    // События уже определены в GameEvents.cs
}