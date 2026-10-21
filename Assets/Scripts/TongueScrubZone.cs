using UnityEngine;

// Same mechanic as ToothScrubZone but for the tongue.
[RequireComponent(typeof(Collider2D))]
public class TongueScrubZone : MonoBehaviour
{
    public float requiredDistance = 2.0f;
    public float minSpeed = 0.25f;
    public Sprite foamSprite;

    static int s_total, s_cleaned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetStatics() { s_total = 0; s_cleaned = 0; }

    Collider2D col;
    SpriteRenderer foamSR;
    float distanceScrubbed;
    Vector3 lastBrushPos;
    bool wasInside;
    bool cleaned;

    void OnEnable()  { s_total++; }
    void OnDisable()
    {
        if (s_total > 0) s_total--;
        // Fim da cena: todas as zonas foram desativadas. Repor o contador de zonas limpas
        // para que repetir o nível na mesma sessão comece do zero.
        if (s_total == 0) s_cleaned = 0;
    }
    void Awake()     { col = GetComponent<Collider2D>(); }
    void Start()     { BuildFoamChild(); }

    void BuildFoamChild()
    {
        var old = transform.Find("Foam");
        if (old != null) Destroy(old.gameObject);
        if (foamSprite == null) return;

        var go = new GameObject("Foam");
        go.transform.SetParent(transform, false);
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
        float sprH = foamSR.sprite.bounds.size.y;
        if (sprH > 0f) foamSR.transform.localScale = Vector3.one * (targetH / sprH);
    }

    void Update()
    {
        if (cleaned) return;

        var brush = ToothbrushController.instance;
        if (brush == null || !brush.IsActive) { wasInside = false; return; }

        var headPos = brush.BrushHeadWorld;
        bool inside = BrushHeadInside(headPos);

        if (inside)
        {
            if (Level2Manager.instance?.hintManager != null &&
                Level2Manager.instance.hintManager.IsScrubLocked())
            {
                wasInside = inside;
                lastBrushPos = headPos;
                return;
            }

            if (wasInside)
            {
                float moved = Vector3.Distance(headPos, lastBrushPos);
                if (moved / Time.deltaTime >= minSpeed)
                    distanceScrubbed += moved;
            }

            SetAlpha(Mathf.Clamp01(distanceScrubbed / requiredDistance));
            Level2Manager.instance?.OnScrubActivity();
            if (distanceScrubbed >= requiredDistance) MarkCleaned();
        }

        wasInside = inside;
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
        Level2Manager.instance?.OnTongueProgress(prog);
    }

    void SetAlpha(float a)
    {
        if (foamSR == null) return;
        var c = foamSR.color;
        c.a = a;
        foamSR.color = c;
    }
}
