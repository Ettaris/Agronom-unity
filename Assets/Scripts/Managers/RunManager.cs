using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using UnityEngine;

namespace Managers
{
    public class RunManager : IGameSystem
    {
        public RunData CurrentRunData { get; private set; }
        public int CurrentRunSeed { get; private set; }
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

        private void OnRunStarted(RunStartedEvent evt)
        {
            _isRunActive = true;
        }

        public void LoadRunData(RunData runData)
        {
            CurrentRunData = runData;
            EventBus.Publish(new RunLoadedEvent { RunData = runData });
            // ѕубликуем RunStartedEvent, чтобы остальные системы (например, CardDrawSystem) не генерировали предложение
            EventBus.Publish(new RunStartedEvent { Seed = runData.Seed, RunData = runData, IsLoaded = true });
        }

        public void StartNewRun(int seed)
        {
            // ѕубликуем событие запроса генерации забега
            EventBus.Publish(new RunGenerationRequestedEvent { Seed = seed });
        }

        public void EndRun()
        {
            if (!_isRunActive || CurrentRunData == null) return;
            bool isWin = CurrentRunData.IsAllStagesCompleted; 
            EventBus.Publish(new RunEndedEvent { FinalRunData = CurrentRunData, IsWin = isWin });
            _isRunActive = false;
        }

        private void OnRunEnded(RunEndedEvent evt)
        {
            _isRunActive = false;
            // —охран€ем мета-прогресс
            ServiceLocator.Get<SaveManager>().SaveJournal(CurrentRunData.Journal);
        }

        public void SetupRunData(RunData runData, int seed)
        {
            CurrentRunData = runData;
            CurrentRunSeed = seed;
            SendRunDataToGameManager(runData);
        }

        private void SendRunDataToGameManager(RunData runData)
        {
            ServiceLocator.Get<GameManager>().InitializeAndActivateRun(runData);
        }
    }
}