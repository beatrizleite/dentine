using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// Animated "shake the phone" hint shown during the gargle phase.
/// Shows a bouncing phone icon with pulsing arrows.
public class ShakeHint : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform    phoneIcon;
    public GameObject       arrowLeft;
    public GameObject       arrowRight;
    public TextMeshProUGUI  hintText;

    [Header("Animation")]
    public float bounceDistance  = 40f;
    public float bounceSpeed     = 3f;
    public float rotationAngle   = 18f;
    public float arrowPulseSpeed = 2f;

    Vector2   _phoneOrigin;
    bool      _animating;
    Coroutine _pulseRoutine;

    // ── Unity ─────────────────────────────────────────────────────────────

    void Awake() => gameObject.SetActive(false);

    void Start()
    {
        if (phoneIcon != null) _phoneOrigin = phoneIcon.anchoredPosition;
        if (hintText  != null) hintText.text = "Shake the phone!";
    }

    void Update()
    {
        if (!_animating || phoneIcon == null) return;

        float t = Mathf.Sin(Time.time * bounceSpeed * Mathf.PI);
        phoneIcon.anchoredPosition = _phoneOrigin + new Vector2(t * bounceDistance, 0f);
        phoneIcon.localRotation    = Quaternion.Euler(0f, 0f, -t * rotationAngle);
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void Show()
    {
        gameObject.SetActive(true);
        _animating = true;
        if (arrowLeft  != null) arrowLeft.SetActive(true);
        if (arrowRight != null) arrowRight.SetActive(true);
        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        _pulseRoutine = StartCoroutine(PulseArrows());
    }

    public void Hide()
    {
        _animating = false;
        if (_pulseRoutine != null) { StopCoroutine(_pulseRoutine); _pulseRoutine = null; }
        gameObject.SetActive(false);
    }

    // ── Coroutines ────────────────────────────────────────────────────────

    IEnumerator PulseArrows()
    {
        var imgL = arrowLeft  != null ? arrowLeft.GetComponent<Image>()  : null;
        var imgR = arrowRight != null ? arrowRight.GetComponent<Image>() : null;

        while (_animating)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * arrowPulseSpeed;
                float a = Mathf.PingPong(t, 1f);
                SetAlpha(imgL, a);
                SetAlpha(imgR, 1f - a);
                yield return null;
            }
        }
    }

    static void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a       = Mathf.Clamp01(a);
        img.color = c;
    }
}
