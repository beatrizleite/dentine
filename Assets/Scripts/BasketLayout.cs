using UnityEngine;

/// Repositions and scales the two baskets so they always fit
/// within a 4:3 screen (the minimum supported aspect ratio).
/// On wider screens the baskets spread out naturally.
public class BasketLayout : MonoBehaviour
{
    [Header("Baskets")]
    public Transform basketGood;
    public Transform basketBad;

    [Header("Layout")]
    [Tooltip("Target scale for the baskets")]
    public float basketScale = 0.40f;

    [Tooltip("Horizontal position: fraction of the 4:3 half-width from the camera centre. "
           + "Positive = right, negative = left.  "
           + "Basket centre stays this far from the camera centre.")]
    public float horizontalFrac = 0.78f;   // 0.78 * 6.67 = 5.2 from cam centre

    [Tooltip("Vertical offset from camera bottom, in world units")]
    public float fromBottom = 2.20f;

    // minimum aspect ratio to guarantee correct placement
    const float kMinAspect = 4f / 3f;

    void Start()   { Layout(); }
    void OnEnable(){ Layout(); }

    public void Layout()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float halfH = cam.orthographicSize;
        // Use the NARROWER of the actual aspect and 4:3 as the safe zone.
        // This means: even on a 4:3 device the baskets are inside the screen.
        float safeHalfW = halfH * Mathf.Min(cam.aspect, kMinAspect);

        float camX       = cam.transform.position.x;
        float camY       = cam.transform.position.y;
        float bottomY    = camY - halfH + fromBottom;

        // Scale baskets
        var scale = Vector3.one * basketScale;
        if (basketGood != null) basketGood.localScale = scale;
        if (basketBad  != null) basketBad.localScale  = scale;

        // Measure basket half-width at chosen scale
        float bHW = BasketHalfWidth(basketGood, basketScale);

        // Safe horizontal offsets — basket edge must stay inside safeHalfW
        float maxOffset = safeHalfW - bHW - 0.15f;   // 0.15 margin
        float offset    = Mathf.Min(safeHalfW * horizontalFrac, maxOffset);

        if (basketGood != null)
            basketGood.position = new Vector3(camX - offset, bottomY, 0f);
        if (basketBad != null)
            basketBad.position  = new Vector3(camX + offset, bottomY, 0f);
    }

    static float BasketHalfWidth(Transform basket, float scale)
    {
        if (basket == null) return 1.5f;
        var sr = basket.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return 1.5f;
        return sr.sprite.bounds.size.x * scale * 0.5f;
    }
}
