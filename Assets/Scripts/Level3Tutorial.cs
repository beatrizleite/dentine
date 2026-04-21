using UnityEngine;
using System.Collections;

/// Purely visual tutorial for the memory match game.
/// Shows a ghost finger: tap card → flip → tap matching card → match glow.
/// Any touch dismisses it and unlocks the real game.
public class Level3Tutorial : MonoBehaviour
{
    [Header("Timing")]
    public float moveSpeed   = 4.5f;   // finger units/sec
    public float postFlipPause   = 0.45f;
    public float matchHoldPause  = 1.6f;
    public float resetPause      = 0.70f;
    public float loopsBeforeHint = 1;   // after 1 loop show a subtle 'tap to skip' pulse

    // ── runtime objects ───────────────────────────────────────────────────
    Transform   demoRoot;          // parent for all tutorial GOs
    Transform   cardARoot, cardBRoot;
    SpriteRenderer cardABack, cardBBack;
    SpriteRenderer cardAFront, cardBFront;
    SpriteRenderer glowA, glowB;
    Transform   fingerRoot;
    SpriteRenderer fingerSR, trailSR;
    SpriteRenderer dimSR;

    bool dismissed   = false;
    bool canDismiss  = false;

    // ── card layout ───────────────────────────────────────────────────────
    const float CARD  = 1.9f;   // demo card size (world units)
    const float GAP   = 0.55f;
    const float Z_DIM = 0.5f;   // z-depth behind tutorial cards
    const float Z_CARD= 0.0f;
    const float Z_FX  = -0.1f;
    const float Z_FNG = -0.3f;

    // ── Unity ─────────────────────────────────────────────────────────────
    void Start()
    {
        BuildVisuals();
        StartCoroutine(TutorialLoop());
    }

    void Update()
    {
        if (!canDismiss || dismissed) return;
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            StartCoroutine(Dismiss());
    }

    // ── setup ─────────────────────────────────────────────────────────────
    void BuildVisuals()
    {
        demoRoot = new GameObject("TutorialRoot").transform;
        demoRoot.SetParent(transform, false);

        // Dark overlay covering everything behind tutorial
        var dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(demoRoot, false);
        dimGO.transform.localPosition = new Vector3(0, 0, Z_DIM);
        dimSR = dimGO.AddComponent<SpriteRenderer>();
        dimSR.sprite       = MakeSolid(4, 4, new Color(0, 0, 0, 0));
        dimSR.sortingOrder = 50;
        dimGO.transform.localScale = new Vector3(30, 30, 1);   // covers full screen

        // Two demo cards side by side, centred on screen
        float leftX  = -(CARD + GAP) * 0.5f;
        float rightX =  (CARD + GAP) * 0.5f;
        float cardY  = 0.2f;

        cardARoot = BuildDemoCard(new Vector3(leftX,  cardY, Z_CARD), out cardABack, out cardAFront, out glowA);
        cardBRoot = BuildDemoCard(new Vector3(rightX, cardY, Z_CARD), out cardBBack, out cardBFront, out glowB);

        // Same front colour for both (they're a pair)
        var pairColor = new Color(0.22f, 0.72f, 0.54f);  // teal-green
        cardAFront.color = pairColor;
        cardBFront.color = pairColor;

        // Ghost finger
        BuildFinger();

        SetAlpha(dimSR, 0f);
    }

    Transform BuildDemoCard(Vector3 pos,
        out SpriteRenderer backSR, out SpriteRenderer frontSR, out SpriteRenderer glowSR)
    {
        var root = new GameObject("DemoCard").transform;
        root.SetParent(demoRoot, false);
        root.localPosition = pos;

        // Shadow
        var shGO = new GameObject("Shadow");
        shGO.transform.SetParent(root, false);
        shGO.transform.localPosition = new Vector3(0.06f, -0.06f, 0.05f);
        var shSR = shGO.AddComponent<SpriteRenderer>();
        shSR.sprite       = MakeSolid(256, 256, new Color(0,0,0,0.4f));
        shSR.sortingOrder = 51;
        shGO.transform.localScale = new Vector3(CARD, CARD, 1);

        // Back face
        var backGO = new GameObject("Back");
        backGO.transform.SetParent(root, false);
        backSR = backGO.AddComponent<SpriteRenderer>();
        backSR.sprite       = MakeCardBack(256);
        backSR.sortingOrder = 52;
        backGO.transform.localScale = new Vector3(CARD, CARD, 1);

        // Front face (starts hidden)
        var frontGO = new GameObject("Front");
        frontGO.transform.SetParent(root, false);
        frontSR = frontGO.AddComponent<SpriteRenderer>();
        frontSR.sprite       = MakeSolid(256, 256, Color.white);
        frontSR.sortingOrder = 52;
        frontSR.enabled      = false;
        frontGO.transform.localScale = new Vector3(CARD * 0.82f, CARD * 0.82f, 1);

        // Draw a simple tool icon on the front (a circle = placeholder)
        DrawCircleOnSprite(frontSR);

        // Glow (match effect)
        var glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(root, false);
        glowGO.transform.localPosition = new Vector3(0, 0, -0.02f);
        glowSR = glowGO.AddComponent<SpriteRenderer>();
        glowSR.sprite       = MakeSolid(256, 256, new Color(1f, 0.85f, 0.2f, 0));
        glowSR.sortingOrder = 53;
        glowSR.enabled      = false;
        glowGO.transform.localScale = new Vector3(CARD * 1.28f, CARD * 1.28f, 1);

        return root;
    }

