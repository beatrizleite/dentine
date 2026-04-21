using UnityEngine;

/// Visual tutorial + idle hint for Level 1 (drag food to basket).
/// Uses a pure Update() state machine — no nested coroutines.
public class Level1GhostHint : MonoBehaviour
{
    [Header("Auto-found at runtime")]
    public Transform targetGood;
    public Transform targetBad;

    [Header("Timing")]
    public float startDelay    = 0.8f;
    public float idleTimeout   = 3.5f;
    public float dragDuration  = 0.9f;

    // ── visuals ───────────────────────────────────────────────────────────
    SpriteRenderer fingerSR, trailSR, glowSR;
    Transform fingerRoot, trailTrans;
    GameObject glowGO;

    // ── state machine ─────────────────────────────────────────────────────
    enum Phase
    {
        Waiting,       // waiting startDelay + food spawn
        FadeIn,        // finger fades in at food position
        Tap,           // scale pulse (finger 'taps' food)
        Drag,          // finger arcs from food to basket
        Bounce,        // basket scales up+down
        FadeOut,       // finger fades out
        Pause,         // short pause between gestures
        Idle,          // game unlocked; watching for player inactivity
        IdleHint       // showing hint gesture during idle
    }

    Phase  phase       = Phase.Waiting;
    float  timer       = 0f;
    int    gesturesDone = 0;      // 0,1 = tutorial; >=2 = idle
    bool   tutDone     = false;

    // current gesture targets
    Vector3    foodPos;
    Vector3    basketPos;
    GameObject basketGO;
    FoodDrag   currentFood;

    // idle tracking
    float idleTimer = 0f;

