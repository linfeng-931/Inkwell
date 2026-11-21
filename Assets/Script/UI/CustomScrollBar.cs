using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class CustomScrollBar : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider globalSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private TextMeshProUGUI globalSliderValue;
    [SerializeField] private TextMeshProUGUI bgmSliderValue;
    [SerializeField] private TextMeshProUGUI sfxSliderValue;

    private const string GLOBAL_PREF_KEY = "GlobalVolumeValue";
    private const string BGM_PREF_KEY = "BgmVolumeValue";
    private const string SFX_PREF_KEY = "SfxVolumeValue";

    public float DEFAULT_VOLUME_VALUE = 10f;

    void Start()
    {
        if (audioMixer != null && globalSlider != null && bgmSlider != null && sfxSlider != null)
        {
            InitAudio();
        }
    }

    public void InitAudio()
    {
        SetupSlider(globalSlider, GLOBAL_PREF_KEY, SetGlobal);
        SetupSlider(bgmSlider, BGM_PREF_KEY, SetBGMVolume);
        SetupSlider(sfxSlider, SFX_PREF_KEY, SetSFXVolume);
    }

    private void SetupSlider(Slider slider, string prefKey, UnityEngine.Events.UnityAction<float> callback)
    {
        slider.onValueChanged.RemoveListener(callback);

        float savedValue = PlayerPrefs.GetFloat(prefKey, DEFAULT_VOLUME_VALUE);
        slider.value = savedValue;
        callback.Invoke(savedValue);

        slider.onValueChanged.AddListener(callback);
    }

    private void SetVolume(string prefKey, float value, TextMeshProUGUI valueText, params string[] mixerParams)
    {
        PlayerPrefs.SetFloat(prefKey, value);
        PlayerPrefs.Save();

        if (valueText != null)
        {
            valueText.text = Mathf.RoundToInt(value).ToString();
        }

        float volume = Mathf.Log10(Mathf.Clamp(value * 0.1f, 0.0001f, 1f)) * 20f;
        foreach (string param in mixerParams)
        {
            audioMixer.SetFloat(param, volume);
        }
    }

    private void SetGlobal(float value) => SetVolume(GLOBAL_PREF_KEY, value, globalSliderValue, "Master_Volume");
    private void SetBGMVolume(float value) => SetVolume(BGM_PREF_KEY, value, bgmSliderValue, "BGM_Volume");
    private void SetSFXVolume(float value) => SetVolume(SFX_PREF_KEY, value, sfxSliderValue, "SFX_Volume", "UI_Volume");

    private void OnDestroy()
    {
        if (globalSlider != null) globalSlider.onValueChanged.RemoveListener(SetGlobal);
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}