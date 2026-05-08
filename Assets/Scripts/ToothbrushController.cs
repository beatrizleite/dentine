/// Draggable toothbrush that follows the pointer/touch each frame.
/// The bristle tip (BrushHeadWorld) tracks the finger exactly — the handle trails behind.
/// Mirrors vertically when cleaning lower teeth or the tongue.
using UnityEngine;

public class ToothbrushController : MonoBehaviour
{
    public static ToothbrushController instance;

    [Header("Brush Head (local space)")]
    [Tooltip("Offset from pivot to bristle tip in local sprite units.")]
    public Vector2 headOffsetLocal = new Vector2(-8.5f, 0f);

    [Header("Feel")]
    [Range(0f, 0.9f)] public float lag = 0.08f;

    [Header("Flip Threshold")]
    [Tooltip("World Y below which the brush flips for lower teeth / tongue.")]
    public float flipYThreshold = -0.60f;

    Camera _cam;
    bool _pressing;
    Vector3 _smoothPos;

    public bool    IsActive      => _pressing;
    public Vector3 BrushHeadWorld =>
        transform.TransformPoint(new Vector3(headOffsetLocal.x, headOffsetLocal.y, 0f));

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

            // Offset pivot so the bristle tip lands exactly on the finger
            Vector3 worldOffset = transform.TransformPoint(
                new Vector3(headOffsetLocal.x, headOffsetLocal.y, 0f)) - transform.position;

            _smoothPos = Vector3.Lerp(w - worldOffset, _smoothPos, lag);
            transform.position = _smoothPos;
        }

        ApplyFlip();
    }

    // Flip the sprite Y so bristles point downward for lower teeth and tongue.
    // Only applied while pressing to avoid flipping when the brush is off-screen.
    void ApplyFlip()
    {
        if (!_pressing) return;

        bool faceDown = (Level2Manager.instance != null &&
                         Level2Manager.instance.CurrentPhase == Level2Manager.Phase.BrushTongue)
                     || transform.position.y < flipYThreshold;

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
