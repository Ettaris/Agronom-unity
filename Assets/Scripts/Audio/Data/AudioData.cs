using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "Game/Audio Data")]
public class AudioData : ScriptableObject
{
    [Header("Clips")]
    public AudioClip[] clips;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Pitch")]
    public float pitch = 1f;
    [Range(0f, 0.2f)]
    public float randomPitch = 0.05f; 

    [Header("Random Volume Variation")]
    [Range(0f, 0.2f)]
    public float randomVolume = 0f; 

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}