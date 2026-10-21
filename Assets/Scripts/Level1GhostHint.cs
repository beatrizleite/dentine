using UnityEngine;
using System.Collections;

public class Level1GhostHint : MonoBehaviour
{
    [Header("Targets")]
    public Transform targetGood;
    public Transform targetBad;

    [Header("Timing")]
    public float startDelay   = 0.8f;
    public float idleTimeout  = 3.5f;
    public float dragDuration = 0.9f;

    // refs built at Start
    Transform      fingerRoot;
    Transform      trailRoot;
    Transform      glowRoot;

    // Keep texture refs alive to prevent GC collecting them
    Texture2D _texCircle;
    Texture2D _texRing;

    bool  tutorialDone = false;
    bool  hinting      = false;
    float idleTimer    = 0f;
    Coroutine hintCo;

    void Start()
    {
        if (targetGood == null) targetGood = GameObject.Find("Target_Good")?.transform;
        if (targetBad  == null) targetBad  = GameObject.Find("Target_Bad")?.transform;

        BuildVisuals();                  // create GOs AND sprites here in Start
        FoodDrag.tutorialActive = true;
        StartCoroutine(RunTutorial());
    }

    void Update()
    {
        if (!tutorialDone) return;

        bool activity = FoodDrag.anyDragActive
            || Input.GetMouseButtonDown(0)
            || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (activity) { idleTimer = 0f; if (hinting) CancelHint(); return; }

        var arr = FindObjectsByType<FoodDrag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (arr.Length == 0) { idleTimer = 0f; return; }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTimeout && !hinting)
        {
            hinting = true;
            hintCo  = StartCoroutine(IdleHintLoop(arr[0]));
        }
    }

    void CancelHint()
    {
        hinting = false; idleTimer = 0f;
        if (hintCo != null) { StopCoroutine(hintCo); hintCo = null; }
        SetAlpha(0f);
        SetGlowActive(false);
    }

    IEnumerator RunTutorial()
    {
        yield return new WaitForSeconds(startDelay);

        // Wait up to 4 seconds for first food to spawn
        FoodDrag food = null;
        float elapsed = 0f;
        while (food == null && elapsed < 4f)
        {
            elapsed += Time.deltaTime;
            var arr = FindObjectsByType<FoodDrag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (arr.Length > 0) food = arr[0];
            yield return null;   // wait one frame
        }
        if (food == null) { Unlock(); yield break; }

        yield return StartCoroutine(SingleGesture(food));
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(SingleGesture(food));
        yield return new WaitForSeconds(0.2f);

        SetAlpha(0f);
        SetGlowActive(false);
        Unlock();
    }

    void Unlock() { FoodDrag.tutorialActive = false; tutorialDone = true; }

    IEnumerator IdleHintLoop(FoodDrag food)
    {
        while (hinting)
        {
            var arr = FindObjectsByType<FoodDrag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            food = arr.Length > 0 ? arr[0] : null;
            if (food == null) break;
            yield return StartCoroutine(SingleGesture(food));
            if (!hinting) break;
            yield return new WaitForSeconds(0.5f);
        }
        hinting = false;
    }

    // Alternate which basket we show so we don't reveal the answer
    int _gestureIndex = 0;

    IEnumerator SingleGesture(FoodDrag food)
    {
        if (food == null) yield break;
        Vector3 from = food.transform.position;

        // Alternate: show Good basket on even gestures, Bad on odd
        // This shows BOTH options without revealing the correct answer
        Transform tgt = (_gestureIndex % 2 == 0) ? targetGood : targetBad;
        _gestureIndex++;
        Vector3 to = tgt != null ? tgt.position : from + Vector3.down * 4f;

        // 1. Appear at food
        SetFingerPos(new Vector3(from.x, from.y - 0.5f, -1f));
        fingerRoot.localScale = Vector3.one;
        trailRoot.position    = fingerRoot.position;
        SetAlpha(0f);
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.28f; SetAlpha(Mathf.Clamp01(t)); yield return null; }

        // 2. Tap pulse
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.12f;
            float s = t < 0.5f ? Mathf.Lerp(1f, 1.45f, t*2f) : Mathf.Lerp(1.45f, 0.85f, (t-0.5f)*2f);
            fingerRoot.localScale = Vector3.one * s;
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.09f;
            fingerRoot.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, Mathf.Clamp01(t));
            yield return null;
        }
        fingerRoot.localScale = Vector3.one;

        // 3. Arc drag toward basket (white glow — neutral, no colour hint)
        SetGlowPos(to); SetGlowActive(true);
        float arcH = Vector3.Distance(from, to) * 0.16f;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(dragDuration, 0.01f);
            float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            float up = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * arcH;
            Vector3 p = Vector3.Lerp(from, to, e) + Vector3.up * up;
            SetFingerPos(new Vector3(p.x, p.y, -1f));
            trailRoot.position = Vector3.Lerp(trailRoot.position, fingerRoot.position, Time.deltaTime * 10f);
            yield return null;
        }
        SetFingerPos(new Vector3(to.x, to.y, -1f));

        // 4. Basket bounce
        if (tgt != null)
        {
            Vector3 orig = tgt.localScale;
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.28f;
                float s = t < 0.5f ? Mathf.Lerp(1f,1.22f,t*2f) : Mathf.Lerp(1.22f,1f,(t-0.5f)*2f);
                tgt.localScale = orig * s;
                yield return null;
            }
            tgt.localScale = orig;
        }

        // 5. Fade out
        t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.28f; SetAlpha(Mathf.Lerp(1f, 0f, Mathf.Clamp01(t))); yield return null; }
        SetAlpha(0f);
        SetGlowActive(false);
    }

    void SetFingerPos(Vector3 p) { if (fingerRoot != null) fingerRoot.position = p; }

    void SetAlpha(float a)
    {
        SetSRAlpha(fingerRoot?.GetComponentInChildren<SpriteRenderer>(), a * 0.94f);
        SetSRAlpha(trailRoot?.GetComponent<SpriteRenderer>(), a * 0.28f);
    }

    void SetGlowPos(Vector3 p)  { if (glowRoot != null) glowRoot.position = new Vector3(p.x, p.y, -0.5f); }
    void SetGlowActive(bool v)
    {
        if (glowRoot == null) return;
        glowRoot.gameObject.SetActive(v);
        if (v) StartCoroutine(PulseGlow());
    }

    IEnumerator PulseGlow()
    {
        var sr = glowRoot?.GetComponent<SpriteRenderer>();
        while (glowRoot != null && glowRoot.gameObject.activeSelf)
        {
            float a = 0.35f + Mathf.Sin(Time.time * 4.5f) * 0.18f;
            SetSRAlpha(sr, a);
            yield return null;
        }
    }

    static void SetSRAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null) return;
        var c = sr.color; c.a = a; sr.color = c;
    }

    void BuildVisuals()
    {
        var root = new GameObject("GhostHintRoot");
        root.transform.SetParent(transform, false);

        // Trail
        var tGO  = new GameObject("Trail");
        tGO.transform.SetParent(root.transform, false);
        var tSR  = tGO.AddComponent<SpriteRenderer>();
        tSR.sortingLayerName = "Default";
        tSR.sortingOrder     = 30;
        tSR.color            = new Color(1,1,1,0);
        _texCircle = MakeCircleTex(64);
        tSR.sprite = Sprite.Create(_texCircle, new Rect(0,0,64,64), Vector2.one*.5f, 64f);
        tGO.transform.localScale = Vector3.one * 0.70f;
        trailRoot = tGO.transform;

        // Finger (parent for scale)
        fingerRoot = new GameObject("Finger").transform;
        fingerRoot.SetParent(root.transform, false);
        var dGO  = new GameObject("Dot");
        dGO.transform.SetParent(fingerRoot, false);
        var dSR  = dGO.AddComponent<SpriteRenderer>();
        dSR.sortingLayerName = "Default";
        dSR.sortingOrder     = 31;
        dSR.color            = new Color(1,1,1,0);
        dSR.sprite = Sprite.Create(_texCircle, new Rect(0,0,64,64), Vector2.one*.5f, 64f);
        dGO.transform.localScale = Vector3.one * 0.48f;

        // Glow ring on basket
        var gGO  = new GameObject("TargetGlow");
        gGO.transform.SetParent(root.transform, false);
        var gSR  = gGO.AddComponent<SpriteRenderer>();
        gSR.sortingLayerName = "Default";
        gSR.sortingOrder     = 32;
        gSR.color            = new Color(1f, 1f, 1f, 0f);   // white = neutral, no colour hint
        _texRing = MakeRingTex(128);
        gSR.sprite = Sprite.Create(_texRing, new Rect(0,0,128,128), Vector2.one*.5f, 128f);
        gGO.transform.localScale = Vector3.one * 3.8f;
        gGO.SetActive(false);
        glowRoot = gGO.transform;
    }

    static Texture2D MakeCircleTex(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size * .5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Sqrt((x-c)*(x-c)+(y-c)*(y-c));
            float a = Mathf.Clamp01(1f - d/c); a *= a;
            tex.SetPixel(x, y, new Color(1,1,1,a));
        }
        tex.Apply();
        return tex;
    }

    static Texture2D MakeRingTex(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c = size*.5f, rO = size*.47f, rI = size*.33f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Sqrt((x-c)*(x-c)+(y-c)*(y-c));
            float a = (d>=rI&&d<=rO)
                ? Mathf.Clamp01((d-rI)/5f)*Mathf.Clamp01((rO-d)/5f)*5f : 0f;
            tex.SetPixel(x, y, new Color(1,1,1,a));
        }
        tex.Apply();
        return tex;
    }
}
