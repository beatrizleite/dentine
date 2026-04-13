using UnityEngine;
using System.Collections;

public class HintManager : MonoBehaviour
{
    [Header("Scrub Hint (arrows + fog)")]
    public GameObject scrubHintPanel;
    public float scrubIdleTimeout    = 2.5f;
    public float scrubHintDisplayTime = 3f;

    [Header("Shake Hint (phone icon + fog)")]
    public GameObject shakePhonePanel;      // standalone panel, child of canvas
    public float shakeIdleTimeout     = 2.5f;
    public float shakeHintDisplayTime = 3f;

    [Header("Phone Shake Animation")]
    public float rotateAngle  = 22f;
    public float rotateSpeed  = 8f;
    public float bouncePixels = 16f;

    // ── state ─────────────────────────────────────────────────────────────
    private float     scrubIdleTimer   = 0f;
    private float     shakeIdleTimer   = 0f;
    private bool      scrubHintActive  = false;
    private bool      shakeHintActive  = false;
    private bool      rinsePhase       = false;
    private Coroutine scrubHideCo;
    private Coroutine shakeHideCo;

    // phone icon cached
    private RectTransform phoneIconRT;
    private Vector2       phoneOrigin;

    void Start()
    {
        if (scrubHintPanel  != null) scrubHintPanel.SetActive(false);
        if (shakePhonePanel != null)
        {
            shakePhonePanel.SetActive(false);
            // Cache the PhoneIcon child for animation
            var t = shakePhonePanel.transform.Find("PhoneIcon");
            if (t != null)
            {
                phoneIconRT = t.GetComponent<RectTransform>();
                if (phoneIconRT != null) phoneOrigin = phoneIconRT.anchoredPosition;
            }
        }
    }

    void Update()
    {
        if (!rinsePhase) TickScrub();
        else             TickShake();

        if (shakeHintActive && phoneIconRT != null)
            AnimatePhone();
    }

    // ── public API (called by Level2Manager) ─────────────────────────────
    public void OnScrubActivity()
    {
        scrubIdleTimer = 0f;
        if (scrubHintActive) HideScrubHint();
    }

    public void EnterRinsePhase()
    {
        rinsePhase = true;
        HideScrubHint();
        shakeIdleTimer = 0f;
    }

    public void OnShakeActivity()
    {
        shakeIdleTimer = 0f;
        if (shakeHintActive) HideShakeHint();
    }

    // ── scrub ─────────────────────────────────────────────────────────────
    void TickScrub()
    {
        if (scrubHintActive) return;
        scrubIdleTimer += Time.deltaTime;
        if (scrubIdleTimer >= scrubIdleTimeout) ShowScrubHint();
    }

    void ShowScrubHint()
    {
        if (scrubHintPanel == null) return;
        scrubHintActive = true;
        scrubHintPanel.SetActive(true);
        if (scrubHideCo != null) StopCoroutine(scrubHideCo);
        scrubHideCo = StartCoroutine(AutoHide(scrubHintDisplayTime, HideScrubHint));
    }

    void HideScrubHint()
    {
        scrubHintActive = false;
        scrubIdleTimer  = 0f;
        if (scrubHintPanel != null) scrubHintPanel.SetActive(false);
        if (scrubHideCo != null) { StopCoroutine(scrubHideCo); scrubHideCo = null; }
    }

    // ── shake ─────────────────────────────────────────────────────────────
    void TickShake()
    {
        if (shakeHintActive) return;
        shakeIdleTimer += Time.deltaTime;
        if (shakeIdleTimer >= shakeIdleTimeout) ShowShakeHint();
    }

    void ShowShakeHint()
    {
        if (shakePhonePanel == null) return;
        shakeHintActive = true;
        shakePhonePanel.SetActive(true);
        if (phoneIconRT != null) phoneOrigin = phoneIconRT.anchoredPosition;
        if (shakeHideCo != null) StopCoroutine(shakeHideCo);
        shakeHideCo = StartCoroutine(AutoHide(shakeHintDisplayTime, HideShakeHint));
    }

    void HideShakeHint()
    {
        shakeHintActive = false;
        shakeIdleTimer  = 0f;
        if (shakePhonePanel != null) shakePhonePanel.SetActive(false);
        if (phoneIconRT != null)
        {
            phoneIconRT.anchoredPosition = phoneOrigin;
            phoneIconRT.localRotation    = Quaternion.identity;
        }
        if (shakeHideCo != null) { StopCoroutine(shakeHideCo); shakeHideCo = null; }
    }

    void AnimatePhone()
    {
        float s = Mathf.Sin(Time.time * rotateSpeed);
        phoneIconRT.anchoredPosition = phoneOrigin + new Vector2(s * bouncePixels, 0f);
        phoneIconRT.localRotation    = Quaternion.Euler(0f, 0f, s * rotateAngle);
    }

    IEnumerator AutoHide(float delay, System.Action cb)
    {
        yield return new WaitForSeconds(delay);
        cb?.Invoke();
    }
}