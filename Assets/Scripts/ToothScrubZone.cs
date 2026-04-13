using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ToothScrubZone : MonoBehaviour
{
    [Header("Scrub Settings")]
    [Tooltip("Total seconds of pressing to fully clean this tooth")]
    public float scrubDuration = 1.2f;

    [Tooltip("The full foam sprite (foam_2) — fades in from 0 to 1 as you scrub")]
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
    private Camera         mainCam;

    void OnEnable()  { s_total++; }
    void OnDisable() { if (s_total > 0) s_total--; }

    void Awake()
    {
        col     = GetComponent<Collider2D>();
        mainCam = Camera.main;
    }

    void Start()
    {
        BuildFoamChild();
    }

    void BuildFoamChild()
    {
        var old = transform.Find("Foam");
        if (old != null) Destroy(old.gameObject);

        var foamGO = new GameObject("Foam");
        foamGO.transform.SetParent(transform, false);
        foamGO.transform.localPosition = Vector3.zero;

        foamSR = foamGO.AddComponent<SpriteRenderer>();
        foamSR.sortingOrder = 4;
        foamSR.sprite = foamSprite;

        // Size to collider height, preserve aspect ratio
        FitToCollider();

        // Start fully transparent but VISIBLE — fades in as user scrubs
        SetAlpha(0f);
        foamGO.SetActive(true);
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

        bool pressing    = Input.GetMouseButton(0) || Input.touchCount > 0;
        Vector3 worldPos = GetPointerWorld();
        bool inside      = pressing && col != null && col.OverlapPoint(worldPos);

        if (inside)
        {
            accumTime += Time.deltaTime;

            // Alpha goes from 0 → 1 linearly over scrubDuration
            float alpha = Mathf.Clamp01(accumTime / scrubDuration);
            SetAlpha(alpha);

            if (Level2Manager.instance != null)
                Level2Manager.instance.OnScrubActivity();

            if (accumTime >= scrubDuration)
                MarkCleaned();
        }
    }

    void MarkCleaned()
    {
        cleaned = true;
        SetAlpha(1f);

        s_cleaned++;
        float progress = s_total > 0 ? (float)s_cleaned / s_total : 1f;
        if (Level2Manager.instance != null)
            Level2Manager.instance.OnCleaningProgress(progress);
    }

    void SetAlpha(float a)
    {
        if (foamSR == null) return;
        Color c = foamSR.color;
        c.a = a;
        foamSR.color = c;
    }

    Vector3 GetPointerWorld()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return Vector3.zero;
        Vector2 screen = Input.touchCount > 0
            ? (Vector2)Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;
        Vector3 w = mainCam.ScreenToWorldPoint(
            new Vector3(screen.x, screen.y, Mathf.Abs(mainCam.transform.position.z)));
        w.z = 0f;
        return w;
    }
}