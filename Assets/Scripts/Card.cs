using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Card : MonoBehaviour, IPointerClickHandler
{
    // ── data ──────────────────────────────────────────────────────────────
    public int  ToolId   { get; private set; }
    public bool IsUp     => state == State.FaceUp   || state == State.Matched;
    public bool IsLocked => state == State.Flipping || state == State.Matched;

    // ── visuals ───────────────────────────────────────────────────────────
    [HideInInspector] public SpriteRenderer backSR;
    [HideInInspector] public SpriteRenderer frontSR;
    [HideInInspector] public SpriteRenderer shadowSR;
    [HideInInspector] public SpriteRenderer glowSR;

    // ── config ────────────────────────────────────────────────────────────
    public float flipDuration   = 0.35f;
    public float shadowOffsetX  = 0.07f;
    public float shadowOffsetY  = -0.07f;

    // ── state ─────────────────────────────────────────────────────────────
    enum State { FaceDown, Flipping, FaceUp, Matched }
    State state = State.FaceDown;

    // ── public setup ──────────────────────────────────────────────────────
    public void Initialise(int toolId, Sprite front)
    {
        ToolId = toolId;
        frontSR.sprite  = front;
        frontSR.enabled = false;
        backSR.enabled  = true;
        glowSR.enabled  = false;
        state = State.FaceDown;
        transform.localScale = Vector3.one;
    }

    // ── flip ──────────────────────────────────────────────────────────────
    public void FlipUp(System.Action done = null)
    {
        if (state != State.FaceDown) { done?.Invoke(); return; }
        StartCoroutine(DoFlip(true, done));
    }

    public void FlipDown(System.Action done = null)
    {
        if (state != State.FaceUp) { done?.Invoke(); return; }
        StartCoroutine(DoFlip(false, done));
    }

    public void SetMatched()
    {
        state = State.Matched;
        glowSR.enabled = true;
        // Soft pulse
        StartCoroutine(PulseGlow());
    }

    // ── input ─────────────────────────────────────────────────────────────
    public void OnPointerClick(PointerEventData _)
    {
        if (IsLocked) return;
        MatchManager.instance?.OnCardClicked(this);
    }

    // ── coroutines ────────────────────────────────────────────────────────
    IEnumerator DoFlip(bool toFront, System.Action done)
    {
        state = State.Flipping;
        float half = flipDuration * 0.5f;

        // First half: collapse X (ease-in cubic)
        float t = 0f;
        Vector3 baseScale = new Vector3(1f, 1f, 1f);
        while (t < 1f)
        {
            t += Time.deltaTime / half;
            float e = t * t * t;                        // ease-in cubic
            float xScale = Mathf.Lerp(1f, 0f, Mathf.Clamp01(e));
            float yScale = Mathf.Lerp(1f, 0.92f, Mathf.Clamp01(t)); // slight squish = depth feel
            transform.localScale = new Vector3(xScale, yScale, 1f);
            shadowSR.transform.localScale = new Vector3(
                Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(t)), 1f, 1f);
            yield return null;
        }

        // Swap faces
        backSR.enabled  = !toFront;
        frontSR.enabled =  toFront;

        // Second half: expand X (ease-out cubic)
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / half;
            float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // ease-out cubic
            float xScale = Mathf.Lerp(0f, 1f, e);
            float yScale = Mathf.Lerp(0.92f, 1f, Mathf.Clamp01(t));
            transform.localScale = new Vector3(xScale, yScale, 1f);
            shadowSR.transform.localScale = new Vector3(
                Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(t)), 1f, 1f);
            yield return null;
        }

        transform.localScale = Vector3.one;
        shadowSR.transform.localScale = Vector3.one;
        state = toFront ? State.FaceUp : State.FaceDown;
        done?.Invoke();
    }

    IEnumerator PulseGlow()
    {
        float speed = 2.5f;
        while (state == State.Matched)
        {
            float a = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f * 0.55f;
            var c = glowSR.color; c.a = a; glowSR.color = c;
            yield return null;
        }
    }

    /// Initialise showing the front face immediately (no animation).
    public void InitialiseFaceUp(int toolId, Sprite front)
    {
        ToolId          = toolId;
        frontSR.sprite  = front;
        frontSR.enabled = true;
        backSR.enabled  = false;
        glowSR.enabled  = false;
        state           = State.FaceUp;
        transform.localScale = Vector3.one;
    }

    /// Instantly put face-down (used at start before intro flip wave).
    public void SetFaceDownImmediate()
    {
        frontSR.enabled = false;
        backSR.enabled  = true;
        state = State.FaceDown;
        transform.localScale = Vector3.one;
    }

    /// Animate card sliding to a new world position with a slight arc.
    public Coroutine SlideTo(Vector3 target, float duration, float arcHeight = 0.8f)
        => StartCoroutine(DoSlide(target, duration, arcHeight));

    IEnumerator DoSlide(Vector3 target, float duration, float arcHeight)
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(duration, 0.01f);
            float e  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            float up = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * arcHeight;
            transform.position = Vector3.Lerp(start, target, e) + Vector3.up * up;
            yield return null;
        }
        transform.position = target;
    }

}