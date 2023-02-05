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

    void OnEnable()
    {
        EventManager.StartListening("sfx", OnSoundPlay);
    }
    void OnDisable()
    {
        EventManager.StopListening("sfx", OnSoundPlay);
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
        }
    }
}

public enum SfxNames
{
    PlayerAttackLands, PlayerAttackMiss, PlayerHit, PlayerDash, RatDashWindup, RatDash, RatBoulderBreak, RatLand, RatSpin, RatThrow
}