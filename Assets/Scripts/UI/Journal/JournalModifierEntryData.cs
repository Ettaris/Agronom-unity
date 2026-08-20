using UnityEngine;
using System.Collections.Generic;
using Data;

public class JournalModifierEntryData : IJournalEntryData
{
    private GenomePropertyData _property;
    private string _permanentFor;
    private bool _isPermanent;

    public JournalModifierEntryData(GenomePropertyData property, bool isPermanent, string permanentFor)
    {
        _property = property;
        _isPermanent = isPermanent;
        _permanentFor = permanentFor;
    }

    public string Title => _property.propertyName;
    public Sprite Icon => _property.icon;
    public string Description => $"{_property.description}\nСтоимость: {_property.genomeCost}";
    public List<GenomePropertyData> Properties => new List<GenomePropertyData> { _property }; // для единообразия
    public bool IsPermanent => _isPermanent;
    public string PermanentFor => _permanentFor;
    public int Count => 1;
}