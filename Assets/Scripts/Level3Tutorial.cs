using UnityEngine;
using System.Collections;

/// Visual-only memory-match tutorial.
/// Shows: correct pair (green flash) then wrong pair (red flash + shake).
/// Auto-dismisses after one cycle — no tap needed.
/// Can be re-triggered by the '?' button.
public class Level3Tutorial : MonoBehaviour
{
    [Header("Timing")]
    public float moveSpeed      = 4.5f;
    public float postFlipPause  = 0.45f;
    public float matchHoldPause = 1.4f;
    public float resetPause     = 0.55f;

    // ── runtime objects ───────────────────────────────────────────────────
    Transform      demoRoot;
    Transform      cardARoot, cardBRoot;
    SpriteRenderer cardABack, cardBBack;
    SpriteRenderer cardAFront, cardBFront;
    SpriteRenderer cardAOverlay, cardBOverlay;   // full-card colour flash
    SpriteRenderer glowA, glowB;
    Transform      fingerRoot;
    SpriteRenderer fingerSR, trailSR;
    SpriteRenderer dimSR;

    bool running = false;

    const float CARD  = 1.9f;
    const float GAP   = 0.55f;
    const float Z_DIM = 0.5f;
    const float Z_CARD= 0.0f;
    const float Z_FNG = -0.3f;

    // ── called by MatchManager.Start and the '?' button ───────────────────
    public void Show()
    {
        if (running) return;
        if (demoRoot != null) Destroy(demoRoot.gameObject);
        BuildVisuals();
        StartCoroutine(RunOnce());
    }

    void Start() { Show(); }

    // ── main sequence ─────────────────────────────────────────────────────
    IEnumerator RunOnce()
    {
        running = true;

        // Fade in overlay
        yield return FadeValue(v => SetAlpha(dimSR, v), 0f, 0.72f, 0.30f);

        // Cards bounce in
        yield return BounceIn(cardARoot, 0f);
        yield return BounceIn(cardBRoot, 0.12f);
        yield return new WaitForSeconds(0.4f);

        // ── 1. CORRECT match ─────────────────────────────────────────────
        yield return MoveFinger(cardARoot.position);
        yield return TapAnim();
        yield return FlipCard(cardARoot, cardABack, cardAFront, true);
        yield return new WaitForSeconds(postFlipPause);

        yield return MoveFinger(cardBRoot.position);
        yield return TapAnim();
        yield return FlipCard(cardBRoot, cardBBack, cardBFront, true);
        yield return new WaitForSeconds(0.2f);

        // Green flash on cards + glow
        yield return FlashCards(new Color(0.18f, 0.85f, 0.28f, 0.70f), matchHoldPause);

        // Flip back
        yield return FlipCard(cardARoot, cardABack, cardAFront, false);
        yield return new WaitForSeconds(0.08f);
        yield return FlipCard(cardBRoot, cardBBack, cardBFront, false);
        SetFingerAlpha(0f);
        yield return new WaitForSeconds(resetPause);

        // ── 2. WRONG match ───────────────────────────────────────────────
        SetCardBWrong(true);

        yield return MoveFinger(cardARoot.position);
        yield return TapAnim();
        yield return FlipCard(cardARoot, cardABack, cardAFront, true);
        yield return new WaitForSeconds(postFlipPause);

        yield return MoveFinger(cardBRoot.position);
        yield return TapAnim();
        yield return FlipCard(cardBRoot, cardBBack, cardBFront, true);
        yield return new WaitForSeconds(0.2f);

        // Red flash on cards + shake
        yield return FlashCards(new Color(0.92f, 0.18f, 0.18f, 0.72f), matchHoldPause * 0.7f);
        yield return ShakeCards();
        yield return new WaitForSeconds(0.15f);

        // Flip back, restore
        yield return FlipCard(cardARoot, cardABack, cardAFront, false);
        yield return new WaitForSeconds(0.08f);
        yield return FlipCard(cardBRoot, cardBBack, cardBFront, false);
        SetCardBWrong(false);
        SetFingerAlpha(0f);
        yield return new WaitForSeconds(0.25f);

        // ── Auto-dismiss ─────────────────────────────────────────────────
        yield return FadeValue(v =>
        {
            SetAlpha(dimSR, v);
            SetFingerAlpha(v * 0.85f);
        }, 0.72f, 0f, 0.35f);

        Destroy(demoRoot.gameObject);
        demoRoot = null;
        running = false;

        // Unlock game (only on first Show — MatchManager checks internally)
        if (MatchManager.instance != null)
            MatchManager.instance.StartIntro();
    }

