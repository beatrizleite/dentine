using UnityEngine;
using System.Collections;

public class Level3Tutorial : MonoBehaviour
{
    public float moveSpeed      = 4.5f;
    public float postFlipPause  = 0.40f;
    public float matchHoldPause = 1.3f;
    public float resetPause     = 0.45f;

    bool running = false;

    const float CARD  = 1.9f;
    const float GAP   = 0.55f;
    const float Z_FNG = -0.3f;

    public void Show()
    {
        if (running) return;
        // Destroy any old tutorial root
        var old = transform.Find("TutorialRoot");
        if (old != null) Destroy(old.gameObject);
        StartCoroutine(RunOnce());
    }

    void Start()
    {
        var titleCard = Object.FindFirstObjectByType<LevelTitleCard>();
        if (titleCard != null)
            titleCard.OnDismissed += Show;
        else
            Show();
    }

    IEnumerator RunOnce()
    {
        running = true;

        var root = new GameObject("TutorialRoot").transform;
        root.SetParent(transform, false);

        // Camera-aligned dim
        var cam     = Camera.main;
        var camPos  = cam != null ? cam.transform.position : Vector3.zero;
        float cW    = cam != null ? cam.orthographicSize * cam.aspect * 2f + 4f : 30f;
        float cH    = cam != null ? cam.orthographicSize * 2f + 4f : 30f;

        var dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(root, false);
        dimGO.transform.position = new Vector3(camPos.x, camPos.y, 0.4f);
        var dimSR = dimGO.AddComponent<SpriteRenderer>();
        dimSR.sprite = MakeSolid(4, 4, new Color(0,0,0,0));
        dimSR.sortingOrder = 50;
        dimGO.transform.localScale = new Vector3(cW, cH, 1f);

        // Cards at camera centre
        float cx = camPos.x, cy = camPos.y;
        float leftX = cx - (CARD + GAP) * 0.5f;
        float rightX= cx + (CARD + GAP) * 0.5f;
        float cardY = cy + 0.2f;

        GameObject cardA = BuildCard(root, new Vector3(leftX, cardY, 0f), false);
        GameObject cardB = BuildCard(root, new Vector3(rightX,cardY, 0f), false);

        // Finger
        var fingerGO = new GameObject("Finger");
        fingerGO.transform.SetParent(root, false);
        fingerGO.transform.position = new Vector3(cx, cy - 3.5f, Z_FNG);

        var trailGO = new GameObject("Trail"); trailGO.transform.SetParent(fingerGO.transform, false);
        var trailSR = trailGO.AddComponent<SpriteRenderer>();
        trailSR.sprite = MakeCircle(64); trailSR.sortingOrder = 60;
        trailSR.color  = new Color(1,1,1,0); trailGO.transform.localScale = Vector3.one * 0.55f;

        var dotGO = new GameObject("Dot"); dotGO.transform.SetParent(fingerGO.transform, false);
        var dotSR = dotGO.AddComponent<SpriteRenderer>();
        dotSR.sprite = MakeCircle(64); dotSR.sortingOrder = 61;
        dotSR.color  = new Color(1,1,1,0); dotGO.transform.localScale = Vector3.one * 0.34f;

        var realCards = FindObjectsByType<Card>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in realCards) c.gameObject.SetActive(false);

