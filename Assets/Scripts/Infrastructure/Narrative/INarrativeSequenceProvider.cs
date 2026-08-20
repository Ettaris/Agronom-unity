public interface INarrativeSequenceProvider
{
    NarrativeSequence LoadSequence(string id);
    void UnloadSequence(string id);
    void PreloadSequences(string[] ids);
}