    // ── Card colour flash + glow ──────────────────────────────────────────
    IEnumerator FlashCards(Color col, float hold)
    {
        // Glow behind cards
        col.a = 0f;
        glowA.color = glowB.color = col;
        glowA.enabled = glowB.enabled = true;

        // Overlay on card faces
        cardAOverlay.enabled = cardBOverlay.enabled = true;
        col.a = 0f;
        cardAOverlay.color = cardBOverlay.color = col;

        // Fade in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.22f;
            float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            var glowCol = glowA.color;     glowCol.a = a * 0.60f;
            glowA.color = glowB.color = glowCol;
            var ovCol = cardAOverlay.color; ovCol.a = a * 0.55f;
            cardAOverlay.color = cardBOverlay.color = ovCol;
            // Slight bounce
            float s = 1f + Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * 0.07f;
            cardARoot.localScale = cardBRoot.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        cardARoot.localScale = cardBRoot.localScale = Vector3.one;

        yield return new WaitForSeconds(hold);

        // Fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.20f;
            float a = 1f - Mathf.Clamp01(t);
            var gc = glowA.color; gc.a = a * 0.60f; glowA.color = glowB.color = gc;
            var oc = cardAOverlay.color; oc.a = a * 0.55f; cardAOverlay.color = cardBOverlay.color = oc;
            yield return null;
        }
        glowA.enabled = glowB.enabled = false;
        cardAOverlay.enabled = cardBOverlay.enabled = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    void SetCardBWrong(bool wrong)
    {
        if (cardBFront == null) return;
        cardBFront.color = wrong
            ? new Color(0.85f, 0.38f, 0.31f, 1f)
            : new Color(0.22f, 0.72f, 0.54f, 1f);
    }

    IEnumerator ShakeCards()
    {
        float t = 0f, dur = 0.38f, amp = 0.20f, freq = 30f;
        var posA = cardARoot.localPosition;
        var posB = cardBRoot.localPosition;
        while (t < dur)
        {
            t += Time.deltaTime;
            float offset = Mathf.Sin(t * freq) * amp * (1f - t / dur);
            cardARoot.localPosition = posA + new Vector3(offset, 0, 0);
            cardBRoot.localPosition = posB + new Vector3(-offset, 0, 0);
            yield return null;
        }
        cardARoot.localPosition = posA;
        cardBRoot.localPosition = posB;
    }

    IEnumerator BounceIn(Transform card, float delay)
    {
        yield return new WaitForSeconds(delay);
        card.localScale = Vector3.zero;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.35f;
            float e = Mathf.Clamp01(t);
            float s = e < 0.7f ? Mathf.Lerp(0, 1.15f, e / 0.7f)
                               : Mathf.Lerp(1.15f, 1.0f, (e - 0.7f) / 0.3f);
            card.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        card.localScale = Vector3.one;
    }

    IEnumerator MoveFinger(Vector3 target)
    {
        target.z = Z_FNG;
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
        float t = 0;
        while (t < 1) { t += Time.deltaTime / 0.12f; float s = t < 0.5f ? Mathf.Lerp(1f,1.35f,t*2f) : Mathf.Lerp(1.35f,0.85f,(t-0.5f)*2f); fingerRoot.localScale = new Vector3(s,s,1); yield return null; }
        t = 0;
        while (t < 1) { t += Time.deltaTime / 0.10f; fingerRoot.localScale = new Vector3(Mathf.Lerp(0.85f,1f,Mathf.Clamp01(t)),Mathf.Lerp(0.85f,1f,Mathf.Clamp01(t)),1); yield return null; }
        fingerRoot.localScale = Vector3.one;
    }