    void BuildFinger()
    {
        fingerRoot = new GameObject("Finger").transform;
        fingerRoot.SetParent(demoRoot, false);
        fingerRoot.localPosition = new Vector3(0, -3.5f, Z_FNG);

        var trailGO = new GameObject("Trail");
        trailGO.transform.SetParent(fingerRoot, false);
        trailSR = trailGO.AddComponent<SpriteRenderer>();
        trailSR.sprite       = MakeCircle(64);
        trailSR.sortingOrder = 60;
        trailSR.color        = new Color(1, 1, 1, 0);
        float ts = 0.55f; trailGO.transform.localScale = new Vector3(ts, ts, 1);

        var dotGO = new GameObject("Dot");
        dotGO.transform.SetParent(fingerRoot, false);
        fingerSR = dotGO.AddComponent<SpriteRenderer>();
        fingerSR.sprite       = MakeCircle(64);
        fingerSR.sortingOrder = 61;
        fingerSR.color        = new Color(1, 1, 1, 0);
        float ds = 0.34f; dotGO.transform.localScale = new Vector3(ds, ds, 1);
    }

    // ── tutorial loop ─────────────────────────────────────────────────────
    IEnumerator TutorialLoop()
    {
        // Fade in dim overlay
        yield return FadeValue(v => SetAlpha(dimSR, v), 0f, 0.72f, 0.35f);

        // Bounce cards in
        yield return BounceIn(cardARoot, 0f);
        yield return BounceIn(cardBRoot, 0.12f);

        yield return new WaitForSeconds(0.5f);

        int loop = 0;
        while (!dismissed)
        {
            // --- Step 1: finger moves to card A, taps, A flips up ---
            yield return MoveFinger(cardARoot.position);
            yield return TapAnim();
            yield return FlipCard(cardARoot, cardABack, cardAFront, true);
            yield return new WaitForSeconds(postFlipPause);

            // --- Step 2: finger moves to card B, taps, B flips up ---
            yield return MoveFinger(cardBRoot.position);
            yield return TapAnim();
            yield return FlipCard(cardBRoot, cardBBack, cardBFront, true);
            yield return new WaitForSeconds(0.25f);

            // --- Step 3: match glow ---
            yield return ShowMatch();
            yield return new WaitForSeconds(matchHoldPause);

            // --- Step 4: hide glow, flip both back ---
            yield return HideMatch();
            yield return FlipCard(cardARoot, cardABack, cardAFront, false);
            yield return new WaitForSeconds(0.1f);
            yield return FlipCard(cardBRoot, cardBBack, cardBFront, false);
            SetFingerAlpha(0f);
            yield return new WaitForSeconds(resetPause);

            loop++;
            if (loop >= loopsBeforeHint) canDismiss = true;
        }
    }

    // ── helpers ───────────────────────────────────────────────────────────
    IEnumerator BounceIn(Transform card, float delay)
    {
        yield return new WaitForSeconds(delay);
        card.localScale = Vector3.zero;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.35f;
            float e = Mathf.Clamp01(t);
            // Overshoot spring
            float s = e < 0.7f
                ? Mathf.Lerp(0, 1.15f, e / 0.7f)
                : Mathf.Lerp(1.15f, 1.0f, (e - 0.7f) / 0.3f);
            card.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        card.localScale = Vector3.one;
    }

    IEnumerator MoveFinger(Vector3 target)
    {
        target.z = Z_FNG;
        // Appear if not already visible
        SetFingerAlpha(0.85f);
        trailSR.transform.position = fingerRoot.position;

        while (Vector3.Distance(fingerRoot.position, target) > 0.02f)
        {
            fingerRoot.position = Vector3.MoveTowards(
                fingerRoot.position, target, moveSpeed * Time.deltaTime);
            trailSR.transform.position = Vector3.Lerp(
                trailSR.transform.position, fingerRoot.position, Time.deltaTime * 10f);
            yield return null;
        }
        fingerRoot.position = target;
    }

