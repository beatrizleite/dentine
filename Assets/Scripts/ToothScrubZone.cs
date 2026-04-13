using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ToothScrubZone : MonoBehaviour
{
    [Tooltip("World-units of scrubbing needed to clean this tooth")]
    public float scrubThreshold = 1.2f;

    [Tooltip("Foam sprites shown progressively: [0]=33% [1]=66% [2]=100%")]
    public Sprite[] foamSprites;

    // ── statics ───────────────────────────────────────────────────────────
    private static int s_total   = 0;
    private static int s_cleaned = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetStatics() { s_total = 0; s_cleaned = 0; }

    // ── per-zone ──────────────────────────────────────────────────────────
    private Collider2D     col;
    private SpriteRenderer foamSR;
    private float          accum     = 0f;
    private bool           cleaned   = false;
    private Vector3        lastPos;
    private bool           wasInside = false;
    private int            lastStep  = -1;

    void OnEnable()  { s_total++; }
    void OnDisable() { if (s_total > 0) s_total--; }

    void Awake()  { col = GetComponent<Collider2D>(); }
    void Start()  { BuildFoamChild(); }

    void BuildFoamChild()
    {
        var old = transform.Find("Foam");
        if (old != null) Destroy(old.gameObject);

        var foamGO = new GameObject("Foam");
        foamGO.transform.SetParent(transform, false);
        foamGO.transform.localPosition = Vector3.zero;

        foamSR = foamGO.AddComponent<SpriteRenderer>();
        foamSR.sortingOrder = 4;
        foamGO.SetActive(false);
    }

    // Scale foam UNIFORMLY so its HEIGHT matches the collider height.
    // Width follows the sprite's own aspect ratio — no distortion.
    void FitFoamToCollider(Sprite sp)
    {
        if (sp == null || foamSR == null) return;
        foamSR.sprite = sp;

        var box = col as BoxCollider2D;
        float targetH = box != null ? box.size.y : 0.6f;
        float sprH    = sp.bounds.size.y;
        if (sprH <= 0f) return;

        float scale = targetH / sprH;
        foamSR.transform.localScale = new Vector3(scale, scale, 1f);
    }

    void Update()
    {
        if (cleaned) return;

        bool pressing    = Input.GetMouseButton(0) || Input.touchCount > 0;
        Vector3 worldPos = GetPointerWorld();
        bool inside      = pressing && col != null && col.OverlapPoint(worldPos);

        if (inside)
        {
            if (wasInside)
            {
                float delta = Vector3.Distance(worldPos, lastPos);
                if (delta > 0.001f)
                {
                    accum += delta;
                    if (Level2Manager.instance != null) Level2Manager.instance.OnScrubActivity();
                    UpdateFoam();
                    if (accum >= scrubThreshold)
                        MarkCleaned();
                }
            }
            lastPos = worldPos;
        }
        wasInside = inside;
    }

    void UpdateFoam()
    {
        if (foamSprites == null || foamSprites.Length == 0) return;

        float pct  = Mathf.Clamp01(accum / scrubThreshold);
        int   step = Mathf.Min(Mathf.FloorToInt(pct * foamSprites.Length), foamSprites.Length - 1);
        if (step == lastStep) return;
        lastStep = step;

        Sprite sp = foamSprites[step];
        if (sp == null) return;

        FitFoamToCollider(sp);
        foamSR.gameObject.SetActive(true);
        Color c = foamSR.color;
        c.a = 0.85f + step * 0.075f;
        foamSR.color = c;
    }

    void MarkCleaned()
    {
        cleaned = true;
        if (foamSprites != null && foamSprites.Length > 0)
        {
            var sp = foamSprites[foamSprites.Length - 1];
            FitFoamToCollider(sp);
            foamSR.gameObject.SetActive(true);
            Color c = foamSR.color; c.a = 1f; foamSR.color = c;
        }
        s_cleaned++;
        float progress = s_total > 0 ? (float)s_cleaned / s_total : 1f;
        if (Level2Manager.instance != null)
            Level2Manager.instance.OnCleaningProgress(progress);
    }

    Vector3 GetPointerWorld()
    {
        Vector2 screen = Input.touchCount > 0
            ? (Vector2)Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;
        Vector3 w = Camera.main.ScreenToWorldPoint(screen);
        w.z = 0f;
        return w;
    }
}