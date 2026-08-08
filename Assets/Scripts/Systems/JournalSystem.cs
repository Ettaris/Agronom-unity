using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;

namespace Systems
{
    public class JournalSystem : IGameSystem
    {
        private JournalData _journal;
        private bool _isLoaded;

        public void Initialize()
        {
            _journal = new JournalData();
            _isLoaded = false;
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<GenomeDiscoveredEvent>(DiscoverProperty);
        }

        public async void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
            if (_journal != null)
                await ServiceLocator.Get<SaveManager>().SaveJournalAsync(_journal);
        }

        private async void OnRunStarted(RunStartedEvent evt)
        {
            var journal = await ServiceLocator.Get<SaveManager>().LoadJournalAsync();
            if (journal != null) _journal = journal;
            _isLoaded = true;
        }

        public void DiscoverProperty(GenomeDiscoveredEvent evt)
        {
            if (evt.Property == null) return;
            if (_journal == null) _journal = new JournalData();
            _journal.AddEntry(evt.Property.Data);
        }

        public bool IsPropertyDiscovered(GenomePropertyData property)
        {
            return _journal != null && _journal.IsPropertyDiscovered(property);
        }

        public JournalData GetJournal() => _journal;

        public void SetJournal(JournalData journal)
        {
            _journal = journal ?? new JournalData();
            _isLoaded = true;
        }
    }
}