using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ShakeHint : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform phoneIcon;
    public GameObject arrowLeft;
    public GameObject arrowRight;
    public TextMeshProUGUI hintText;

    [Header("Animation")]
    public float bounceDistance = 40f;
    public float bounceSpeed = 3f;
    public float rotationAngle = 18f;
    public float arrowPulseSpeed = 2f;

    private Vector2 phoneOrigin;
    private bool animating = false;
    private Coroutine pulseRoutine;

    void Awake() { gameObject.SetActive(false); }

    void Start()
    {
        if (phoneIcon != null) phoneOrigin = phoneIcon.anchoredPosition;
        if (hintText != null) hintText.text = "Abana o telemovel!";
    }

    void Update()
    {
        if (!animating || phoneIcon == null) return;
        float t = Mathf.Sin(Time.time * bounceSpeed * Mathf.PI);
        phoneIcon.anchoredPosition = phoneOrigin + new Vector2(t * bounceDistance, 0f);
        phoneIcon.localRotation = Quaternion.Euler(0f, 0f, -t * rotationAngle);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        animating = true;
        if (arrowLeft != null) arrowLeft.SetActive(true);
        if (arrowRight != null) arrowRight.SetActive(true);
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseArrows());
    }

    public void Hide()
    {
        animating = false;
        if (pulseRoutine != null) { StopCoroutine(pulseRoutine); pulseRoutine = null; }
        gameObject.SetActive(false);
    }

    IEnumerator PulseArrows()
    {
        Image imgL = arrowLeft != null ? arrowLeft.GetComponent<Image>() : null;
        Image imgR = arrowRight != null ? arrowRight.GetComponent<Image>() : null;
        bool running = true;
        while (running)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * arrowPulseSpeed;
                float a = Mathf.PingPong(t, 1f);
                SetImageAlpha(imgL, a);
                SetImageAlpha(imgR, 1f - a);
                yield return null;
            }
        }
    }

    void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = Mathf.Clamp01(a); img.color = c;
    }
}