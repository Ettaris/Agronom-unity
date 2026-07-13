using Infrastructure;
using Data;

namespace Gameplay
{
    public class RunData
    {
        public int Seed { get; }
        public SeedGenerator Random { get; }
        public GridBoard Board { get; }
        public Hand Hand { get; }
        public Deck Deck { get; }
        public PlayerInventory Inventory { get; }
        public int CurrentDay { get; set; }
        public int DailyQuota { get; set; }
        public bool IsQuotaReached { get; set; }

        // —сылка на журнал (мета-данные) передаЄтс€ извне
        public JournalData Journal { get; set; }

        public RunData(int seed, int boardWidth, int boardHeight, int handMaxSize, int dailyQuota, JournalData journal)
        {
            Seed = seed;
            Random = new SeedGenerator(seed);
            Board = new GridBoard(boardWidth, boardHeight);
            Hand = new Hand(handMaxSize);
            Deck = new Deck();
            Inventory = new PlayerInventory();
            CurrentDay = 0;
            DailyQuota = dailyQuota;
            IsQuotaReached = false;
            Journal = journal;
        }
    }
}