    IEnumerator FlipCard(Transform root, SpriteRenderer back, SpriteRenderer front, bool toFront)
    {
        float half = 0.22f, t = 0;
        while (t < 1) { t += Time.deltaTime / half; root.localScale = new Vector3(Mathf.Lerp(1,0,Mathf.Clamp01(t*t*t)),1,1); yield return null; }
        back.enabled = !toFront; front.enabled = toFront;
        t = 0;
        while (t < 1) { t += Time.deltaTime / half; root.localScale = new Vector3(Mathf.Lerp(0,1,1-Mathf.Pow(1-Mathf.Clamp01(t),3)),1,1); yield return null; }
        root.localScale = Vector3.one;
    }

    IEnumerator FadeValue(System.Action<float> apply, float from, float to, float dur)
    {
        float t = 0;
        while (t < 1) { t += Time.deltaTime / Mathf.Max(dur, 0.01f); apply(Mathf.Lerp(from, to, Mathf.Clamp01(t))); yield return null; }
        apply(to);
    }

    void SetAlpha(SpriteRenderer sr, float a) { if (sr == null) return; var c = sr.color; c.a = a; sr.color = c; }
    void SetFingerAlpha(float a) { SetAlpha(fingerSR, a * 0.92f); SetAlpha(trailSR, a * 0.28f); }

    // ── Build visuals ─────────────────────────────────────────────────────
    void BuildVisuals()
    {
        demoRoot = new GameObject("TutorialRoot").transform;
        demoRoot.SetParent(transform, false);

        // Dim overlay
        var dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(demoRoot, false);
        dimGO.transform.localPosition = new Vector3(0, 0, Z_DIM);
        dimSR = dimGO.AddComponent<SpriteRenderer>();
        dimSR.sprite = MakeSolid(4, 4, new Color(0,0,0,0));
        dimSR.sortingOrder = 50;
        dimGO.transform.localScale = new Vector3(30, 30, 1);

        float leftX  = -(CARD + GAP) * 0.5f;
        float rightX =  (CARD + GAP) * 0.5f;
        float cardY  = 0.2f;

        cardARoot = BuildDemoCard(new Vector3(leftX,  cardY, Z_CARD), out cardABack, out cardAFront, out glowA, out cardAOverlay);
        cardBRoot = BuildDemoCard(new Vector3(rightX, cardY, Z_CARD), out cardBBack, out cardBFront, out glowB, out cardBOverlay);

        var pairCol = new Color(0.22f, 0.72f, 0.54f);
        cardAFront.color = cardBFront.color = pairCol;

        BuildFinger();
        SetAlpha(dimSR, 0f);
    }

    Transform BuildDemoCard(Vector3 pos,
        out SpriteRenderer backSR, out SpriteRenderer frontSR,
        out SpriteRenderer glowSR, out SpriteRenderer overlaySR)
    {
        var root = new GameObject("DemoCard").transform;
        root.SetParent(demoRoot, false);
        root.localPosition = pos;

        // Shadow
        var shGO = new GameObject("Shadow"); shGO.transform.SetParent(root, false);
        shGO.transform.localPosition = new Vector3(0.06f, -0.06f, 0.05f);
        var shSR = shGO.AddComponent<SpriteRenderer>();
        shSR.sprite = MakeSolid(256, 256, new Color(0,0,0,0.4f));
        shSR.sortingOrder = 51;
        shGO.transform.localScale = new Vector3(CARD, CARD, 1);

        // Back
        var backGO = new GameObject("Back"); backGO.transform.SetParent(root, false);
        backSR = backGO.AddComponent<SpriteRenderer>();
        backSR.sprite = MakeCardBack(256);
        backSR.sortingOrder = 52;
        backGO.transform.localScale = new Vector3(CARD, CARD, 1);

        // Front
        var frontGO = new GameObject("Front"); frontGO.transform.SetParent(root, false);
        frontSR = frontGO.AddComponent<SpriteRenderer>();
        frontSR.sortingOrder = 52;
        frontSR.enabled = false;
        frontGO.transform.localScale = new Vector3(CARD * 0.82f, CARD * 0.82f, 1);
        DrawCircleOnSprite(frontSR);

        // Colour overlay (green/red flash — sits on top of front)
        var ovGO = new GameObject("Overlay"); ovGO.transform.SetParent(root, false);
        overlaySR = ovGO.AddComponent<SpriteRenderer>();
        overlaySR.sprite = MakeSolid(256, 256, Color.white);
        overlaySR.sortingOrder = 54;   // above front (52) and glow (53)
        overlaySR.enabled = false;
        ovGO.transform.localScale = new Vector3(CARD, CARD, 1);

        // Glow
        var glowGO = new GameObject("Glow"); glowGO.transform.SetParent(root, false);
        glowGO.transform.localPosition = new Vector3(0, 0, -0.02f);
        glowSR = glowGO.AddComponent<SpriteRenderer>();
        glowSR.sprite = MakeSolid(256, 256, Color.white);
        glowSR.sortingOrder = 53;
        glowSR.enabled = false;
        glowGO.transform.localScale = new Vector3(CARD * 1.30f, CARD * 1.30f, 1);

        return root;
    }

