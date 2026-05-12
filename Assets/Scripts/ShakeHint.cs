using UnityEngine;
using UnityEngine.UI;

// Shows a bouncing phone icon to hint the player to shake the device.
public class ShakeHint : MonoBehaviour
{
    public RectTransform phoneIcon;

    public float bounceDistance = 50f;
    public float bounceSpeed    = 3f;
    public float rotationAngle  = 20f;

    Vector2 _origin;
    bool    _animating;

    void Awake() => gameObject.SetActive(false);

    void Start()
    {
        if (phoneIcon != null) _origin = phoneIcon.anchoredPosition;
    }

    void Update()
    {
        if (!_animating || phoneIcon == null) return;
        float t = Mathf.Sin(Time.time * bounceSpeed * Mathf.PI);
        phoneIcon.anchoredPosition = _origin + new Vector2(t * bounceDistance, 0f);
        phoneIcon.localRotation    = Quaternion.Euler(0f, 0f, -t * rotationAngle);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _animating = true;
    }

    public void Hide()
    {
        _animating = false;
        gameObject.SetActive(false);
    }
}
