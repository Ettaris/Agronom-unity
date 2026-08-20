using UnityEngine;
using System.Collections.Generic;
using Data;

public interface IJournalEntryData
{
    string Title { get; }
    Sprite Icon { get; }
    string Description { get; } // для модификаторов
    List<GenomePropertyData> Properties { get; } // для растений – список свойств
    bool IsPermanent { get; } // для модификаторов – является ли перманентным (для каких-то растений)
    string PermanentFor { get; } // строка с названиями растений, для которых перманентный
    int Count { get; } // количество раз, когда открыт (для растений – число анализов)
}