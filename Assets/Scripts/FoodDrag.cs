using UnityEngine;

public class FoodDrag : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 originalScale;
    private bool isDragging = false;
    private bool isLocked = false;

    private SpriteRenderer mySprite;
    private Vector3 offset;

    void Start()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;
        mySprite = GetComponent<SpriteRenderer>();
    }

    // ======================
    // MOUSE (PC)
    // ======================
    void OnMouseDown()
    {
        if (isLocked) return;

        isDragging = true;

        Vector3 pointerWorldPos = GetPointerWorldPosition();
        offset = transform.position - pointerWorldPos;

        transform.localScale = originalScale * 1.2f;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 pointerWorldPos = GetPointerWorldPosition();
        transform.position = new Vector3(
            pointerWorldPos.x + offset.x,
            pointerWorldPos.y + offset.y,
            transform.position.z
        );
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;
        transform.localScale = originalScale;
        CheckDrop();
    }

    // ======================
    // TOUCH (MOBILE)
    // ======================
    void Update()
    {
        if (isLocked) return;
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        Vector3 touchWorldPos = GetPointerWorldPosition(touch.position);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                Collider2D hit = Physics2D.OverlapPoint(touchWorldPos);
                if (hit != null && hit.gameObject == gameObject)
                {
                    isDragging = true;
                    offset = transform.position - touchWorldPos;
                    transform.localScale = originalScale * 1.2f;
                }
                break;

            case TouchPhase.Moved:
                if (isDragging)
                {
                    transform.position = new Vector3(
                        touchWorldPos.x + offset.x,
                        touchWorldPos.y + offset.y,
                        transform.position.z
                    );
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging)
                {
                    isDragging = false;
                    transform.localScale = originalScale;
                    CheckDrop();
                }
                break;
        }
    }

    // ======================
    // HELPERS
    // ======================
    Vector3 GetPointerWorldPosition()
    {
        return GetPointerWorldPosition(Input.mousePosition);
    }

    Vector3 GetPointerWorldPosition(Vector2 screenPos)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPos);
        pos.z = 0;
        return pos;
    }

    // ======================
    // DROP LOGIC
    // ======================
    void CheckDrop()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            transform.position,
            new Vector2(0.5f, 0.5f),
            0
        );

        foreach (Collider2D hit in hits)
        {
            if (!hit.isTrigger) continue;

            // CORRECT
            if ((CompareTag("Healthy") && hit.name == "Target_Good") ||
                (CompareTag("Unhealthy") && hit.name == "Target_Bad"))
            {
                Success(hit.transform.position);
                return;
            }

            // WRONG
            if ((CompareTag("Healthy") && hit.name == "Target_Bad") ||
                (CompareTag("Unhealthy") && hit.name == "Target_Good"))
            {
                Error();
                return;
            }
        }

        // Drop no vazio
        transform.position = startPosition;
    }

    void Success(Vector3 targetPos)
    {
        transform.position = targetPos;

        Destroy(gameObject, 0.2f);

        if (mySprite != null)
            mySprite.color = Color.green;

        if (Level1Manager.instance != null)
        {
            Level1Manager.instance.FoodCorrect();
        }
    }

    void Error()
    {
        isLocked = true;
        isDragging = false;

        if (mySprite != null)
            mySprite.color = Color.red;

        Invoke(nameof(ResetToStart), 0.5f);
    }

    void ResetToStart()
    {
        transform.position = startPosition;

        if (mySprite != null)
            mySprite.color = Color.white;

        isLocked = false;
    }
}
