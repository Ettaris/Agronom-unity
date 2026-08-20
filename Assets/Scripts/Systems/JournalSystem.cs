using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using System.Collections.Generic;

namespace Systems
{
    public class JournalSystem : IGameSystem
    {
        private JournalData _journal;
        private bool _isLoaded;

        public List<IJournalEntryData> GetPlantEntries() => _journal.GetPlantEntries();
        public List<IJournalEntryData> GetModifierEntries() => _journal.GetModifierEntries();

        public void Initialize()
        {
            _journal = new JournalData();
            _isLoaded = false;
            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<GenomeDiscoveredEvent>(DiscoverPlantProperties);
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

        public void DiscoverPlantProperties(GenomeDiscoveredEvent evt)
        {
            if (evt.Plant == null) return;
            foreach (var prop in evt.Plant.Genome.Properties)
            {
                bool perm = evt.isPermanent || (evt.Plant.PermanentModifier != null && evt.Plant.PermanentModifier.Data == prop.Data) ;
                _journal.AddOrUpdatePlant(evt.Plant, prop.Data, perm);
            }
        }

        public JournalData GetJournal() => _journal;

        public void SetJournal(JournalData journal)
        {
            _journal = journal ?? new JournalData();
            _isLoaded = true;
        }

        public bool IsPropertyDiscovered(PlantData plant, GenomePropertyData property)
        {
            return _journal.IsPropertyDiscovered(plant, property);
        }
    }
}