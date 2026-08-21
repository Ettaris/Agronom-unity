using Data;
using Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Events
{
    //Для анимаций клеток.
    public enum EffectType
    {
        Grow,    
        Boost,   
        Debuff,  
        Bomb,    
        Weed,
        Sacrifice,
    }

    public struct EffectAppliedEvent
    {
        public int X;
        public int Y;
        public EffectType Type;
        public float Duration; 
    }

    // ---------- Жизненный цикл забега ----------
    public struct RunStartedEvent
    {
        public int Seed;
        public RunData RunData;
        public bool IsLoaded;
    }

    public struct RunEndedEvent
    {
        public RunData FinalRunData;
        public bool IsWin;
    }

    // ---------- Дни ----------
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

    // ---------- Посадка и удаление растений ----------
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

    // ---------- Рост и сбор ----------
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

    // ---------- Свойства и анализ ----------

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
        public bool isPermanent;
    }

    public struct GenomeChangedEvent
    {
        public PlantInstance Plant;
        public GenomePropertyInstance Property;
        public bool IsAdded;
    }

    public struct GenomeTransferFailedEvent
    {
        public PlantInstance Donor;
        public PlantInstance Target;
    }

    // ---------- Счёт и прогресс ----------
    public struct ScoreChangedEvent
    {
        public int CurrentCalories;
    }

    // ---------- Сохранение ----------
    public struct SaveRequestedEvent
    {
        public bool IsMetaSave; // true – сохранить журнал, false – сохранить забег
    }

    public struct HarvestModifiedEvent
    {
        public PlantInstance Plant;
        public int ModifiedCalories;
    }

    public struct PlantMutatedEvent
    {
        public PlantInstance Plant;
        public int NewFillPercent; // процент заполнения генома
    }

    public struct CardSelectedEvent
    {
        public ItemInstance Item;
    }

    public struct RunGenerationRequestedEvent
    {
        public int Seed;
    }

    public struct HandUpdatedEvent { }

    public struct HandFullEvent { }

    public struct OfferGeneratedEvent
    {
        public List<ItemInstance> Offer;
        public int MaxSelectable;
    }

    public struct RunLoadedEvent
    {
        public RunData RunData;
    }

    public struct StageChangedEvent
    {
        public int StageIndex;
        public RunData RunData;
    }

    public struct StageFailedEvent
    {
        public int StageIndex;
        public int RequiredCalories;
        public int CurrentCalories;
    }

    public struct GameWinEvent { }

    public struct PlantAnalyzedEvent
    {
        public PlantInstance Plant;
    }

    public struct CardDropEvent
    {
        public CardView Card;
        public GameObject Target;
    }

    //SFX and Music Events
    public struct LabOpenedEvent { }
    public struct CardHoveredEvent { }

}