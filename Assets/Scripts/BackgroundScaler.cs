using UnityEngine;

/// Scales a SpriteRenderer to always cover the full camera view,
/// preserving the sprite's aspect ratio (cover mode = no letterboxing).
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    void Start()  { Scale(); }
    void OnEnable() { Scale(); }

    public void Scale()
    {
        var cam = Camera.main;
        if (cam == null) return;
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float camH = cam.orthographicSize * 2f;
        float camW = camH * cam.aspect;

        // Reset scale to measure sprite in world units
        transform.localScale = Vector3.one;
        float sprW = sr.sprite.bounds.size.x;
        float sprH = sr.sprite.bounds.size.y;

        // Cover mode: scale so both axes fill (take the larger scale)
        float scaleX = camW / sprW;
        float scaleY = camH / sprH;
        float s = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(s, s, 1f);
        // Centre on camera
        transform.position = new Vector3(cam.transform.position.x,
                                         cam.transform.position.y,
                                         transform.position.z);
    }
}