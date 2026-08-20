using System.Collections.Generic;
using UnityEngine;

public class ResourcesNarrativeSequenceProvider : INarrativeSequenceProvider
{
    private readonly Dictionary<string, NarrativeSequence> _cache = new Dictionary<string, NarrativeSequence>();

    public NarrativeSequence LoadSequence(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("ResourcesNarrativeSequenceProvider: id is null or empty");
            return null;
        }

        if (_cache.TryGetValue(id, out var cached))
            return cached;

        var sequence = Resources.Load<NarrativeSequence>($"NarrativeSequences/{id}");
        if (sequence == null)
        {
            Debug.LogError($"ResourcesNarrativeSequenceProvider: Sequence not found at path NarrativeSequences/{id}");
            return null;
        }

        _cache[id] = sequence;
        return sequence;
    }

    public void UnloadSequence(string id)
    {
        if (_cache.TryGetValue(id, out var seq))
        {
            Resources.UnloadAsset(seq);
            _cache.Remove(id);
        }
    }

    public void PreloadSequences(string[] ids)
    {
        foreach (var id in ids)
        {
            LoadSequence(id);
        }
    }
}