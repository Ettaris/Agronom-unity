using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Game/Audio Config")]
public class AudioConfig : ScriptableObject
{
    [Header("Volumes")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float voiceVolume = 1f;

    [Header("Music")]
    public AudioData mainTheme;
    public AudioData gameplayMusic;
    public AudioData laboratoryMusic;
    public AudioData endRunMusic;

    [Header("SFX")]
    public AudioData buttonClick;
    public AudioData journalPageSwitchSfx;
    public AudioData cardSelect;
    public AudioData cardHover;
    public AudioData plantPlaced;
    public AudioData plantHarvest;
    public AudioData plantGrown;
    public AudioData modifierTriggered;
    public AudioData fermentItemDropped;
    public AudioData analyzeDone;
    public AudioData labOpened;
    public AudioData batteryItemDropped;
    public AudioData centrifugeDone;
    public AudioData dayEnd;

    [Header("Voice")]
    public AudioData guardIntroduction;
    public AudioData guardTutorial;
    public AudioData guardStageComplete;

}