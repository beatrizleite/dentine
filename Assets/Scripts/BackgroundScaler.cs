using UnityEngine;

/// Scales a SpriteRenderer so it always covers the full camera view (cover mode).
/// Works like CSS background-size: cover — no letterboxing on any aspect ratio.
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    void Start()    => Scale();
    void OnEnable() => Scale();

    public void Scale()
    {
        var cam = Camera.main;
        var sr  = GetComponent<SpriteRenderer>();
        if (cam == null || sr == null || sr.sprite == null) return;

        float camH = cam.orthographicSize * 2f;
        float camW = camH * cam.aspect;

        // Reset scale to measure the sprite's natural world-unit size
        transform.localScale = Vector3.one;
        float sprW = sr.sprite.bounds.size.x;
        float sprH = sr.sprite.bounds.size.y;
        if (sprW <= 0f || sprH <= 0f) return;

        // Cover: take the larger scale so both axes fill the camera
        float s = Mathf.Max(camW / sprW, camH / sprH);
        transform.localScale = new Vector3(s, s, 1f);

        // Always keep centred on the camera
        transform.position = new Vector3(
            cam.transform.position.x,
            cam.transform.position.y,
            transform.position.z);
    }
}
