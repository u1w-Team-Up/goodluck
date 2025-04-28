using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public sealed class AudioMixView : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField]
    private AudioMixer audioMixer;

    [Header("References")]
    [SerializeField]
    private AudioSource sfxTestSource;

    [SerializeField]
    private Slider bgmSlider;

    [SerializeField]
    private Slider seSlider;

    private const string KeyofBgmVolume = "KBV";
    private const string KeyofSfxVolume = "KSV";

    private void Awake()
    {
        SetBgmSlider();
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

        if (seSlider == null) return;

        audioMixer.GetFloat(AudioHelper.NAMESE, out float value);
        float sp = AudioHelper.DbToPacent(value);
        PlayerPrefs.GetFloat(KeyofSfxVolume, sp);
        seSlider.value = sp;

        seSlider.onValueChanged.AddListener(OnSEVolumeChanged);
    }

    public void SetBgmSlider()
    {
        audioMixer.GetFloat(AudioHelper.NAMEBGM, out float value);
        float bp = AudioHelper.DbToPacent(value);
        PlayerPrefs.GetFloat(KeyofBgmVolume, bp);
        bgmSlider.value = bp;
    }

    private void OnDestroy()
    {
        SaveVolume();
    }

    private void SaveVolume()
    {
        PlayerPrefs.SetFloat(KeyofBgmVolume, bgmSlider.value);

        if (seSlider != null)
        {
            PlayerPrefs.SetFloat(KeyofSfxVolume, seSlider.value);
        }

        PlayerPrefs.Save();
    }

    private void OnBGMVolumeChanged(float value)
    {
        audioMixer.SetFloat(AudioHelper.NAMEBGM, AudioHelper.PaToDb(value));

        SaveVolume();
    }

    private void OnSEVolumeChanged(float value)
    {
        audioMixer.SetFloat(AudioHelper.NAMESE, AudioHelper.PaToDb(value));

        const float delay = 0.2f;
        if (!sfxTestSource.isPlaying || sfxTestSource.time >= delay)
        {
            sfxTestSource.Play();
        }

        SaveVolume();
    }
    public void Save()
    {
        PlayerPrefs.Save();
    }
}