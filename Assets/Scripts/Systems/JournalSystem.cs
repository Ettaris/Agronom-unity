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
            EventBus.Subscribe<ServicesInitializedEvent>(OnServicesInitialized);
        }

        public async void Dispose()
        {
            EventBus.Unsubscribe<ServicesInitializedEvent>(OnServicesInitialized);
            if (_journal != null)
                await ServiceLocator.Get<SaveManager>().SaveJournalAsync(_journal);
        }

        private async void OnServicesInitialized(ServicesInitializedEvent evt)
        {
            var journal = await ServiceLocator.Get<SaveManager>().LoadJournalAsync();
            if (journal != null) _journal = journal;
            _isLoaded = true;
        }


        public void DiscoverProperty(GenomePropertyData property)
        {
            if (property == null) return;
            if (_journal == null) _journal = new JournalData();
            _journal.AddEntry(property);
            // ћожно публиковать событие, если нужно
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