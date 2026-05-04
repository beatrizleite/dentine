using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ToothScrubZone : MonoBehaviour
{
    [Header("Scrub Settings")]
    public float scrubDuration = 2.0f;
    public Sprite foamSprite;

    // ── statics ───────────────────────────────────────────────────────────
    private static int s_total   = 0;
    private static int s_cleaned = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetStatics() { s_total = 0; s_cleaned = 0; }

    // ── per-zone ──────────────────────────────────────────────────────────
    private Collider2D     col;
    private SpriteRenderer foamSR;
    private float          accumTime = 0f;
    private bool           cleaned   = false;

    void OnEnable()  { s_total++; }
    void OnDisable() { if (s_total > 0) s_total--; }

    void Awake() { col = GetComponent<Collider2D>(); }
    void Start() { BuildFoamChild(); }

    void BuildFoamChild()
    {
        var old = transform.Find("Foam");
        if (old != null) Destroy(old.gameObject);
        if (foamSprite == null) return;

        var go = new GameObject("Foam");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;

        foamSR = go.AddComponent<SpriteRenderer>();
        foamSR.sortingLayerName = "Default";
        foamSR.sortingOrder = 4;
        foamSR.sprite = foamSprite;
        FitToCollider();
        SetAlpha(0f);
    }

    void FitToCollider()
    {
        if (foamSR == null || foamSR.sprite == null) return;
        var box = col as BoxCollider2D;
        float targetH = box != null ? box.size.y : 0.6f;
        float sprH    = foamSR.sprite.bounds.size.y;
        if (sprH <= 0f) return;
        float s = targetH / sprH;
        foamSR.transform.localScale = new Vector3(s, s, 1f);
    }

    void Update()
    {
        if (cleaned) return;

        // Scrubbing only happens when the brush head is inside this zone
        var brush = ToothbrushController.instance;
        bool inside = brush != null && brush.IsActive && BrushHeadInside(brush.BrushHeadWorld);

        if (inside)
        {
            accumTime += Time.deltaTime;
            SetAlpha(Mathf.Clamp01(accumTime / scrubDuration));
            if (Level2Manager.instance != null) Level2Manager.instance.OnScrubActivity();
            if (accumTime >= scrubDuration) MarkCleaned();
        }
    }

    // Direct transform-based AABB — no Physics2D latency
    bool BrushHeadInside(Vector3 p)
    {
        var box = col as BoxCollider2D;
        if (box == null) return false;
        Vector2 wc = transform.TransformPoint(box.offset);
        Vector3 ls = transform.lossyScale;
        float hx = box.size.x * 0.5f * Mathf.Abs(ls.x);
        float hy = box.size.y * 0.5f * Mathf.Abs(ls.y);
        return p.x >= wc.x - hx && p.x <= wc.x + hx &&
               p.y >= wc.y - hy && p.y <= wc.y + hy;
    }

    void MarkCleaned()
    {
        cleaned = true; SetAlpha(1f);
        s_cleaned++;
        float prog = s_total > 0 ? (float)s_cleaned / s_total : 1f;
        if (Level2Manager.instance != null) Level2Manager.instance.OnCleaningProgress(prog);
    }

    void SetAlpha(float a)
    {
        if (foamSR == null) return;
        Color c = foamSR.color; c.a = a; foamSR.color = c;
    }
}
