using UnityEngine;
using TMPro;

// Fits the level title to 90% of screen width at runtime.
// Uses world-space TMP to avoid CanvasScaler blur.
[RequireComponent(typeof(TextMeshPro))]
public class LevelTitleScaler : MonoBehaviour
{
    public float fontSizeMax    = 7.0f;
    public float fontSizeMin    = 3.0f;
    public float widthFraction  = 0.90f;
    public float topMargin      = 0.65f;

    void Start()    => Fit();
    void OnEnable() => Fit();

    public void Fit()
    {
        var cam = Camera.main;
        var tmp = GetComponent<TextMeshPro>();
        if (cam == null || tmp == null) return;

        float camW = cam.orthographicSize * 2f * cam.aspect;

        var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(camW * widthFraction, fontSizeMax * 1.5f);

        tmp.enableAutoSizing   = true;
        tmp.fontSizeMin        = fontSizeMin;
        tmp.fontSizeMax        = fontSizeMax;
        tmp.enableWordWrapping = false;
        tmp.overflowMode       = TextOverflowModes.Truncate;

        float topY = cam.transform.position.y + cam.orthographicSize;
        transform.position = new Vector3(cam.transform.position.x, topY - topMargin, transform.position.z);
    }
}
