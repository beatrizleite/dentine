using UnityEngine;

// Positions the two baskets within a 4:3 safe zone so they fit on any device.
public class BasketLayout : MonoBehaviour
{
    public Transform basketGood;
    public Transform basketBad;
    public float basketScale    = 0.40f;
    public float horizontalFrac = 0.78f;
    public float fromBottom     = 2.20f;

    const float kMinAspect = 4f / 3f;

    void Start()    => Layout();
    void OnEnable() => Layout();

    public void Layout()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float halfH     = cam.orthographicSize;
        float safeHalfW = halfH * Mathf.Min(cam.aspect, kMinAspect);
        float bottomY   = cam.transform.position.y - halfH + fromBottom;
        float camX      = cam.transform.position.x;

        var scale = Vector3.one * basketScale;
        if (basketGood != null) basketGood.localScale = scale;
        if (basketBad  != null) basketBad.localScale  = scale;

        float bHW   = BasketHalfWidth(basketGood, basketScale);
        float offset = Mathf.Min(safeHalfW * horizontalFrac, safeHalfW - bHW - 0.15f);

        if (basketGood != null) basketGood.position = new Vector3(camX - offset, bottomY, 0f);
        if (basketBad  != null) basketBad.position  = new Vector3(camX + offset, bottomY, 0f);
    }

    static float BasketHalfWidth(Transform basket, float scale)
    {
        if (basket == null) return 1.5f;
        var sr = basket.GetComponent<SpriteRenderer>();
        return (sr != null && sr.sprite != null) ? sr.sprite.bounds.size.x * scale * 0.5f : 1.5f;
    }
}
