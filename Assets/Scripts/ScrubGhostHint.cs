using UnityEngine;
using System.Collections;

public class ScrubGhostHint : MonoBehaviour
{
    [Header("Path")]
    public Vector3[] waypoints;

    [Header("Animation")]
    public float sweepSpeed  = 4.5f;
    public float pauseAtTurn = 0.25f;
    public float fingerRadius = 0.28f;

    [Header("Appearance")]
    [Range(0f,1f)] public float maxAlpha  = 0.75f;
    public float fadeInTime  = 0.25f;
    public float fadeOutTime = 0.35f;

    private SpriteRenderer dotSR;
    private SpriteRenderer trailSR;
    private Coroutine animCo;
    private Coroutine fadeCo;
    private bool running;

    void Awake()
    {
        var trailGO = new GameObject("Trail");
        trailGO.transform.SetParent(transform, false);
        trailSR = trailGO.AddComponent<SpriteRenderer>();
        trailSR.sprite = MakeCircleSprite(64);
        trailSR.sortingLayerName = "Default";
        trailSR.sortingOrder = 19;
        trailSR.color = new Color(1,1,1,0);
        float ts = fingerRadius * 2.6f;
        trailGO.transform.localScale = new Vector3(ts, ts, 1f);

        var dotGO = new GameObject("Dot");
        dotGO.transform.SetParent(transform, false);
        dotSR = dotGO.AddComponent<SpriteRenderer>();
        dotSR.sprite = MakeCircleSprite(64);
        dotSR.sortingLayerName = "Default";
        dotSR.sortingOrder = 20;
        dotSR.color = new Color(1,1,1,0);
        float ds = fingerRadius * 2f;
        dotGO.transform.localScale = new Vector3(ds, ds, 1f);

        gameObject.SetActive(false);
    }

    public void Show(Vector3[] path)
    {
        if (path == null || path.Length < 2) return;
        waypoints = path;
        gameObject.SetActive(true);
        transform.position = waypoints[0];
        running = true;

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(AnimLoop());

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

    IEnumerator AnimLoop()
    {
        int idx = 0, dir = 1;
        while (running)
        {
            int next = idx + dir;
            if (next < 0 || next >= waypoints.Length)
            {
                yield return new WaitForSeconds(pauseAtTurn);
                dir = -dir;
                next = idx + dir;
                if (next < 0 || next >= waypoints.Length) yield break;
            }

            float dist = Vector3.Distance(transform.position, waypoints[next]);
            float dur  = Mathf.Max(dist / sweepSpeed, 0.05f);
            Vector3 from = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / dur;
                float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                transform.position = Vector3.Lerp(from, waypoints[next], e);
                trailSR.transform.position = Vector3.Lerp(
                    trailSR.transform.position, transform.position, Time.deltaTime * 12f);
                yield return null;
            }
            transform.position = waypoints[next];
            idx = next;
        }
    }

    IEnumerator FadeTo(float target, float duration)
    {
        float start = GetAlpha(), t = 0f;
        while (t < 1f) { t += Time.deltaTime / Mathf.Max(duration, 0.01f); SetAlpha(Mathf.Lerp(start, target, Mathf.Clamp01(t))); yield return null; }
        SetAlpha(target);
    }

    IEnumerator FadeOutAndDisable()
    { yield return FadeTo(0f, fadeOutTime); gameObject.SetActive(false); }

    void SetAlpha(float a)
    {
        if (dotSR   != null) { var c = dotSR.color;   c.a = a * 0.90f; dotSR.color   = c; }
        if (trailSR != null) { var c = trailSR.color; c.a = a * 0.30f; trailSR.color = c; }
    }
    float GetAlpha() => dotSR != null ? dotSR.color.a / 0.9f : 0f;

    static Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float cx = size * 0.5f, cy = size * 0.5f, r = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x-cx)*(x-cx)+(y-cy)*(y-cy));
                float a = Mathf.Clamp01(1f - d/r); a *= a;
                tex.SetPixel(x, y, new Color(1,1,1,a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
    }
}
