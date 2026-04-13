using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// Manages both scrub hints (arrows) and shake hint (phone icon).
/// Attach to the Level2Manager GameObject and wire references in Inspector.
public class Level2HintController : MonoBehaviour
{
    [Header("Scrub hint — arrows")]
    public GameObject scrubHintPanel;
    public float inactivityDelay = 2.5f;
    public float hintShowDuration = 3f;

    [Header("Shake hint — phone icon")]
    public GameObject shakeHintPanel;
    public RectTransform shakePhoneIcon;

    [Header("Shake animation")]
    public float shakeAngle     = 18f;
    public float shakeCycle     = 0.35f;
    public float shakeTranslate = 22f;

    // ── state ──────────────────────────────────────────────────────────────
    private float       lastScrubTime;
    private bool        scrubHintVisible  = false;
    private bool        scrubPhaseActive  = true;
    private Coroutine   scrubHintRoutine;
    private Coroutine   shakeAnimRoutine;

    // ── public API called by Level2Manager ────────────────────────────────

    /// Call every time a scrub action happens (finger moved over tooth).
    public void RegisterScrubActivity()
    {
        lastScrubTime = Time.time;
        if (scrubHintVisible) HideScrubHint();
    }

    /// Call when all teeth are done and rinse phase begins.
    public void BeginShakePhase()
    {
        scrubPhaseActive = false;
        HideScrubHint();
        if (scrubHintRoutine != null) StopCoroutine(scrubHintRoutine);

        if (shakeHintPanel != null) shakeHintPanel.SetActive(true);
        if (shakeAnimRoutine != null) StopCoroutine(shakeAnimRoutine);
        if (shakePhoneIcon  != null) shakeAnimRoutine = StartCoroutine(AnimateShakePhone());
    }

    /// Call when rinsing is complete.
    public void EndShakePhase()
    {
        if (shakeAnimRoutine != null) StopCoroutine(shakeAnimRoutine);
        if (shakeHintPanel   != null) shakeHintPanel.SetActive(false);
        if (shakePhoneIcon   != null)
        {
            shakePhoneIcon.anchoredPosition = Vector2.zero;
            shakePhoneIcon.localRotation    = Quaternion.identity;
        }
    }

    // ── Unity lifecycle ────────────────────────────────────────────────────

    void Start()
    {
        lastScrubTime = Time.time;
        if (scrubHintPanel  != null) scrubHintPanel.SetActive(false);
        if (shakeHintPanel  != null) shakeHintPanel.SetActive(false);
        scrubHintRoutine = StartCoroutine(ScrubInactivityWatcher());
    }

    // ── Coroutines ─────────────────────────────────────────────────────────

    IEnumerator ScrubInactivityWatcher()
    {
        bool running = true;
        while (running && scrubPhaseActive)
        {
            yield return new WaitForSeconds(0.5f);
            if (!scrubPhaseActive) break;

            float idle = Time.time - lastScrubTime;

            if (!scrubHintVisible && idle >= inactivityDelay)
            {
                ShowScrubHint();
                yield return new WaitForSeconds(hintShowDuration);
                HideScrubHint();
                // Reset timer so it doesn't immediately re-show
                lastScrubTime = Time.time;
            }
        }
    }

    void ShowScrubHint()
    {
        if (scrubHintPanel == null) return;
        scrubHintVisible = true;
        scrubHintPanel.SetActive(true);
    }

    void HideScrubHint()
    {
        if (scrubHintPanel == null) return;
        scrubHintVisible = false;
        scrubHintPanel.SetActive(false);
    }

    IEnumerator AnimateShakePhone()
    {
        if (shakePhoneIcon == null) yield break;
        Vector2 origin = shakePhoneIcon.anchoredPosition;
        bool running = true;
        while (running)
        {
            // Left swing
            yield return LerpShake(origin, -shakeTranslate, -shakeAngle, shakeCycle * 0.5f);
            // Right swing
            yield return LerpShake(origin,  shakeTranslate,  shakeAngle, shakeCycle);
            // Back to centre
            yield return LerpShake(origin,  0f,              0f,         shakeCycle * 0.5f);
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator LerpShake(Vector2 origin, float targetX, float targetRot, float duration)
    {
        if (shakePhoneIcon == null) yield break;
        Vector2 startPos = shakePhoneIcon.anchoredPosition;
        float   startRot = shakePhoneIcon.localEulerAngles.z;
        if (startRot > 180f) startRot -= 360f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(duration, 0.01f);
            float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            shakePhoneIcon.anchoredPosition = Vector2.Lerp(startPos,
                origin + new Vector2(targetX, 0f), e);
            shakePhoneIcon.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Lerp(startRot, targetRot, e));
            yield return null;
        }
    }
}