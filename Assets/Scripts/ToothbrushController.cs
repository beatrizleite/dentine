/// Draggable toothbrush that follows the pointer/touch each frame.
/// Mirrors vertically when cleaning lower teeth or the tongue.
using UnityEngine;

public class ToothbrushController : MonoBehaviour
{
    public static ToothbrushController instance;

    [Header("Brush Head (local space)")]
    [Tooltip("Offset from pivot to bristle tip in local sprite units.")]
    public Vector2 headOffsetLocal = new Vector2(8f, 0f);

    [Header("Feel")]
    [Range(0f, 0.9f)] public float lag = 0.08f;

    [Header("Flip Threshold")]
    [Tooltip("World Y below which the brush flips (lower teeth / tongue).")]
    public float flipYThreshold = -0.60f;

    // ── state ─────────────────────────────────────────────────────────────
    Camera  _cam;
    bool    _pressing;
    Vector3 _smoothPos;

    // ── public API ────────────────────────────────────────────────────────
    public bool    IsActive      => _pressing;
    public Vector3 BrushHeadWorld =>
        transform.TransformPoint(new Vector3(headOffsetLocal.x, headOffsetLocal.y, 0f));

    // ── Unity ─────────────────────────────────────────────────────────────
    void Awake()
    {
        instance   = this;
        _cam       = Camera.main;
        _smoothPos = transform.position;
    }

    void Update()
    {
        if (_cam == null) _cam = Camera.main;

        _pressing = Input.GetMouseButton(0) || Input.touchCount > 0;

        if (_pressing)
        {
            Vector2 screen = Input.touchCount > 0
                ? (Vector2)Input.GetTouch(0).position
                : (Vector2)Input.mousePosition;

            Vector3 w = _cam.ScreenToWorldPoint(
                new Vector3(screen.x, screen.y, Mathf.Abs(_cam.transform.position.z)));
            w.z = transform.position.z;

            _smoothPos = Vector3.Lerp(w, _smoothPos, lag);
            transform.position = _smoothPos;
        }

        ApplyFlip();
    }

    /// Flip sprite vertically when actively scrubbing lower teeth or tongue.
    /// Only applies while the player is touching (IsActive) to avoid
    /// flipping while the brush is hidden off-screen.
    void ApplyFlip()
    {
        if (!_pressing) return;   // don't flip while not touching

        bool isTonguePhase = Level2Manager.instance != null &&
                             Level2Manager.instance.CurrentPhase == Level2Manager.Phase.BrushTongue;

        bool isLowerTeeth = transform.position.y < flipYThreshold;

        bool faceDown = isTonguePhase || isLowerTeeth;

        Vector3 s = transform.localScale;
        s.y = faceDown ? -Mathf.Abs(s.y) : Mathf.Abs(s.y);
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(BrushHeadWorld, 0.15f);
        Gizmos.DrawLine(transform.position, BrushHeadWorld);
    }
}
