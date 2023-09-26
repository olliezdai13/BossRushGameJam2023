using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioManager : MonoBehaviour
{
    private static AudioManager audioManager;

    [Header("SFX")]
    public AudioGroup _sfxPlayerAttackLands;
    public AudioGroup _sfxPlayerAttackMiss;
    public AudioGroup _sfxPlayerHit;
    public AudioGroup _sfxPlayerDash;
    public AudioGroup _sfxRatDashWindup;
    public AudioGroup _sfxRatDash;
    public AudioGroup _sfxRatBoulderBreak;
    public AudioGroup _sfxRatLand;
    public AudioGroup _sfxRatSpin;
    public AudioGroup _sfxRatThrow;
    public AudioGroup _sfxCatTeleportOut;
    public AudioGroup _sfxCatTeleportIn;
    public AudioGroup _sfxCatStrandSpawn;
    public AudioGroup _sfxCatStrandSpawnHard;
    public AudioGroup _sfxCatStrandAttack;
    public AudioGroup _sfxCatStrandAttackHard;
    public AudioGroup _sfxCatClawAttack;
    public AudioGroup _sfxRatWhistle;
    public AudioGroup _sfxKey;

    [Header("soundtrack")]
    public LoopAudio _none;
    public LoopAudio _sewer;
    public LoopAudio _ratFight;
    public LoopAudio _wind;
    public LoopAudio _catFight;
    public LoopAudio _lobby;


    void OnEnable()
    {
        EventManager.StartListening("sfx", OnSoundPlay);
        EventManager.StartListening("soundtrack", OnSoundtrack);
        EventManager.StartListening("stopSoundtrack", StopAllSoundtrack);
    }
    void OnDisable()
    {
        EventManager.StopListening("sfx", OnSoundPlay);
        EventManager.StopListening("soundtrack", OnSoundtrack);
        EventManager.StopListening("stopSoundtrack", StopAllSoundtrack);
    }

    public static AudioManager instance
    {
        get
        {
            if (!audioManager)
            {
                audioManager = FindObjectOfType(typeof(AudioManager)) as AudioManager;

                if (!audioManager)
                {
                    Debug.LogError("There needs to be one active EventManager script on a GameObject in your scene.");
                }
                else
                {
                    audioManager.Init();

                    //  Sets this to not be destroyed when reloading scene
                    DontDestroyOnLoad(audioManager);
                }
            }
            return audioManager;
        }
    }

    void Init()
    {
    }

    void OnSoundPlay(Dictionary<string, object> data)
    {
        SfxNames name = (SfxNames)data["name"];
        switch (name)
        {
            case SfxNames.PlayerAttackLands:
                _sfxPlayerAttackLands.PlayRandom();
                break;
            case SfxNames.PlayerAttackMiss:
                _sfxPlayerAttackMiss.PlayRandom();
                break;
            case SfxNames.PlayerHit:
                _sfxPlayerHit.PlayRandom();
                break;
            case SfxNames.PlayerDash:
                _sfxPlayerDash.PlayRandom();
                break;
            case SfxNames.RatDashWindup:
                _sfxRatDashWindup.PlayRandom();
                break;
            case SfxNames.RatDash:
                _sfxRatDash.PlayRandom();
                break;
            case SfxNames.RatBoulderBreak:
                _sfxRatBoulderBreak.PlayRandom();
                break;
            case SfxNames.RatLand:
                _sfxRatLand.PlayRandom();
                break;
            case SfxNames.RatSpin:
                _sfxRatSpin.PlayRandom();
                break;
            case SfxNames.RatThrow:
                _sfxRatThrow.PlayRandom();
                break;
            case SfxNames.CatTeleportOut:
                _sfxCatTeleportOut.PlayRandom();
                break;
            case SfxNames.CatTeleportIn:
                _sfxCatTeleportIn.PlayRandom();
                break;
            case SfxNames.CatStrandSpawn:
                _sfxCatStrandSpawn.PlayRandom();
                break;
            case SfxNames.CatStrandSpawnHard:
                _sfxCatStrandSpawnHard.PlayRandom();
                break;
            case SfxNames.CatStrandAttack:
                _sfxCatStrandAttack.PlayRandom();
                break;
            case SfxNames.CatStrandAttackHard:
                _sfxCatStrandAttackHard.PlayRandom();
                break;
            case SfxNames.CatClawAttack:
                _sfxCatClawAttack.PlayRandom();
                break;
            case SfxNames.RatWhistle:
                _sfxRatWhistle.PlayRandom();
                break;
            case SfxNames.Key:
                _sfxKey.PlayRandom();
                break;
        }
    }

    void OnSoundtrack(Dictionary<string, object> data)
    {
        SoundtrackNames name = (SoundtrackNames)data["name"];
        StopAllSoundtrack(new Dictionary<string, object> { });
        switch (name)
        {
            case SoundtrackNames.Sewer:
                if (_sewer) _sewer.Play();
                break;
            case SoundtrackNames.RatFight:
                if (_ratFight) _ratFight.Play();
                break;
            case SoundtrackNames.Wind:
                if (_wind) _wind.Play();
                break;
            case SoundtrackNames.CatFight:
                if (_catFight) _catFight.Play();
                break;
            case SoundtrackNames.Lobby:
                if (_lobby) _lobby.Play();
                break;
            case SoundtrackNames.None:
                if (_none) _none.Play();
                break;
        }
    }

    void StopAllSoundtrack(Dictionary<string, object> data)
    {
        if (_sewer) _sewer.Stop();
        if (_ratFight) _ratFight.Stop();
        if (_wind) _wind.Stop();
        if (_catFight) _catFight.Stop();
        if (_lobby) _lobby.Stop();
        if (_none) _none.Stop();
    }

}

public enum SfxNames
{
    PlayerAttackLands, PlayerAttackMiss, PlayerHit, PlayerDash, RatDashWindup, RatDash, RatBoulderBreak, RatLand, RatSpin, RatThrow, RatWhistle,
    CatTeleportOut, CatTeleportIn, CatStrandSpawn, CatStrandSpawnHard, CatStrandAttack, CatStrandAttackHard, CatClawAttack, Key
}
public enum SoundtrackNames
{
    None, Sewer, RatFight, Wind, CatFight, Lobby
}