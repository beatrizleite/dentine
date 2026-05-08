using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HintManager : MonoBehaviour
{
    [Header("Ghost Finger Hint")]
    public ScrubGhostHint scrubGhostHint;
    public float scrubIdleTimeout    = 2.5f;
    public float scrubHintDisplayTime = 3f;

    [Header("Shake Hint (phone icon)")]
    public GameObject shakePhonePanel;
    public float shakeIdleTimeout    = 2.5f;
    public float shakeHintDisplayTime = 3f;

    [Header("Phone Shake Animation")]
    public float rotateAngle  = 22f;
    public float rotateSpeed  = 8f;
    public float bouncePixels = 16f;

    private float     scrubIdleTimer  = 0f;
    private float     shakeIdleTimer  = 0f;
    private bool      scrubHintActive = false;
    private bool      shakeHintActive = false;
    private bool      rinsePhase      = false;
    private Coroutine scrubHideCo;
    private Coroutine shakeHideCo;

    private Vector3[] currentScrubPath;

    private RectTransform phoneIconRT;
    private Vector2       phoneOrigin;

    void Start()
    {
        if (shakePhonePanel != null)
        {
            shakePhonePanel.SetActive(false);
            var t = shakePhonePanel.transform.Find("PhoneIcon");
            if (t != null)
            {
                phoneIconRT = t.GetComponent<RectTransform>();
                if (phoneIconRT != null) phoneOrigin = phoneIconRT.anchoredPosition;
            }
        }
        currentScrubPath = BuildTeethArcPath();
    }

    void Update()
    {
        if (!rinsePhase) TickScrub();
        else             TickShake();
        if (shakeHintActive && phoneIconRT != null) AnimatePhone();
    }

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

    public void ExitRinsePhase()
    {
        rinsePhase     = false;
        shakeIdleTimer = 0f;
        scrubIdleTimer = 0f;
        HideShakeHint();
        currentScrubPath = BuildTonguePath();
    }

    public void OnShakeActivity()
    {
        shakeIdleTimer = 0f;
        if (shakeHintActive) HideShakeHint();
    }

    void TickScrub()
    {
        if (scrubHintActive) return;
        scrubIdleTimer += Time.deltaTime;
        if (scrubIdleTimer >= scrubIdleTimeout) ShowScrubHint();
    }

    void ShowScrubHint()
    {
        if (scrubGhostHint == null || currentScrubPath == null) return;
        scrubHintActive = true;
        scrubGhostHint.Show(currentScrubPath);
        if (scrubHideCo != null) StopCoroutine(scrubHideCo);
        scrubHideCo = StartCoroutine(AutoHide(scrubHintDisplayTime, HideScrubHint));
    }

    void HideScrubHint()
    {
        scrubHintActive = false;
        scrubIdleTimer  = 0f;
        if (scrubGhostHint != null) scrubGhostHint.Hide();
        if (scrubHideCo != null) { StopCoroutine(scrubHideCo); scrubHideCo = null; }
    }

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

    /// Teeth path: two quadratic Bézier arcs (upper arch + lower arch).
    /// The finger sweeps left→right along the upper arch, then right→left
    /// along the lower arch, forming a continuous dental-arch motion.
    static Vector3[] BuildTeethArcPath()
    {
        const float z    = -0.5f;
        const int   segs = 10;   // points per arc

        // Gather zone data
        var zones = Object.FindObjectsByType<ToothScrubZone>(FindObjectsSortMode.None);

        float upperY = 0.28f, lowerY = -1.55f;   // fallback midpoints
        float leftX  = -2.3f, rightX =  2.3f;

        if (zones.Length > 0)
        {
            float sumUpper = 0f, sumLower = 0f;
            int   cntU = 0, cntL = 0;
            float minX = float.MaxValue, maxX = float.MinValue;

            foreach (var z2 in zones)
            {
                var p = z2.transform.position;
                // Tooth zone world half-width = box.size.x * 0.5 * lossyScale.x
                var box = z2.GetComponent<BoxCollider2D>();
                float hw = box != null
                    ? box.size.x * 0.5f * Mathf.Abs(z2.transform.lossyScale.x)
                    : 0.8f;

                minX = Mathf.Min(minX, p.x - hw);
                maxX = Mathf.Max(maxX, p.x + hw);

                if (p.y > -0.5f) { sumUpper += p.y; cntU++; }
                else             { sumLower += p.y; cntL++; }
            }
            if (cntU > 0) upperY = sumUpper / cntU;
            if (cntL > 0) lowerY = sumLower / cntL;
            leftX  = minX + 0.1f;
            rightX = maxX - 0.1f;
        }

        float midX = (leftX + rightX) * 0.5f;

        // Upper arch: left→right, control point raised above the ends
        // (teeth arch is convex upward — centre is higher than the corners)
        float upperCtrlBoost = 0.30f;
        var upper = QuadBezier(
            new Vector3(leftX,  upperY,           z),
            new Vector3(midX,   upperY + upperCtrlBoost, z),
            new Vector3(rightX, upperY,           z),
            segs);

        // Lower arch: right→left, control point pushed below the ends
        // (lower arch is convex downward — centre is lower than the corners)
        float lowerCtrlBoost = 0.30f;
        var lower = QuadBezier(
            new Vector3(rightX, lowerY,           z),
            new Vector3(midX,   lowerY - lowerCtrlBoost, z),
            new Vector3(leftX,  lowerY,           z),
            segs);

        // Concatenate: upper sweep → lower sweep
        var path = new Vector3[upper.Length + lower.Length];
        upper.CopyTo(path, 0);
        lower.CopyTo(path, upper.Length);
        return path;
    }

    /// Tongue path: diagonal Bézier across the tongue zones.
    static Vector3[] BuildTonguePath()
    {
        const float z    = -0.5f;
        const int   segs = 8;

        var zones = Object.FindObjectsByType<TongueScrubZone>(FindObjectsSortMode.None);
        if (zones.Length == 0)
        {
            return QuadBezier(
                new Vector3(-1.5f,  0.3f, z),
                new Vector3( 0.0f, -1.0f, z),
                new Vector3( 1.5f, -2.5f, z), segs);
        }

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (var tz in zones)
        {
            var p = tz.transform.position;
            minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
        }

        float midX = (minX + maxX) * 0.5f;
        float midY = (minY + maxY) * 0.5f;

        // Diagonal sweep top-left → mid → bottom-right
        return QuadBezier(
            new Vector3(minX,  maxY, z),
            new Vector3(midX,  midY, z),
            new Vector3(maxX,  minY, z), segs);
    }

    /// Evaluate a quadratic Bézier curve with `steps` equally-spaced points.
    static Vector3[] QuadBezier(Vector3 p0, Vector3 ctrl, Vector3 p2, int steps)
    {
        var pts = new Vector3[steps];
        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);
            float u = 1f - t;
            pts[i] = u * u * p0 + 2f * u * t * ctrl + t * t * p2;
        }
        return pts;
    }

    IEnumerator AutoHide(float delay, System.Action cb)
    { yield return new WaitForSeconds(delay); cb?.Invoke(); }
}
