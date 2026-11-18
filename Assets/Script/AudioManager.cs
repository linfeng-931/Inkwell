using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum LoopChannel
{
    General,
    Footstep,
    Typing
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer & Sources")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    private const string MASTER_KEY = "Master_Volume";
    private const string BGM_KEY = "BGM_Volume";
    private const string SFX_KEY = "SFX_Volume";

    private readonly Dictionary<LoopChannel, AudioSource> loopSources = new Dictionary<LoopChannel, AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Set default music volume
        SetMasterVolume(PlayerPrefs.GetFloat(MASTER_KEY, 0.5f));
        SetBGMVolume(PlayerPrefs.GetFloat(BGM_KEY, 0.5f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_KEY, 0.5f));
    }

    public void SetMasterVolume(float sliderValue)
    {
        ApplyVolume(MASTER_KEY, sliderValue);
    }

    public void SetBGMVolume(float sliderValue)
    {
        ApplyVolume(BGM_KEY, sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        ApplyVolume(SFX_KEY, sliderValue);
    }

    private void ApplyVolume(string paramName, float sliderValue)
    {
        sliderValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float dB = Mathf.Log10(sliderValue) * 20f;
        masterMixer.SetFloat(paramName, dB);
        PlayerPrefs.SetFloat(paramName, sliderValue);
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null || bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip, int playType = 1)
    {
        if (clip == null) return;

        if (playType == 0)
        {
            PlayLoop(clip, LoopChannel.General);
        }
        else if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StopSFX()
    {
        StopLoop(LoopChannel.General);
    }

    public void PlayLoop(AudioClip clip, LoopChannel channel = LoopChannel.General)
    {
        if (clip == null) return;

        AudioSource source = GetLoopSource(channel);
        if (source.isPlaying && source.clip == clip) return;

        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    public void StopLoop(LoopChannel channel = LoopChannel.General)
    {
        if (loopSources.TryGetValue(channel, out AudioSource source) && source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    public bool IsLoopPlaying(LoopChannel channel)
    {
        return loopSources.TryGetValue(channel, out AudioSource source) && source != null && source.isPlaying;
    }

    private AudioSource GetLoopSource(LoopChannel channel)
    {
        if (loopSources.TryGetValue(channel, out AudioSource existing) && existing != null)
        {
            return existing;
        }

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        if (sfxSource != null) source.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;

        loopSources[channel] = source;
        return source;
    }

    public void PlayUISFX(AudioClip clip, bool stopPrevious = true)
    {
        if (clip == null || uiSource == null) return;

        if (stopPrevious)
        {
            uiSource.Stop();
            uiSource.clip = clip;
            uiSource.Play();
        }
        else
        {
            uiSource.PlayOneShot(clip);
        }
    }

    public void StopUISFX()
    {
        if (uiSource != null)
        {
            uiSource.Stop();
        }
    }
}