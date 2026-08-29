using Data;
using Gameplay;
using Infrastructure;
using Infrastructure.Events;
using Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Systems
{
    public class JournalSystem : IGameSystem
    {
        private JournalData _journal;
        private bool _isLoaded;
        private GameConfig _config;

        public void Initialize()
        {
            _journal = new JournalData();
            _isLoaded = false;
            _config = ServiceLocator.Get<GameConfig>();

            EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Subscribe<PlantAnalyzedEvent>(OnPlantAnalyzed);
            EventBus.Subscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
        }

        public async void Dispose()
        {
            EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
            EventBus.Unsubscribe<PlantAnalyzedEvent>(OnPlantAnalyzed);
            EventBus.Unsubscribe<GenomeDiscoveredEvent>(OnGenomeDiscovered);
            if (_journal != null)
                await ServiceLocator.Get<SaveManager>().SaveJournalAsync(_journal);
        }

        private async void OnRunStarted(RunStartedEvent evt)
        {
            var loaded = await ServiceLocator.Get<SaveManager>().LoadJournalAsync();
            if (loaded != null) _journal = loaded;
            _isLoaded = true;
            ServiceLocator.Get<RunManager>().CurrentRunData.SetJournalData(_journal);
        }

        private void OnPlantAnalyzed(PlantAnalyzedEvent evt)
        {
            if (evt.Plant == null) return;
            _journal.StudyPlant(evt.Plant.PlantData, 1);

            bool isPermanent(GenomePropertyInstance prop) =>
                (evt.Plant.PermanentModifier != null && evt.Plant.PermanentModifier.Data == prop.Data);

            foreach (var prop in evt.Plant.Genome.Properties)
            {
                bool perm = isPermanent(prop);
                _journal.DiscoverModifier(prop.Data, evt.Plant, perm);
            }
        }

        private void OnGenomeDiscovered(GenomeDiscoveredEvent evt)
        {
        }

        public List<IJournalEntryData> GetPlantEntries()
        {
            return _journal.GetPlantEntries(_config);
        }

        public List<IJournalEntryData> GetModifierEntries()
        {
            return _journal.GetModifierEntries(_config);
        }

        public JournalData GetJournal() => _journal;
        public void SetJournal(JournalData journal)
        {
            _journal = journal ?? new JournalData();
            _isLoaded = true;
        }

        public bool IsPlantStudied(PlantData plant) => _journal.IsPlantStudied(plant);
        public bool IsModifierDiscovered(GenomePropertyData modifier) => _journal.IsModifierDiscovered(modifier);
    }
}