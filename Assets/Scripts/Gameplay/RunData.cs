using Infrastructure;
using Data;
using System.Collections.Generic;

namespace Gameplay
{
    public class RunData
    {
        public SeedGenerator Random { get; }
        public GridBoard Board { get; private set; }
        public Hand Hand { get; }
        public Deck Deck { get; }
        public PlayerInventory Inventory { get; }
        public JournalData Journal { get; private set; }
        public int Seed { get; }
        public int CurrentDay { get; set; }
        public bool IsQuotaReached { get; set; }
        public int CurrentStageIndex { get; set; }
        public int StageStartDay { get; set; }
        public StageData[] Stages { get; }
        public Dictionary<PlantData, GenomePropertyData> PermanentModifiers { get; set; }
        public Dictionary<PlantData, List<GenomePropertyData>> DiscoveredGenomes { get; set; }

        public RunData(int seed, int boardWidth, int boardHeight, int handMaxSize, JournalData journal, StageData[] stages)
        {
            Seed = seed;
            Random = new SeedGenerator(seed);
            Board = new GridBoard(boardWidth, boardHeight);
            Hand = new Hand(handMaxSize);
            Deck = new Deck();
            Inventory = new PlayerInventory();
            CurrentDay = 0;
            IsQuotaReached = false;
            Journal = journal;
            Stages = stages;
            DiscoveredGenomes = new Dictionary<PlantData, List<GenomePropertyData>>(40);
        }

        public StageData GetCurrentStage()
        {
            if (Stages == null || CurrentStageIndex >= Stages.Length)
                return default;
            return Stages[CurrentStageIndex];
        }

        public void SetJournalData(JournalData journalData) => Journal = journalData;
        public void SetBoard(GridBoard board) => Board = board;

        public bool IsAllStagesCompleted => CurrentStageIndex >= Stages.Length;
    }
}