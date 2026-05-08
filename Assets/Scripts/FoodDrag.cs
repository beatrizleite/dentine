using UnityEngine;

/// Drag-and-drop food item for Level 1.
/// Supports both mouse (editor/PC) and single-touch (mobile).
/// Tags:  "Healthy"   → Target_Good
///        "Unhealthy" → Target_Bad
public class FoodDrag : MonoBehaviour
{
    /// Blocks all drag input while the tutorial is playing.
    public static bool tutorialActive;

    /// True while any food is being dragged (used by Level1GhostHint).
    public static bool anyDragActive;

    Vector3        _startPos;
    Vector3        _originalScale;
    Vector3        _dragOffset;
    bool           _isDragging;
    bool           _isLocked;
    SpriteRenderer _sr;

    void Start()
    {
        _startPos      = transform.position;
        _originalScale = transform.localScale;
        _sr            = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (_isLocked || tutorialActive) return;
        BeginDrag(PointerToWorld(Input.mousePosition));
    }

    void OnMouseDrag()
    {
        if (!_isDragging) return;
        MoveTo(PointerToWorld(Input.mousePosition));
    }

    void OnMouseUp()
    {
        if (!_isDragging) return;
        EndDrag();
    }

    void Update()
    {
        if (_isLocked || tutorialActive || Input.touchCount == 0) return;

        Touch touch     = Input.GetTouch(0);
        Vector3 worldPos = PointerToWorld(touch.position);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null && hit.gameObject == gameObject)
                    BeginDrag(worldPos);
                break;

            case TouchPhase.Moved:
                if (_isDragging) MoveTo(worldPos);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (_isDragging) EndDrag();
                break;
        }
    }

    void BeginDrag(Vector3 worldPos)
    {
        _isDragging    = true;
        anyDragActive  = true;
        _dragOffset    = transform.position - worldPos;
        transform.localScale = _originalScale * 1.2f;
    }

    void MoveTo(Vector3 worldPos)
    {
        transform.position = new Vector3(
            worldPos.x + _dragOffset.x,
            worldPos.y + _dragOffset.y,
            transform.position.z);
    }

    void EndDrag()
    {
        _isDragging   = false;
        anyDragActive = false;
        transform.localScale = _originalScale;
        CheckDrop();
    }

    void CheckDrop()
    {
        var hits = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.5f, 0.5f), 0f);

        foreach (var col in hits)
        {
            if (!col.isTrigger) continue;

            bool isHealthy   = CompareTag("Healthy");
            bool isGoodTarget = col.name == "Target_Good";
            bool isBadTarget  = col.name == "Target_Bad";

            if ((isHealthy && isGoodTarget) || (!isHealthy && isBadTarget))
            {
                OnCorrectDrop(col.transform.position);
                return;
            }

            if ((isHealthy && isBadTarget) || (!isHealthy && isGoodTarget))
            {
                OnWrongDrop();
                return;
            }
        }

        // Dropped in empty space — return to start
        transform.position = _startPos;
    }

    void OnCorrectDrop(Vector3 targetPos)
    {
        transform.position = targetPos;
        if (_sr != null) _sr.color = Color.green;
        Destroy(gameObject, 0.2f);
        Level1Manager.instance?.FoodDropped();
    }

    void OnWrongDrop()
    {
        _isLocked  = true;
        _isDragging = false;
        if (_sr != null) _sr.color = Color.red;
        Invoke(nameof(ResetToStart), 0.5f);
    }

    void ResetToStart()
    {
        transform.position = _startPos;
        if (_sr != null) _sr.color = Color.white;
        _isLocked = false;
    }

    static Vector3 PointerToWorld(Vector2 screenPos)
    {
        var p = Camera.main.ScreenToWorldPoint(screenPos);
        p.z = 0f;
        return p;
    }
}
