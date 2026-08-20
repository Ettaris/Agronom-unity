using System;
using Infrastructure;
using UnityEngine;

namespace Systems
{
    public class NarrativeSystem : IGameSystem
    {
        private NarrativeRunner _runner;
        private INarrativeSequenceProvider _provider;

        public bool IsRunning => _runner != null && _runner.IsRunning;

        public NarrativeSystem()
        {
            _provider = new ResourcesNarrativeSequenceProvider();
        }

        public void SetProvider(INarrativeSequenceProvider provider)
        {
            _provider = provider ?? new ResourcesNarrativeSequenceProvider();
        }

        public void Initialize() { }

        public void Dispose()
        {
            StopSequence();
        }

        public void StartSequence(string sequenceId, Action onComplete = null, Action onCancel = null)
        {
            var sequence = _provider.LoadSequence(sequenceId);
            if (sequence == null)
            {
                Debug.LogError($"NarrativeSystem: Sequence '{sequenceId}' not found.");
                onComplete?.Invoke();
                return;
            }
            StartSequence(sequence, onComplete, onCancel);
        }

        public void StartSequence(NarrativeSequence sequence, Action onComplete = null, Action onCancel = null)
        {
            StopSequence();
            if (sequence == null)
            {
                onComplete?.Invoke();
                return;
            }

            _runner = new NarrativeRunner();
            _runner.Start(sequence, () =>
            {
                onComplete?.Invoke();
                _runner = null;
            }, () =>
            {
                onCancel?.Invoke();
                _runner = null;
            });
        }

        public void StopSequence()
        {
            if (_runner != null)
            {
                _runner.Stop();
                _runner = null;
            }
        }

        public void UnloadSequence(string id)
        {
            _provider.UnloadSequence(id);
        }
    }
}