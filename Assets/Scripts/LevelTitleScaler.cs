using UnityEngine;
using TMPro;

/// Keeps the level title centred and sized to fit the camera width on any aspect ratio.
/// Attach alongside a TextMeshPro component on the LevelTitle GameObject.
[RequireComponent(typeof(TextMeshPro))]
public class LevelTitleScaler : MonoBehaviour
{
    [Tooltip("Maximum font size in world units.")]
    public float fontSizeMax = 3.0f;

    [Tooltip("Minimum font size in world units — text shrinks no further.")]
    public float fontSizeMin = 1.2f;

    [Tooltip("Fraction of camera width the text may occupy (0–1).")]
    public float widthFraction = 0.90f;

    [Tooltip("Distance below the top edge of the camera in world units.")]
    public float topMargin = 0.65f;

    void Start()   => Fit();
    void OnEnable() => Fit();

    public void Fit()
    {
        var cam = Camera.main;
        if (cam == null) return;

        var tmp = GetComponent<TextMeshPro>();
        if (tmp == null) return;

        float camW   = cam.orthographicSize * 2f * cam.aspect;
        float maxW   = camW * widthFraction;

        // Set a bounding rect so TMP knows the available width
        var rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(maxW, fontSizeMax * 1.5f);

        // Enable auto-sizing within the allowed range
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin      = fontSizeMin;
        tmp.fontSizeMax      = fontSizeMax;
        tmp.enableWordWrapping = false;
        tmp.overflowMode     = TextOverflowModes.Truncate;

        // Reposition to top centre of camera
        float topY = cam.transform.position.y + cam.orthographicSize;
        transform.position = new Vector3(
            cam.transform.position.x,
            topY - topMargin,
            transform.position.z);
    }
}
