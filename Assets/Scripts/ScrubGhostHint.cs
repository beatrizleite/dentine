using UnityEngine;
using System.Collections;

public class ScrubGhostHint : MonoBehaviour
{
    public float sweepSpeed  = 3.5f;
    public float pauseAtTurn = 0.35f;
    public float fingerRadius = 0.55f;

    [Range(0f, 1f)] public float maxAlpha = 0.92f;
    public float fadeInTime  = 0.20f;
    public float fadeOutTime = 0.35f;

    // Fires once the hint has completed one full back-and-forth sweep.
    public event System.Action OnOneSweepDone;

    SpriteRenderer dotSR, glowSR;
    Coroutine animCo, fadeCo;
    bool running;

    void Awake()
    {
        var glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(transform, false);
        glowGO.transform.localScale = Vector3.one * (fingerRadius * 3.2f);
        glowSR = glowGO.AddComponent<SpriteRenderer>();
        glowSR.sprite = MakeCircleSprite(64);
        glowSR.sortingLayerName = "Default";
        glowSR.sortingOrder = 18;
        glowSR.color = new Color(0.4f, 0.8f, 1f, 0f);

        var dotGO = new GameObject("Dot");
        dotGO.transform.SetParent(transform, false);
        dotGO.transform.localScale = Vector3.one * (fingerRadius * 2f);
        dotSR = dotGO.AddComponent<SpriteRenderer>();
        dotSR.sprite = MakeCircleSprite(64);
        dotSR.sortingLayerName = "Default";
        dotSR.sortingOrder = 20;
        dotSR.color = new Color(1f, 1f, 1f, 0f);

        gameObject.SetActive(false);
    }

    public void Show(Vector3[] path)
    {
        if (path == null || path.Length < 2) return;
        gameObject.SetActive(true);
        transform.position = path[0];
        running = true;

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(AnimLoop(path));

        SetAlpha(0f);
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeTo(maxAlpha, fadeInTime));
    }

    public void Hide()
    {
        running = false;
        if (animCo != null) { StopCoroutine(animCo); animCo = null; }
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeOutAndDisable());
    }

    IEnumerator AnimLoop(Vector3[] waypoints)
    {
        int idx = 0;
        int dir = 1;
        bool firedOnce = false;
        int turnsHit = 0;

        while (running)
        {
            int next = idx + dir;

            if (next < 0 || next >= waypoints.Length)
            {
                yield return new WaitForSeconds(pauseAtTurn);
                turnsHit++;

                if (!firedOnce && turnsHit >= 2)
                {
                    firedOnce = true;
                    OnOneSweepDone?.Invoke();
                }

                dir  = -dir;
                next = idx + dir;
                if (next < 0 || next >= waypoints.Length) yield break;
            }

            float dist = Vector3.Distance(transform.position, waypoints[next]);
            float dur  = Mathf.Max(dist / sweepSpeed, 0.05f);
            Vector3 from = transform.position;

            for (float t = 0f; t < 1f;)
            {
                t += Time.deltaTime / dur;
                transform.position = Vector3.Lerp(from, waypoints[next], Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
                glowSR.transform.position = Vector3.Lerp(glowSR.transform.position, transform.position, Time.deltaTime * 10f);
                yield return null;
            }

            transform.position = waypoints[next];
            idx = next;
        }
    }

    IEnumerator FadeTo(float target, float duration)
    {
        float start = GetAlpha();
        for (float t = 0f; t < 1f;)
        {
            t += Time.deltaTime / Mathf.Max(duration, 0.01f);
            SetAlpha(Mathf.Lerp(start, target, Mathf.Clamp01(t)));
            yield return null;
        }
        SetAlpha(target);
    }

    IEnumerator FadeOutAndDisable()
    {
        yield return FadeTo(0f, fadeOutTime);
        gameObject.SetActive(false);
    }

    void SetAlpha(float a)
    {
        if (dotSR  != null) { var c = dotSR.color;  c.a = a;        dotSR.color  = c; }
        if (glowSR != null) { var c = glowSR.color; c.a = a * 0.45f; glowSR.color = c; }
    }

    float GetAlpha() => dotSR != null ? dotSR.color.a : 0f;

    static Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float cx = size * 0.5f;
        float r  = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
                float a = Mathf.Clamp01(1f - d / r);
                tex.SetPixel(x, y, new Color(1, 1, 1, a * a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
