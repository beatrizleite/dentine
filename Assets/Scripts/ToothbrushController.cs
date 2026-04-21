using UnityEngine;

/// Draggable toothbrush that follows the pointer/touch.
/// Scrub zones check this brush's head position instead of raw pointer.
public class ToothbrushController : MonoBehaviour
{
    public static ToothbrushController instance;

    [Header("Brush Head (local space)")]
    [Tooltip("Offset from pivot to bristle tip in local sprite units. Adjust in Inspector.")]
    public Vector2 headOffsetLocal = new Vector2(8f, 0f);

    [Header("Feel")]
    [Tooltip("0 = snap to finger, 1 = never moves. 0.06-0.12 feels natural.")]
    [Range(0f, 0.9f)] public float lag = 0.08f;

    // ── state ─────────────────────────────────────────────────────────────
    private Camera  mainCam;
    private bool    pressing;
    private Vector3 smoothPos;

    // ── public API ─────────────────────────────────────────────────────────
    /// Is the player currently touching / clicking?
    public bool IsActive => pressing;

    /// World-space position of the brush head (bristle tip).
    public Vector3 BrushHeadWorld =>
        transform.TransformPoint(new Vector3(headOffsetLocal.x, headOffsetLocal.y, 0f));

    // ── Unity ──────────────────────────────────────────────────────────────
    void Awake()
    {
        instance = this;
        mainCam  = Camera.main;
        smoothPos = transform.position;
    }

    void Update()
    {
        if (mainCam == null) mainCam = Camera.main;

        pressing = Input.GetMouseButton(0) || Input.touchCount > 0;

        if (pressing)
        {
            Vector2 screen = Input.touchCount > 0
                ? (Vector2)Input.GetTouch(0).position
                : (Vector2)Input.mousePosition;

            Vector3 w = mainCam.ScreenToWorldPoint(
                new Vector3(screen.x, screen.y, Mathf.Abs(mainCam.transform.position.z)));
            w.z = transform.position.z;

            // Smooth lag for natural feel; set lag=0 for instant snap
            smoothPos = Vector3.Lerp(w, smoothPos, lag);
            transform.position = smoothPos;
        }
    }

    // Editor helper — draw brush head gizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(BrushHeadWorld, 0.15f);
        Gizmos.DrawLine(transform.position, BrushHeadWorld);
    }
}
