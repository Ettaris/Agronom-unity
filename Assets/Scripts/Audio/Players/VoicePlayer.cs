using UnityEngine;

public class VoicePlayer : MonoBehaviour
{
    private AudioSource _source;
    private AudioConfig _config;

    public void Initialize(AudioConfig config)
    {
        _config = config;
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
    }

    public void Play(AudioData audio)
    {
        if (audio == null) return;
        var clip = audio.GetRandomClip();
        if (clip == null) return;
        float vol = audio.volume * _config.voiceVolume * _config.masterVolume;
        float pitch = audio.pitch + Random.Range(-audio.randomPitch, audio.randomPitch);
        _source.pitch = pitch;
        _source.volume = vol;
        _source.PlayOneShot(clip);
    }

    public void Stop()
    {
        _source.Stop();
    }

    public bool IsPlaying => _source.isPlaying;

    public void SetVolume(float v) { /* можно сохранять TODO: */ }
}