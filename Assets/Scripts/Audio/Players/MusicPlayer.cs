using UnityEngine;
using DG.Tweening;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _sourceA;
    [SerializeField] AudioSource _sourceB;
    private AudioSource _activeSource;
    private AudioConfig _config;
    private Tween _crossfadeTween;

    public void Initialize(AudioConfig config)
    {
        _config = config;
        if (_sourceA == null)
            _sourceA = gameObject.AddComponent<AudioSource>();
        if (_sourceB == null)
            _sourceB = gameObject.AddComponent<AudioSource>();
        _sourceA.loop = true;
        _sourceB.loop = true;
        _sourceA.volume = 0f;
        _sourceB.volume = 0f;
        _sourceA.playOnAwake = false;
        _sourceB.playOnAwake = false;
        _activeSource = _sourceA;
        Stop();
    }

    public void Play(AudioData audio)
    {
        if (audio == null) return;
        var clip = audio.GetRandomClip();
        if (clip == null) return;
        StopCrossfade();
        // Используем активный источник
        _activeSource.clip = clip;
        _activeSource.volume = audio.volume * _config.musicVolume * _config.masterVolume;
        _activeSource.pitch = audio.pitch;
        _activeSource.Play();
    }

    public void Stop()
    {
        StopCrossfade();
        _sourceA.Stop();
        _sourceB.Stop();
        _sourceA.volume = 0f;
        _sourceB.volume = 0f;
        _activeSource = _sourceA;
    }

    public void Crossfade(AudioData audio, float duration = 1f)
    {
        if (audio == null) return;
        var clip = audio.GetRandomClip();
        if (clip == null) return;
        StopCrossfade();
        // Определяем, какой источник сейчас активен, и используем другой
        AudioSource nextSource = (_activeSource == _sourceA) ? _sourceB : _sourceA;
        nextSource.clip = clip;
        nextSource.volume = 0f;
        nextSource.pitch = audio.pitch;
        nextSource.Play();

        float targetVolume = audio.volume * _config.musicVolume * _config.masterVolume;
        _crossfadeTween = DOTween.To(() => _activeSource.volume, x => _activeSource.volume = x, 0f, duration)
            .SetEase(Ease.Linear);
        DOTween.To(() => nextSource.volume, x => nextSource.volume = x, targetVolume, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _activeSource.Stop();
                _activeSource = nextSource;
            });
    }

    private void StopCrossfade()
    {
        _crossfadeTween?.Kill();
        _crossfadeTween = null;
    }

    public void SetVolume(float v) { /* можно сохранять TODO: */ }
}