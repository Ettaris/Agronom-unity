using UnityEngine;

public class SfxPlayer : MonoBehaviour
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
        float vol = audio.volume * _config.sfxVolume * _config.masterVolume;
        float pitch = audio.pitch + Random.Range(-audio.randomPitch, audio.randomPitch);
        vol += Random.Range(-audio.randomVolume, audio.randomVolume);
        vol = Mathf.Clamp01(vol);
        _source.pitch = pitch;
        _source.PlayOneShot(clip, vol);
    }

    public void SetVolume(float v) { /* можно сохранять TODO: */  }
}