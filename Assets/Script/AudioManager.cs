using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer & Sources")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private const string MASTER_KEY = "Master_Volume";
    private const string BGM_KEY = "BGM_Volume";
    private const string SFX_KEY = "SFX_Volume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Set default music volume
        SetMasterVolume(PlayerPrefs.GetFloat(MASTER_KEY, 0.8f));
        SetBGMVolume(PlayerPrefs.GetFloat(BGM_KEY, 0.8f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_KEY, 0.8f));
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

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}