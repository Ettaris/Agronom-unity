using System;
using Infrastructure;
using Infrastructure.Events;
using Managers;

public class AudioService : IGameSystem
{
    private readonly AudioConfig _config;
    private readonly SfxPlayer _sfx;
    private readonly MusicPlayer _music;
    private readonly VoicePlayer _voice;

    public AudioService(AudioConfig config, SfxPlayer sfx, MusicPlayer music, VoicePlayer voice)
    {
        _config = config;
        _sfx = sfx;
        _music = music;
        _voice = voice;
    }

    public void Initialize()
    {
        EventBus.Subscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Subscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Subscribe<PlantGrownEvent>(OnPlantGrown);
        EventBus.Subscribe<PlantAnalyzedEvent>(OnPlantAnalyzed);
        EventBus.Subscribe<GenomeTransferredEvent>(OnBatteryUsed);
        EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Subscribe<RunEndedEvent>(OnRunEnded);
        EventBus.Subscribe<LabOpenedEvent>(OnLabOpened);
        EventBus.Subscribe<CardSelectedEvent>(OnCardSelected);
        EventBus.Subscribe<CardHoveredEvent>(OnCardHovered);
        EventBus.Subscribe<EffectAppliedEvent>(OnEffectApplied);
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<PlantPlacedEvent>(OnPlantPlaced);
        EventBus.Unsubscribe<PlantHarvestedEvent>(OnPlantHarvested);
        EventBus.Unsubscribe<PlantGrownEvent>(OnPlantGrown);
        EventBus.Unsubscribe<PlantAnalyzedEvent>(OnPlantAnalyzed);
        EventBus.Unsubscribe<GenomeTransferredEvent>(OnBatteryUsed);
        EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Unsubscribe<RunEndedEvent>(OnRunEnded);
        EventBus.Unsubscribe<LabOpenedEvent>(OnLabOpened);
        EventBus.Unsubscribe<CardSelectedEvent>(OnCardSelected);
        EventBus.Unsubscribe<CardHoveredEvent>(OnCardHovered);
        EventBus.Subscribe<EffectAppliedEvent>(OnEffectApplied);
    }

    // ----- Public API для UI и Narrative -----
    public void PlaySfx(AudioData audio) => _sfx.Play(audio);
    public void PlayMusic(AudioData audio) => _music.Play(audio);
    public void StopMusic() => _music.Stop();
    public void CrossfadeMusic(AudioData audio, float duration = 1f) => _music.Crossfade(audio, duration);
    public void PlayVoice(AudioData audio) => _voice.Play(audio);
    public void StopVoice() => _voice.Stop();

    public void SetMasterVolume(float v) { /* реализация через плееры TODO: для общей громкости можно менять в настройках */ }
    public void SetMusicVolume(float v) => _music.SetVolume(v);
    public void SetSfxVolume(float v) => _sfx.SetVolume(v);
    public void SetVoiceVolume(float v) => _voice.SetVolume(v);

    // ----- Event handlers -----
    private void OnPlantPlaced(PlantPlacedEvent evt) => PlaySfx(_config.plantPlaced);
    private void OnPlantHarvested(PlantHarvestedEvent evt) => PlaySfx(_config.plantHarvest);
    private void OnPlantGrown(PlantGrownEvent evt) => PlaySfx(_config.plantGrown);
    private void OnPlantAnalyzed(PlantAnalyzedEvent evt) => PlaySfx(_config.analyzer);
    private void OnBatteryUsed(GenomeTransferredEvent evt) => PlaySfx(_config.centrifuge);
    private void OnDayEnded(DayEndedEvent evt) => PlaySfx(_config.dayEnd);
    private void OnCardSelected(CardSelectedEvent evt) => PlaySfx(_config.cardSelect);
    private void OnEffectApplied(EffectAppliedEvent evt) => PlaySfx(_config.modifierTriggered);
    private void OnCardHovered(CardHoveredEvent evt) => PlaySfx(_config.cardHover);

    private void OnRunStarted(RunStartedEvent evt)
    {
        CrossfadeMusic(_config.gameplayMusic, 1.5f);
    }

    private void OnRunEnded(RunEndedEvent evt)
    {
        CrossfadeMusic(_config.endRunMusic, 1.5f);
    }

    private void OnLabOpened(LabOpenedEvent evt)
    {
        CrossfadeMusic(_config.laboratoryMusic, 1f);
    }
}