    IEnumerator TapAnim()
    {
        // Scale up then down = press
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.12f;
            float s = t < 0.5f ? Mathf.Lerp(1f, 1.35f, t * 2f)
                                : Mathf.Lerp(1.35f, 0.85f, (t - 0.5f) * 2f);
            fingerRoot.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        // Release
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.10f;
            float s = Mathf.Lerp(0.85f, 1f, Mathf.Clamp01(t));
            fingerRoot.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        fingerRoot.localScale = Vector3.one;
    }

    IEnumerator FlipCard(Transform root, SpriteRenderer back, SpriteRenderer front, bool toFront)
    {
        float half = 0.22f;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / half;
            float e = t * t * t;
            root.localScale = new Vector3(Mathf.Lerp(1, 0, Mathf.Clamp01(e)), 1, 1);
            yield return null;
        }
        back.enabled  = !toFront;
        front.enabled =  toFront;
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / half;
            float e = 1 - Mathf.Pow(1 - Mathf.Clamp01(t), 3);
            root.localScale = new Vector3(Mathf.Lerp(0, 1, e), 1, 1);
            yield return null;
        }
        root.localScale = Vector3.one;
    }

    IEnumerator ShowMatch()
    {
        glowA.enabled = glowB.enabled = true;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.3f;
            float a = Mathf.Lerp(0, 0.65f, Mathf.Clamp01(t));
            SetAlpha(glowA, a); SetAlpha(glowB, a);
            // Slight bounce on matched cards
            float s = 1f + Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * 0.08f;
            cardARoot.localScale = new Vector3(s, s, 1);
            cardBRoot.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        cardARoot.localScale = cardBRoot.localScale = Vector3.one;
    }

    IEnumerator HideMatch()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.25f;
            float a = Mathf.Lerp(0.65f, 0, Mathf.Clamp01(t));
            SetAlpha(glowA, a); SetAlpha(glowB, a);
            yield return null;
        }
        glowA.enabled = glowB.enabled = false;
    }

    IEnumerator Dismiss()
    {
        dismissed = true;
        yield return FadeValue(v =>
        {
            SetAlpha(dimSR, v);
            SetFingerAlpha(v * 0.85f);
        }, 0.72f, 0f, 0.35f);

        Destroy(demoRoot.gameObject);
        // Trigger the real game intro (face-up preview → flip → shuffle)
        if (MatchManager.instance != null)
            MatchManager.instance.StartIntro();
        Destroy(this);
    }

    // ── generic utilities ─────────────────────────────────────────────────
    IEnumerator FadeValue(System.Action<float> apply, float from, float to, float dur)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / Mathf.Max(dur, 0.01f);
            apply(Mathf.Lerp(from, to, Mathf.Clamp01(t)));
            yield return null;
        }
        apply(to);
    }

    void SetAlpha(SpriteRenderer sr, float a) { if (sr==null) return; var c=sr.color; c.a=a; sr.color=c; }
    void SetFingerAlpha(float a) { SetAlpha(fingerSR, a * 0.92f); SetAlpha(trailSR, a * 0.28f); }

    // ── sprite factories ──────────────────────────────────────────────────
    static Sprite MakeSolid(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px  = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = col;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,w,h), Vector2.one*0.5f, (float)w);
    }

    static void DrawCircleOnSprite(SpriteRenderer sr)
    {
        // Replace the sprite with one that has a simple circle icon
        var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        var bg  = new Color(0.22f, 0.72f, 0.54f, 1f);
        var fg  = new Color(1f, 1f, 1f, 0.85f);
        for (int y = 0; y < 256; y++)
        for (int x = 0; x < 256; x++)
        {
            float nx = (x - 128f) / 80f, ny = (y - 128f) / 80f;
            float d  = nx*nx + ny*ny;
            tex.SetPixel(x, y, d < 1f && d > 0.45f ? fg : bg);
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,256,256), Vector2.one*0.5f, 256f);
    }

    static Sprite MakeCardBack(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var bg  = new Color(0.10f, 0.36f, 0.52f);
        var brd = new Color(0.06f, 0.22f, 0.33f);
        var inn = new Color(0.13f, 0.44f, 0.62f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            int b = 14, h = 8;
            Color c = (x<b||x>=size-b||y<b||y>=size-b) ? brd
                    : (x<b+h||x>=size-b-h||y<b+h||y>=size-b-h) ? inn : bg;
            if (x%18<2 && y%18<2) c = Color.Lerp(c, new Color(1,1,1,0.15f), 0.5f);
            tex.SetPixel(x, y, c);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size), Vector2.one*0.5f, (float)size);
    }

    static Sprite MakeCircle(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float cx=size*.5f, cy=size*.5f, r=size*.5f;
        for (int y=0;y<size;y++) for (int x=0;x<size;x++)
        {
            float d = Mathf.Sqrt((x-cx)*(x-cx)+(y-cy)*(y-cy));
            float a = Mathf.Clamp01(1f-d/r); a*=a;
            tex.SetPixel(x,y,new Color(1,1,1,a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size), Vector2.one*0.5f, size);
    }
}
