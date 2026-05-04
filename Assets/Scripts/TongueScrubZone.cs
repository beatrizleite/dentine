using UnityEngine;

/// Tongue scrub zone. Same distance-based mechanic as ToothScrubZone
/// but with a shorter required distance (tongue is smaller).
[RequireComponent(typeof(Collider2D))]
public class TongueScrubZone : MonoBehaviour
{
    [Header("Scrub Settings")]
    [Tooltip("Total brush travel (world units) required to clean this zone.")]
    public float requiredDistance = 3.5f;   // slightly easier than teeth

    [Tooltip("Minimum brush speed (units/sec) to count as scrubbing.")]
    public float minSpeed = 0.3f;

    public Sprite foamSprite;

    // ── statics ───────────────────────────────────────────────────────────
    static int s_total   = 0;
    static int s_cleaned = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetStatics() { s_total = 0; s_cleaned = 0; }

    // ── per-zone ──────────────────────────────────────────────────────────
    Collider2D     col;
    SpriteRenderer foamSR;
    float          distanceScrubbed = 0f;
    Vector3        lastBrushPos;
    bool           wasInside = false;
    bool           cleaned   = false;

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
        foamSR.sortingOrder = 5;
        foamSR.sprite = foamSprite;
        FitToCollider();
        SetAlpha(0f);
    }

    void FitToCollider()
    {
        if (foamSR == null || foamSR.sprite == null) return;
        var box = col as BoxCollider2D;
        float targetH = box != null ? box.size.y : 0.4f;
        float sprH    = foamSR.sprite.bounds.size.y;
        if (sprH <= 0f) return;
        foamSR.transform.localScale = Vector3.one * (targetH / sprH);
    }

    void Update()
    {
        if (cleaned) return;

        var brush = ToothbrushController.instance;
        if (brush == null || !brush.IsActive) { wasInside = false; return; }

        Vector3 headPos = brush.BrushHeadWorld;
        bool inside = BrushHeadInside(headPos);

        if (inside)
        {
            if (wasInside)
            {
                float moved = Vector3.Distance(headPos, lastBrushPos);
                float speed = moved / Time.deltaTime;
                if (speed >= minSpeed)
                    distanceScrubbed += moved;
            }

            SetAlpha(Mathf.Clamp01(distanceScrubbed / requiredDistance));
            if (Level2Manager.instance != null) Level2Manager.instance.OnScrubActivity();
            if (distanceScrubbed >= requiredDistance) MarkCleaned();
        }

        wasInside    = inside;
        lastBrushPos = headPos;
    }

    const float brushRadius = 0.35f;
    bool BrushHeadInside(Vector3 p)
    {
        var box = col as BoxCollider2D;
        if (box == null) return false;
        Vector2 wc = col.transform.TransformPoint(box.offset);
        Vector3 ls = col.transform.lossyScale;
        float hx = box.size.x * 0.5f * Mathf.Abs(ls.x) + brushRadius;
        float hy = box.size.y * 0.5f * Mathf.Abs(ls.y) + brushRadius;
        return p.x >= wc.x - hx && p.x <= wc.x + hx &&
               p.y >= wc.y - hy && p.y <= wc.y + hy;
    }

    void MarkCleaned()
    {
        cleaned = true;
        SetAlpha(1f);
        s_cleaned++;
        float prog = s_total > 0 ? (float)s_cleaned / s_total : 1f;
        if (Level2Manager.instance != null) Level2Manager.instance.OnTongueProgress(prog);
    }

    void SetAlpha(float a)
    {
        if (foamSR == null) return;
        Color c = foamSR.color; c.a = a; foamSR.color = c;
    }
}
