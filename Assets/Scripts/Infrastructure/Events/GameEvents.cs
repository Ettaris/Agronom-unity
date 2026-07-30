// GameEvents.cs
using Data;
using Gameplay;
using System.Collections.Generic;

namespace Infrastructure.Events
{
    // ---------- ∆изненный цикл забега ----------
    public struct RunStartedEvent
    {
        public int Seed;
        public RunData RunData;
        public bool IsLoaded; // true Ч загрузка сохранени€, false Ч новый забег
    }

    public struct RunEndedEvent
    {
        public RunData FinalRunData;
        public bool IsWin;
    }

    // ---------- ƒни ----------
    public struct DayStartedEvent
    {
        public int DayNumber;
    }

    public struct DayLoadedEvent
    {
        public int DayNumber;
    }

    public struct DayEndedEvent
    {
        public int DayNumber;
    }

    // ---------- ѕосадка и удаление растений ----------
    public struct PlantPlacedEvent
    {
        public PlantInstance Plant;
        public int X;
        public int Y;
    }

    public struct PlantHarvestedEvent
    {
        public PlantInstance Plant;
        public int X;
        public int Y;
        public int CaloriesGained;
    }

    public struct PlantKilledEvent
    {
        public PlantInstance Plant;
        public int X;
        public int Y;
        public string Reason;
    }


    public struct PlantRemovedEvent
    {
        public PlantInstance Plant;
        public int X;
        public int Y;
    }

    // ---------- –ост и сбор ----------
    public struct PlantGrownEvent
    {
        public PlantInstance Plant;
    }

    public struct HarvestEvent
    {
        public PlantInstance Plant;
        public int BaseCalories;
        public int ModifiedCalories; // будет заполнено после обработки свойств
    }

    // ---------- —войства и анализ ----------
    public struct PropertyDiscoveredEvent
    {
        public GenomePropertyInstance Property;
        public PlantInstance Plant;
    }

    public struct PropertyAssignedEvent
    {
        public PlantInstance Plant;
        public GenomePropertyInstance Property;
    }

    public struct PropertyExtractedEvent
    {
        public PlantInstance Donor;
        public GenomePropertyInstance Property;
    }

    // ---------- —чЄт и прогресс ----------
    public struct ScoreChangedEvent
    {
        public int CurrentCalories;
    }

    public struct TotalGoalReachedEvent { }


    // ---------- —охранение ----------
    public struct SaveRequestedEvent
    {
        public bool IsMetaSave; // true Ц сохранить журнал, false Ц сохранить забег
    }

    // ---------- ƒополнительное событие дл€ модификации калорий (используетс€ в PropertyResolverSystem) ----------
    public struct HarvestModifiedEvent
    {
        public PlantInstance Plant;
        public int ModifiedCalories;
    }


    public struct GenomeChangedEvent
    {
        public PlantInstance Plant;
        public GenomePropertyInstance Property;
        public bool IsAdded; // true Ц добавлено, false Ц удалено
    }

    public struct PlantMutatedEvent
    {
        public PlantInstance Plant;
        public int NewFillPercent; // процент заполнени€ генома
    }

    public struct CardSelectedEvent
    {
        public ItemInstance Item;
    }

    public struct FermentUsedEvent
    {
        public PlantInstance Target;
        public FermentData Ferment;
    }

    public struct BatteryUsedEvent
    {
        public PlantInstance Donor;
        public PlantInstance Target;
        public BatteryData Battery;
    }

    public struct GenomeTransferredEvent
    {
        public PlantInstance Donor;
        public PlantInstance Target;
        public GenomePropertyInstance Property;
    }

    public struct GenomeDiscoveredEvent
    {
        public PlantInstance Plant;
        public GenomePropertyInstance Property;
    }

    public struct RunGenerationRequestedEvent
    {
        public int Seed;
    }

    public struct HandUpdatedEvent { }

    public struct OfferGeneratedEvent
    {
        public List<ItemInstance> Offer;
        public int MaxSelectable;
    }

    public struct RunLoadedEvent
    {
        public RunData RunData;
    }

    public struct ServicesInitializedEvent { }

    public struct StageChangedEvent { public int StageIndex; public RunData RunData; }
    public struct StageFailedEvent { public int StageIndex; public int RequiredCalories; public int CurrentCalories; }
    public struct GameWinEvent { }
}