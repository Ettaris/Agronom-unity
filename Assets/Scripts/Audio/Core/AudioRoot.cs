using UnityEngine;
using Infrastructure;

public class AudioRoot : MonoBehaviour, IGameSystem
{
    [SerializeField] private AudioConfig _config;
    [SerializeField] private SfxPlayer _sfxPlayer;
    [SerializeField] private MusicPlayer _musicPlayer;
    [SerializeField] private VoicePlayer _voicePlayer;

    private AudioService _audioService;

    public void Initialize()
    {
        _audioService = new AudioService(_config, _sfxPlayer, _musicPlayer, _voicePlayer);
        ServiceLocator.Register(_audioService);
        _sfxPlayer.Initialize(_config);
        _musicPlayer.Initialize(_config);
        _voicePlayer.Initialize(_config);
        _audioService.Initialize();
    }

    public void Dispose()
    {
        _audioService.Dispose();
    }

}