        yield return StartCoroutine(FadeSR(dimSR, 0f, 0.72f, 0.30f));

        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(DoTap(fingerGO.transform, dotSR, trailSR, cardA));
        yield return new WaitForSeconds(postFlipPause);
        yield return StartCoroutine(DoTap(fingerGO.transform, dotSR, trailSR, cardB));
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FlashBoth(cardA, cardB, new Color(0.18f,0.85f,0.28f), matchHoldPause));
        yield return StartCoroutine(Flip(cardA, false));
        yield return new WaitForSeconds(0.06f);
        yield return StartCoroutine(Flip(cardB, false));
        SetDotAlpha(dotSR, trailSR, 0f);
        yield return new WaitForSeconds(resetPause);

        SetCardColour(cardB, new Color(0.85f, 0.38f, 0.31f, 1f));  // different tool
        yield return StartCoroutine(DoTap(fingerGO.transform, dotSR, trailSR, cardA));
        yield return new WaitForSeconds(postFlipPause);
        yield return StartCoroutine(DoTap(fingerGO.transform, dotSR, trailSR, cardB));
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FlashBoth(cardA, cardB, new Color(0.92f,0.18f,0.18f), matchHoldPause * 0.7f));
        yield return StartCoroutine(ShakeTwo(cardA, cardB, 0.38f));
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(Flip(cardA, false));
        yield return new WaitForSeconds(0.06f);
        yield return StartCoroutine(Flip(cardB, false));
        SetCardColour(cardB, new Color(0.22f, 0.72f, 0.54f, 1f));  // reset
        SetDotAlpha(dotSR, trailSR, 0f);
        yield return new WaitForSeconds(0.2f);

        SetDotAlpha(dotSR, trailSR, 0f);
        yield return StartCoroutine(FadeSR(dimSR, 0.72f, 0f, 0.30f));

        Destroy(root.gameObject);

        foreach (var c in realCards)
            if (c != null) c.gameObject.SetActive(true);

        running = false;
        if (MatchManager.instance != null) MatchManager.instance.StartIntro();
    }


    IEnumerator DoTap(Transform finger, SpriteRenderer dot, SpriteRenderer trail, GameObject card)
    {
        yield return StartCoroutine(MoveTo(finger, dot, trail, card.transform.position));
        yield return StartCoroutine(TapAnim(finger, dot, trail));
        yield return StartCoroutine(Flip(card, true));
    }

    IEnumerator MoveTo(Transform finger, SpriteRenderer dot, SpriteRenderer trail, Vector3 target)
    {
        target.z = Z_FNG;
        SetDotAlpha(dot, trail, 0.85f);
        Vector3 start = finger.position;
        float dist    = Vector3.Distance(start, target);
        float dur     = Mathf.Max(dist / moveSpeed, 0.05f);
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            finger.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            if (trail != null)
                trail.transform.position = Vector3.Lerp(
                    trail.transform.position, finger.position, Time.deltaTime * 10f);
            yield return null;
        }
        finger.position = target;
    }

    IEnumerator TapAnim(Transform finger, SpriteRenderer dot, SpriteRenderer trail)
    {
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime/0.12f; float s=t<0.5f?Mathf.Lerp(1f,1.35f,t*2f):Mathf.Lerp(1.35f,0.85f,(t-0.5f)*2f); finger.localScale=Vector3.one*s; yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.deltaTime/0.10f; finger.localScale=Vector3.one*Mathf.Lerp(0.85f,1f,Mathf.Clamp01(t)); yield return null; }
        finger.localScale = Vector3.one;
    }

    IEnumerator Flip(GameObject card, bool toFront)
    {
        if (card == null) yield break;
        var back  = card.transform.Find("Back")?.GetComponent<SpriteRenderer>();
        var front = card.transform.Find("Front")?.GetComponent<SpriteRenderer>();
        float t = 0f, half = 0.20f;
        while (t < 1f) { t += Time.deltaTime/half; card.transform.localScale=new Vector3(Mathf.Lerp(1,0,Mathf.Clamp01(t*t*t)),1,1); yield return null; }
        if (back)  back.enabled  = !toFront;
        if (front) front.enabled =  toFront;
        t = 0f;
        while (t < 1f) { t += Time.deltaTime/half; card.transform.localScale=new Vector3(Mathf.Lerp(0,1,1f-Mathf.Pow(1f-Mathf.Clamp01(t),3f)),1,1); yield return null; }
        card.transform.localScale = Vector3.one;
    }

    IEnumerator FlashBoth(GameObject a, GameObject b, Color col, float hold)
    {
        var srs = new SpriteRenderer[4];
        srs[0] = a?.transform.Find("Overlay")?.GetComponent<SpriteRenderer>();
        srs[1] = b?.transform.Find("Overlay")?.GetComponent<SpriteRenderer>();
        srs[2] = a?.transform.Find("Glow")?.GetComponent<SpriteRenderer>();
        srs[3] = b?.transform.Find("Glow")?.GetComponent<SpriteRenderer>();
        foreach (var s in srs) { if (s) { s.color=new Color(col.r,col.g,col.b,0); s.enabled=true; } }

        float t = 0f;
        while (t < 1f) { t += Time.deltaTime/0.20f; float al=Mathf.SmoothStep(0,1,Mathf.Clamp01(t)); SetSRAlphas(srs, al*0.58f); float sc=1f+Mathf.Sin(Mathf.Clamp01(t)*Mathf.PI)*0.07f; if(a)a.transform.localScale=Vector3.one*sc; if(b)b.transform.localScale=Vector3.one*sc; yield return null; }
        if(a)a.transform.localScale=Vector3.one; if(b)b.transform.localScale=Vector3.one;

        yield return new WaitForSeconds(hold);

        t = 0f;
        while (t < 1f) { t += Time.deltaTime/0.18f; SetSRAlphas(srs, (1f-Mathf.Clamp01(t))*0.58f); yield return null; }
        foreach (var s in srs) { if (s) s.enabled=false; }
    }

    void SetSRAlphas(SpriteRenderer[] srs, float a) { foreach (var s in srs) { if (!s) continue; var c=s.color; c.a=a; s.color=c; } }

    IEnumerator ShakeTwo(GameObject a, GameObject b, float dur)
    {
        if (a==null||b==null) yield break;
        var pA=a.transform.localPosition; var pB=b.transform.localPosition;
        float t=0f, amp=0.20f, freq=30f;
        while (t < dur) { t+=Time.deltaTime; float off=Mathf.Sin(t*freq)*amp*(1f-t/dur); a.transform.localPosition=pA+new Vector3(off,0,0); b.transform.localPosition=pB+new Vector3(-off,0,0); yield return null; }
        a.transform.localPosition=pA; b.transform.localPosition=pB;
    }

    IEnumerator FadeSR(SpriteRenderer sr, float from, float to, float dur)
    {
        if (sr == null) yield break;
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime/Mathf.Max(dur,0.01f); var c=sr.color; c.a=Mathf.Lerp(from,to,Mathf.Clamp01(t)); sr.color=c; yield return null; }
        var cf=sr.color; cf.a=to; sr.color=cf;
    }

    void SetDotAlpha(SpriteRenderer dot, SpriteRenderer trail, float a)
    {
        if (dot)   { var c=dot.color;   c.a=a*0.92f; dot.color=c;   }
        if (trail) { var c=trail.color; c.a=a*0.28f; trail.color=c; }
    }

    void SetCardColour(GameObject card, Color col)
    {
        var front = card?.transform.Find("Front")?.GetComponent<SpriteRenderer>();
        if (front) front.color = col;
    }

    GameObject BuildCard(Transform parent, Vector3 pos, bool faceUp)
    {
        var root = new GameObject("DemoCard").transform;
        root.SetParent(parent, false); root.localPosition = pos;

        var sh = new GameObject("Shadow"); sh.transform.SetParent(root,false);
        sh.transform.localPosition = new Vector3(0.06f,-0.06f,0.05f);
        var shSR = sh.AddComponent<SpriteRenderer>(); shSR.sprite=MakeSolid(256,256,new Color(0,0,0,0.4f)); shSR.sortingOrder=51; sh.transform.localScale=new Vector3(CARD,CARD,1);

        var bk = new GameObject("Back"); bk.transform.SetParent(root,false);
        var bkSR = bk.AddComponent<SpriteRenderer>(); bkSR.sprite=MakeCardBack(256); bkSR.sortingOrder=52; bk.transform.localScale=new Vector3(CARD,CARD,1); bkSR.enabled=!faceUp;

        var ft = new GameObject("Front"); ft.transform.SetParent(root,false);
        var ftSR = ft.AddComponent<SpriteRenderer>(); DrawCircle(ftSR); ftSR.sortingOrder=52; ft.transform.localScale=new Vector3(CARD*0.82f,CARD*0.82f,1); ftSR.enabled=faceUp;

        var ov = new GameObject("Overlay"); ov.transform.SetParent(root,false);
        var ovSR = ov.AddComponent<SpriteRenderer>(); ovSR.sprite=MakeSolid(256,256,Color.white); ovSR.sortingOrder=54; ov.transform.localScale=new Vector3(CARD,CARD,1); ovSR.enabled=false;

        var gl = new GameObject("Glow"); gl.transform.SetParent(root,false); gl.transform.localPosition=new Vector3(0,0,-0.02f);
        var glSR = gl.AddComponent<SpriteRenderer>(); glSR.sprite=MakeSolid(256,256,Color.white); glSR.sortingOrder=53; gl.transform.localScale=new Vector3(CARD*1.3f,CARD*1.3f,1); glSR.enabled=false;

        return root.gameObject;
    }

    static Sprite MakeSolid(int w,int h,Color col){var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);var px=new Color[w*h];for(int i=0;i<px.Length;i++)px[i]=col;tex.SetPixels(px);tex.Apply();return Sprite.Create(tex,new Rect(0,0,w,h),Vector2.one*.5f,(float)w);}
    static void DrawCircle(SpriteRenderer sr){var tex=new Texture2D(256,256,TextureFormat.RGBA32,false);var bg=new Color(0.22f,0.72f,0.54f,1f);var fg=new Color(1f,1f,1f,0.85f);for(int y=0;y<256;y++)for(int x=0;x<256;x++){float nx=(x-128f)/80f,ny=(y-128f)/80f,d=nx*nx+ny*ny;tex.SetPixel(x,y,d<1f&&d>0.45f?fg:bg);}tex.Apply();sr.sprite=Sprite.Create(tex,new Rect(0,0,256,256),Vector2.one*.5f,256f);}
    static Sprite MakeCardBack(int size){var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);var bg=new Color(0.10f,0.36f,0.52f);var brd=new Color(0.06f,0.22f,0.33f);var inn=new Color(0.13f,0.44f,0.62f);for(int y=0;y<size;y++)for(int x=0;x<size;x++){int b=14,h2=8;Color c=(x<b||x>=size-b||y<b||y>=size-b)?brd:(x<b+h2||x>=size-b-h2||y<b+h2||y>=size-b-h2)?inn:bg;if(x%18<2&&y%18<2)c=Color.Lerp(c,new Color(1,1,1,0.15f),0.5f);tex.SetPixel(x,y,c);}tex.Apply();return Sprite.Create(tex,new Rect(0,0,size,size),Vector2.one*.5f,(float)size);}
    static Sprite MakeCircle(int size){var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);float c=size*.5f;for(int y=0;y<size;y++)for(int x=0;x<size;x++){float d=Mathf.Sqrt((x-c)*(x-c)+(y-c)*(y-c));float a=Mathf.Clamp01(1f-d/c);a*=a;tex.SetPixel(x,y,new Color(1,1,1,a));}tex.Apply();return Sprite.Create(tex,new Rect(0,0,size,size),Vector2.one*.5f,size);}
}
