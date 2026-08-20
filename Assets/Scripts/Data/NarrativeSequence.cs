using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NarrativeSequence", menuName = "Game/Narrative Sequence")]
public class NarrativeSequence : ScriptableObject
{
    public string sequenceId;
    [SerializeReference] public List<NarrativeStep> steps = new List<NarrativeStep>();
}