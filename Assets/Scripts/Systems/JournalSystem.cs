using Data;
using Gameplay;
using Infrastructure;
using Managers;

namespace Systems
{
    public class JournalSystem : IGameSystem
    {
        private JournalData _journal;

        public void Initialize()
        {
            _journal = ServiceLocator.Get<SaveManager>().LoadJournal() ?? new JournalData();
        }

        public void Dispose()
        {
            ServiceLocator.Get<SaveManager>().SaveJournal(_journal);
        }

        public void DiscoverProperty(GenomePropertyData property)
        {
            if (property == null) return;
            _journal.AddEntry(property);
            // ћожно публиковать событие, если нужно
        }

        public bool IsPropertyDiscovered(GenomePropertyData property)
        {
            return _journal.IsPropertyDiscovered(property);
        }

        public JournalData GetJournal() => _journal;

        public void SetJournal(JournalData journal)
        {
            _journal = journal ?? new JournalData();
        }
    }
}