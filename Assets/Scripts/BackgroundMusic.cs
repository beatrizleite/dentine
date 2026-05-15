using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Persists across scene loads. Controls master volume via AudioListener.volume.
// Provides Duck/Unduck for narration — lowers BG music while speech plays.
public class BackgroundMusic : MonoBehaviour
{
    const string VolumeKey   = "masterVolume";
    const float  DefaultVolume = 0.6f;
    const float  DuckedVolume  = 0.06f;   // BG volume while narration is playing
    const float  DuckSpeed     = 3.5f;    // seconds to fade down/up

    static BackgroundMusic _instance;

    public Slider volumeSlider;

    AudioSource _src;
    Coroutine   _duckCo;
    float       _targetVolume = 1f;

    void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _src = GetComponent<AudioSource>();
        _src.volume = 1f;

        AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
    }

    void Start()
    {
        if (volumeSlider != null) WireSlider();
    }

    public static void RegisterSlider(Slider sl)
    {
        if (_instance == null) return;
        _instance.volumeSlider = sl;
        _instance.WireSlider();
    }

    // Call before playing any narration clip
    public static void Duck()
    {
        if (_instance == null) return;
        _instance.SetBGVolume(DuckedVolume);
    }

    // Call when narration finishes
    public static void Unduck()
    {
        if (_instance == null) return;
        _instance.SetBGVolume(1f);
    }

    void SetBGVolume(float target)
    {
        _targetVolume = target;
        if (_duckCo != null) StopCoroutine(_duckCo);
        _duckCo = StartCoroutine(FadeBG(target));
    }

    IEnumerator FadeBG(float target)
    {
        float start = _src.volume;
        for (float t = 0f; t < 1f;)
        {
            t += Time.deltaTime * DuckSpeed;
            _src.volume = Mathf.Lerp(start, target, Mathf.Clamp01(t));
            yield return null;
        }
        _src.volume = target;
    }

    void WireSlider()
    {
        volumeSlider.value = PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float val)
    {
        AudioListener.volume = val;
        PlayerPrefs.SetFloat(VolumeKey, val);
        PlayerPrefs.Save();
    }
}