    // ── Unity ─────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildVisuals();
    }

    void Start()
    {
        if (targetGood == null) targetGood = GameObject.Find("Target_Good")?.transform;
        if (targetBad  == null) targetBad  = GameObject.Find("Target_Bad")?.transform;
        FoodDrag.tutorialActive = true;
        timer = 0f;
        phase = Phase.Waiting;
    }

    void Update()
    {
        switch (phase)
        {
            case Phase.Waiting:       DoWaiting();   break;
            case Phase.FadeIn:        DoFadeIn();    break;
            case Phase.Tap:           DoTap();       break;
            case Phase.Drag:          DoDrag();      break;
            case Phase.Bounce:        DoBounce();    break;
            case Phase.FadeOut:       DoFadeOut();   break;
            case Phase.Pause:         DoPause();     break;
            case Phase.Idle:          DoIdle();      break;
            case Phase.IdleHint:      DoDrag();      break;   // reuse Drag logic
        }

        // Trail always smoothly follows finger
        if (trailTrans != null && fingerRoot != null)
            trailTrans.position = Vector3.Lerp(
                trailTrans.position, fingerRoot.position, Time.deltaTime * 10f);
    }

    // ── Phase handlers ────────────────────────────────────────────────────
    void DoWaiting()
    {
        timer += Time.deltaTime;
        if (timer < startDelay) return;

        // Look for spawned food
        var food = FindFirstObjectByType<FoodDrag>();
        if (food == null) return;   // keep waiting

        PrepareGesture(food);
        Enter(Phase.FadeIn);
    }

    void DoFadeIn()
    {
        timer += Time.deltaTime;
        float a = Mathf.Clamp01(timer / 0.28f);
        SetAlpha(a);
        if (a >= 1f) Enter(Phase.Tap);
    }

    float tapT = 0f;
    void DoTap()
    {
        tapT += Time.deltaTime / 0.22f;
        float raw = Mathf.Clamp01(tapT);
        float s = raw < 0.5f
            ? Mathf.Lerp(1f, 1.45f, raw * 2f)
            : Mathf.Lerp(1.45f, 0.9f, (raw - 0.5f) * 2f);
        fingerRoot.localScale = Vector3.one * Mathf.Lerp(s, 1f, Mathf.Clamp01(tapT - 0.8f) / 0.2f);
        if (tapT >= 1f) { fingerRoot.localScale = Vector3.one; Enter(Phase.Drag); }
    }

    void DoDrag()
    {
        timer += Time.deltaTime;
        float t  = Mathf.Clamp01(timer / dragDuration);
        float e  = Mathf.SmoothStep(0f, 1f, t);
        float up = Mathf.Sin(t * Mathf.PI) * Vector3.Distance(foodPos, basketPos) * 0.18f;
        fingerRoot.position = Vector3.Lerp(foodPos, basketPos, e) + Vector3.up * up;

        // Pulse glow on basket
        if (glowGO != null && glowGO.activeSelf)
        {
            float ga = 0.3f + Mathf.Sin(Time.time * 5f) * 0.2f;
            SetGlowAlpha(ga);
        }

        if (t >= 1f)
        {
            fingerRoot.position = basketPos;
            // show glow
            if (glowGO != null) { glowGO.SetActive(true); SetGlowAlpha(0.5f); }
            if (phase == Phase.IdleHint) Enter(Phase.FadeOut);
            else Enter(Phase.Bounce);
        }
    }

    float bounceScale = 1f;
    Transform bounceTarget;
    Vector3   bounceOrig;
    void DoBounce()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / 0.28f);
        float s = t < 0.5f ? Mathf.Lerp(1f, 1.22f, t * 2f)
                            : Mathf.Lerp(1.22f, 1f, (t - 0.5f) * 2f);
        if (bounceTarget != null)
            bounceTarget.localScale = bounceOrig * s;
        if (t >= 1f)
        {
            if (bounceTarget != null) bounceTarget.localScale = bounceOrig;
            Enter(Phase.FadeOut);
        }
    }

    void DoFadeOut()
    {
        timer += Time.deltaTime;
        float a = Mathf.Clamp01(1f - timer / 0.32f);
        SetAlpha(a);
        SetGlowAlpha(a * 0.45f);
        if (a <= 0f)
        {
            if (glowGO != null) glowGO.SetActive(false);
            gesturesDone++;
            Enter(Phase.Pause);
        }
    }

    void DoPause()
    {
        timer += Time.deltaTime;
        float pauseTime = tutDone ? 0.5f : 0.35f;
        if (timer < pauseTime) return;

        if (!tutDone && gesturesDone < 2)
        {
            // Tutorial: do second gesture then unlock
            var food = FindFirstObjectByType<FoodDrag>();
            if (food != null) PrepareGesture(food);
            Enter(Phase.FadeIn);
        }
        else
        {
            // Tutorial complete
            if (!tutDone)
            {
                tutDone = true;
                FoodDrag.tutorialActive = false;
            }
            idleTimer = 0f;
            Enter(Phase.Idle);
        }
    }

    void DoIdle()
    {
        // Reset on real interaction
        bool realInteraction = FoodDrag.anyDragActive
            || Input.GetMouseButtonDown(0)
            || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (realInteraction) { idleTimer = 0f; return; }

        var food = FindFirstObjectByType<FoodDrag>();
        if (food == null) { idleTimer = 0f; return; }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTimeout)
        {
            idleTimer = 0f;
            PrepareGesture(food);
            Enter(Phase.IdleHint);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    void PrepareGesture(FoodDrag food)
    {
        currentFood = food;
        foodPos = food.transform.position;

        bool isUnhealthy = food.CompareTag("Unhealthy");
        var  target = isUnhealthy ? targetBad : targetGood;

        basketPos = target != null ? target.position : foodPos + Vector3.down * 4f;
        basketGO  = target?.gameObject;

        // Position finger just above food
        fingerRoot.position = new Vector3(foodPos.x, foodPos.y, -1f);
        trailTrans.position = fingerRoot.position;

        // Prepare glow on correct basket
        if (glowGO != null)
        {
            glowGO.transform.position = new Vector3(basketPos.x, basketPos.y, -0.5f);
            glowGO.SetActive(true);
            SetGlowAlpha(0f);
        }

        // Cache bounce target
        bounceTarget = basketGO?.transform;
        bounceOrig   = bounceTarget != null ? bounceTarget.localScale : Vector3.one;
    }

    void Enter(Phase next)
    {
        phase = next;
        timer = 0f;
        tapT  = 0f;
    }

    void SetAlpha(float a)
    {
        if (fingerSR != null) { var c = fingerSR.color; c.a = a * 0.95f; fingerSR.color = c; }
        if (trailSR  != null) { var c = trailSR.color;  c.a = a * 0.30f; trailSR.color  = c; }
    }

    void SetGlowAlpha(float a)
    {
        if (glowSR != null) { var c = glowSR.color; c.a = a; glowSR.color = c; }
    }

    // ── Build visuals ─────────────────────────────────────────────────────
    void BuildVisuals()
    {
        var root = new GameObject("GhostHintRoot");
        root.transform.SetParent(transform, false);

        // Trail
        var tGO = new GameObject("Trail");
        tGO.transform.SetParent(root.transform, false);
        trailSR    = tGO.AddComponent<SpriteRenderer>();
        trailSR.sprite = Circle(64);
        trailSR.sortingLayerName = "Default";
        trailSR.sortingOrder = 30;
        trailSR.color = new Color(1,1,1,0);
        tGO.transform.localScale = Vector3.one * 0.65f;
        trailTrans = tGO.transform;

        // Finger
        fingerRoot = new GameObject("Finger").transform;
        fingerRoot.SetParent(root.transform, false);
        var dGO = new GameObject("Dot");
        dGO.transform.SetParent(fingerRoot, false);
        fingerSR = dGO.AddComponent<SpriteRenderer>();
        fingerSR.sprite = Circle(64);
        fingerSR.sortingLayerName = "Default";
        fingerSR.sortingOrder = 31;
        fingerSR.color = new Color(1,1,1,0);
        dGO.transform.localScale = Vector3.one * 0.45f;

        // Target glow ring
        glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(root.transform, false);
        glowSR = glowGO.AddComponent<SpriteRenderer>();
        glowSR.sprite = Ring(128);
        glowSR.sortingLayerName = "Default";
        glowSR.sortingOrder = 29;
        glowSR.color = new Color(1f, 0.95f, 0.2f, 0f);
        glowGO.transform.localScale = Vector3.one * 3.5f;
        glowGO.SetActive(false);
    }

    static Sprite Circle(int n)
    {
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        float c = n * .5f;
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            float d = Mathf.Sqrt((x-c)*(x-c)+(y-c)*(y-c));
            float a = Mathf.Clamp01(1f - d/c); a *= a;
            tex.SetPixel(x,y,new Color(1,1,1,a));
        }
        tex.Apply();
        return Sprite.Create(tex,new Rect(0,0,n,n),Vector2.one*.5f,n);
    }

    static Sprite Ring(int n)
    {
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        float c = n*.5f, ro = n*.47f, ri = n*.33f;
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            float d = Mathf.Sqrt((x-c)*(x-c)+(y-c)*(y-c));
            float a = (d>=ri&&d<=ro) ? Mathf.Clamp01((d-ri)/5f)*Mathf.Clamp01((ro-d)/5f)*5f : 0f;
            tex.SetPixel(x,y,new Color(1,1,1,a));
        }
        tex.Apply();
        return Sprite.Create(tex,new Rect(0,0,n,n),Vector2.one*.5f,n);
    }
}