    void BuildFinger()
    {
        fingerRoot = new GameObject("Finger").transform;
        fingerRoot.SetParent(demoRoot, false);
        fingerRoot.localPosition = new Vector3(0, -3.5f, Z_FNG);

        var tGO = new GameObject("Trail"); tGO.transform.SetParent(fingerRoot, false);
        trailSR = tGO.AddComponent<SpriteRenderer>();
        trailSR.sprite = MakeCircle(64); trailSR.sortingOrder = 60;
        trailSR.color = new Color(1,1,1,0); tGO.transform.localScale = new Vector3(0.55f,0.55f,1);

        var dGO = new GameObject("Dot"); dGO.transform.SetParent(fingerRoot, false);
        fingerSR = dGO.AddComponent<SpriteRenderer>();
        fingerSR.sprite = MakeCircle(64); fingerSR.sortingOrder = 61;
        fingerSR.color = new Color(1,1,1,0); dGO.transform.localScale = new Vector3(0.34f,0.34f,1);
    }

    // ── Sprite factories ──────────────────────────────────────────────────
    static Sprite MakeSolid(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px  = new Color[w*h]; for (int i=0;i<px.Length;i++) px[i]=col;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,w,h), Vector2.one*.5f, (float)w);
    }

    static void DrawCircleOnSprite(SpriteRenderer sr)
    {
        var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        var bg = new Color(0.22f, 0.72f, 0.54f, 1f);
        var fg = new Color(1f, 1f, 1f, 0.85f);
        for (int y=0;y<256;y++) for (int x=0;x<256;x++)
        { float nx=(x-128f)/80f,ny=(y-128f)/80f,d=nx*nx+ny*ny; tex.SetPixel(x,y,d<1f&&d>0.45f?fg:bg); }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,256,256), Vector2.one*.5f, 256f);
    }

    static Sprite MakeCardBack(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var bg=new Color(0.10f,0.36f,0.52f); var brd=new Color(0.06f,0.22f,0.33f); var inn=new Color(0.13f,0.44f,0.62f);
        for (int y=0;y<size;y++) for (int x=0;x<size;x++)
        { int b=14,h=8; Color c=(x<b||x>=size-b||y<b||y>=size-b)?brd:(x<b+h||x>=size-b-h||y<b+h||y>=size-b-h)?inn:bg; if(x%18<2&&y%18<2)c=Color.Lerp(c,new Color(1,1,1,0.15f),0.5f); tex.SetPixel(x,y,c); }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size), Vector2.one*.5f, (float)size);
    }

    static Sprite MakeCircle(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c=size*.5f;
        for (int y=0;y<size;y++) for (int x=0;x<size;x++)
        { float d=Mathf.Sqrt((x-c)*(x-c)+(y-c)*(y-c)); float a=Mathf.Clamp01(1f-d/c); a*=a; tex.SetPixel(x,y,new Color(1,1,1,a)); }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size), Vector2.one*.5f, size);
    }
}
