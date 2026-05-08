using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;

    [System.Serializable]
    public class ToolConfig
    {
        public string toolName = "Tool";
        public Sprite toolSprite;
        [TextArea(3, 8)]
        public string infoText = "Info placeholder — add your text here.";
    }

    [Header("Tools (6 needed for a 4x3 grid)")]
    public ToolConfig[] tools;

    [Header("Grid")]
    public int   cols         = 4;
    public int   rows         = 4;
    public float cardWidth    = 1.10f;
    public float cardHeight   = 1.10f;
    public float gapX         = 0.20f;
    public float gapY         = 0.20f;
    public Vector3 gridCenter = Vector3.zero;

    [Header("Card Flip")]
    public float flipDuration      = 0.35f;
    public float mismatchDelay     = 0.8f;   // seconds before flipping mismatched cards back

    [Header("Info Panel (assign in Inspector)")]
    public GameObject   infoPanel;
    public Image        infoPanelToolImage;
    public TextMeshProUGUI infoPanelTitle;
    public TextMeshProUGUI infoPanelBody;
    public Button       infoPanelCloseBtn;
    public Image        dimOverlay;

    [Header("Tutorial")]
    public GameObject tutorialPanel;
    public UnityEngine.UI.Button tutorialStartBtn;

    [Header("Quiz")]
    public string levelID       = "Level3";
    public string quizSceneName = "Quiz";

    readonly List<Card>   allCards       = new();
    readonly List<Vector3> gridPositions   = new();
    Card                firstFlipped;
    bool                locked;           // block input while evaluating
    int                 matchesFound;
    public void SetLocked(bool v) { locked = v; }
    public void ShowHintTutorial()
    {
        var tut = GetComponent<Level3Tutorial>();
        if (tut == null) tut = gameObject.AddComponent<Level3Tutorial>();
        locked = true;   // block input during replay
        tut.Show();
    }
    int                 totalPairs;

    static Sprite s_cardBack;
    static Sprite s_shadow;
    static Sprite s_glow;
    static readonly Sprite[] s_placeholders = new Sprite[8];

    void Awake()
    {
        instance = this;
        // Disable URP Rendering Debugger overlay (performance monitor)
        var t = System.Type.GetType("UnityEngine.Rendering.DebugManager, Unity.RenderPipelines.Core.Runtime");
        if (t != null) {
            var inst = t.GetProperty("instance", System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static)?.GetValue(null);
            t.GetProperty("enableRuntimeUI")?.SetValue(inst, false);
        }
        BuildSharedSprites();
        EnsureTools();
    }

    [Header("Intro Preview")]
    public float previewDuration = 2.2f;
    public float flipStagger     = 0.06f;
    public float shuffleDuration = 0.75f;

    void Start()
    {
        totalPairs = (cols * rows) / 2;
        if (infoPanel  != null) infoPanel.SetActive(false);
        if (dimOverlay != null) dimOverlay.gameObject.SetActive(false);
        if (infoPanelCloseBtn != null)
            infoPanelCloseBtn.onClick.AddListener(CloseInfoPanel);

        // Wire the '?' hint button
        var hintBtn = GameObject.Find("HintButton")?.GetComponent<UnityEngine.UI.Button>();
        if (hintBtn != null) hintBtn.onClick.AddListener(ShowHintTutorial);

        locked = true;
        BuildGrid();

        bool hasTutorial = FindFirstObjectByType<Level3Tutorial>() != null;
        if (hasTutorial)
        {
            // Hide all cards while tutorial plays
            foreach (var card in allCards) card.gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(IntroSequence());
        }
    }

    /// Called by Level3Tutorial when the player dismisses it.
    bool _introStarted = false;
    public void StartIntro()
    {
        if (_introStarted) { locked = false; return; }
        _introStarted = true;
        // Reactivate cards that were hidden during tutorial
        foreach (var card in allCards)
            if (card != null) card.gameObject.SetActive(true);
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        locked = true;
        yield return new WaitForSeconds(previewDuration);
        yield return FlipAllFaceDown();
        yield return new WaitForSeconds(0.25f);
        yield return ShuffleCards();
        yield return new WaitForSeconds(0.15f);
        locked = false;
    }

    IEnumerator FlipAllFaceDown()
    {
        int pending = allCards.Count;
        bool allDone = false;
        for (int i = 0; i < allCards.Count; i++)
        {
            var card = allCards[i];
            float delay = i * flipStagger;
            StartCoroutine(DelayedFlipDown(card, delay, () => { if (--pending == 0) allDone = true; }));
        }
        float maxWait = allCards.Count * flipStagger + flipDuration + 0.5f;
        float elapsed = 0f;
        while (!allDone && elapsed < maxWait) { elapsed += Time.deltaTime; yield return null; }
    }

    IEnumerator DelayedFlipDown(Card card, float delay, System.Action done)
    {
        yield return new WaitForSeconds(delay);
        card.FlipDown(done);
    }

    IEnumerator ShuffleCards()
    {
        var positions = new List<Vector3>(gridPositions);
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (positions[i], positions[j]) = (positions[j], positions[i]);
        }
        float arcH = Mathf.Max(cardWidth, cardHeight) * 0.7f;
        for (int i = 0; i < allCards.Count; i++)
            allCards[i].SlideTo(positions[i], shuffleDuration, arcH);
        yield return new WaitForSeconds(shuffleDuration + 0.05f);
    }

    void DismissTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        locked = false;
    }

    void BuildGrid()
    {
        // Ordered pairs so player can clearly see all tools face-up:
        // pair 0 → positions 0&1, pair 1 → 2&3, etc.
        var ids = new List<int>();
        for (int i = 0; i < totalPairs; i++) { ids.Add(i % tools.Length); ids.Add(i % tools.Length); }
        // NOTE: we do NOT shuffle ids here — the shuffle happens as an animation later.

        float totalW = cols * cardWidth  + (cols - 1) * gapX;
        float totalH = rows * cardHeight + (rows - 1) * gapY;
        float startX = gridCenter.x - totalW * 0.5f + cardWidth  * 0.5f;
        float startY = gridCenter.y + totalH * 0.5f - cardHeight * 0.5f;

        gridPositions.Clear();
        int idx = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = startX + c * (cardWidth  + gapX);
                float y = startY - r * (cardHeight + gapY);
                var pos = new Vector3(x, y, 0f);
                gridPositions.Add(pos);

                // Create card FACE-UP so player sees all tools during preview
                var card = CreateCard(idx, pos, ids[idx], faceUp: true);
                allCards.Add(card);
                idx++;
            }
        }
    }

    Card CreateCard(int index, Vector3 worldPos, int toolId, bool faceUp = false)
    {
        var go = new GameObject("Card_" + index);
        go.transform.position = worldPos;

        // Shadow
        var shadowGO = new GameObject("Shadow");
        shadowGO.transform.SetParent(go.transform, false);
        shadowGO.transform.localPosition = new Vector3(0.07f, -0.07f, 0.1f);
        var shadowSR = shadowGO.AddComponent<SpriteRenderer>();
        shadowSR.sprite = s_shadow;
        shadowSR.sortingOrder = 0;
        shadowSR.transform.localScale = new Vector3(cardWidth, cardHeight, 1f);

        // Back face
        var backGO = new GameObject("Back");
        backGO.transform.SetParent(go.transform, false);
        var backSR = backGO.AddComponent<SpriteRenderer>();
        backSR.sprite = s_cardBack;
        backSR.sortingOrder = 1;
        backGO.transform.localScale = new Vector3(cardWidth, cardHeight, 1f);

        // Front face — fill card using a uniform scale
        var frontGO = new GameObject("Front");
        frontGO.transform.SetParent(go.transform, false);
        var frontSR = frontGO.AddComponent<SpriteRenderer>();
        var toolSprite = tools[toolId].toolSprite ?? s_placeholders[toolId % s_placeholders.Length];
        frontSR.sprite = toolSprite;
        frontSR.sortingOrder = 2;
        frontSR.enabled = false;
        // Scale to fill 86% of card on shortest axis, preserving aspect ratio
        float fill   = cardWidth * 0.86f;
        float sprW   = toolSprite.bounds.size.x;
        float sprH   = toolSprite.bounds.size.y;
        float scaleX = (sprW > 0) ? fill / sprW : fill;
        float scaleY = (sprH > 0) ? fill / sprH : fill;
        // Use smaller scale so image fits entirely inside the card
        float fitS   = Mathf.Min(scaleX, scaleY);
        // But bump it up so the image fills nicely — cap at card size
        float maxS   = cardWidth * 0.92f / Mathf.Max(sprW, sprH);
        fitS = Mathf.Min(fitS, maxS);
        frontGO.transform.localScale = new Vector3(fitS, fitS, 1f);

        // Match overlay — sits ON TOP of the front face as a colour flash
        var glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(go.transform, false);
        glowGO.transform.localPosition = Vector3.zero;   // same plane as front
        var glowSR = glowGO.AddComponent<SpriteRenderer>();
        glowSR.sprite = s_glow;
        glowSR.sortingOrder = 4;   // above front (2) and shadow (0)
        glowSR.color = new Color(1f, 1f, 1f, 0f);        // transparent white — coloured in SetMatched
        glowSR.enabled = false;
        glowGO.transform.localScale = new Vector3(cardWidth, cardHeight, 1f);

        // Collider for click detection
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(cardWidth, cardHeight);

        // Card component
        var card = go.AddComponent<Card>();
        card.backSR   = backSR;
        card.frontSR  = frontSR;
        card.shadowSR = shadowSR;
        card.glowSR   = glowSR;
        card.flipDuration = flipDuration;
        if (faceUp)
            card.InitialiseFaceUp(toolId, frontSR.sprite);
        else
            card.Initialise(toolId, frontSR.sprite);

        return card;
    }

    public void OnCardClicked(Card card)
    {
        if (locked || card.IsUp) return;

        card.FlipUp();

        if (firstFlipped == null)
        {
            firstFlipped = card;
        }
        else
        {
            locked = true;
            StartCoroutine(EvaluatePair(firstFlipped, card));
            firstFlipped = null;
        }
    }

    IEnumerator EvaluatePair(Card a, Card b)
    {
        yield return new WaitForSeconds(mismatchDelay);

        if (a.ToolId == b.ToolId)
        {
            a.SetMatched();
            b.SetMatched();
            matchesFound++;
            ShowInfoPanel(a.ToolId);
            // locked stays true until panel is closed
        }
        else
        {
            a.FlipDown();
            b.FlipDown(() => locked = false);
        }
    }

    void ShowInfoPanel(int toolId)
    {
        var cfg = tools[toolId % tools.Length];

        if (infoPanelToolImage != null)
            infoPanelToolImage.sprite = cfg.toolSprite ?? s_placeholders[toolId % s_placeholders.Length];
        if (infoPanelTitle != null) infoPanelTitle.text = cfg.toolName;
        if (infoPanelBody  != null) infoPanelBody.text  = cfg.infoText;

        if (infoPanel  != null) infoPanel.SetActive(true);
        if (dimOverlay != null) dimOverlay.gameObject.SetActive(true);

        StartCoroutine(AnimatePanel(true));
    }

    void CloseInfoPanel()
    {
        StartCoroutine(AnimatePanel(false, () =>
        {
            if (infoPanel  != null) infoPanel.SetActive(false);
            if (dimOverlay != null) dimOverlay.gameObject.SetActive(false);
            locked = false;

            if (matchesFound >= totalPairs)
                StartCoroutine(FinishLevel());
        }));
    }

    IEnumerator AnimatePanel(bool show, System.Action done = null)
    {
        if (infoPanel == null) { done?.Invoke(); yield break; }
        var rt = infoPanel.GetComponent<RectTransform>();
        Vector3 from = show ? Vector3.one * 0.5f : Vector3.one;
        Vector3 to   = show ? Vector3.one        : Vector3.one * 0.5f;
        float   fa   = show ? 0f : 1f;
        float   fb   = show ? 1f : 0f;

        var cg = infoPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = infoPanel.AddComponent<CanvasGroup>();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            rt.localScale = Vector3.Lerp(from, to, e);
            cg.alpha      = Mathf.Lerp(fa, fb, e);
            yield return null;
        }
        rt.localScale = to;
        cg.alpha      = fb;
        done?.Invoke();
    }

    IEnumerator FinishLevel()
    {
        yield return new WaitForSeconds(1f);
        QuizManager.LoadQuizForLevel(levelID, quizSceneName);
    }

    static void BuildSharedSprites()
    {
        s_cardBack = MakeCardBack(256, 256);
        s_shadow   = MakeSolidSprite(256, 256, new Color(0f, 0f, 0f, 0.35f));
        s_glow     = MakeSolidSprite(256, 256, new Color(1f, 1f, 1f, 1f));

        Color[] palettes = {
            new Color(0.18f,0.62f,0.78f),  // blue
            new Color(0.22f,0.72f,0.54f),  // teal
            new Color(0.85f,0.38f,0.31f),  // red
            new Color(0.95f,0.68f,0.22f),  // amber
            new Color(0.58f,0.34f,0.72f),  // purple
            new Color(0.28f,0.55f,0.28f),  // green
            new Color(0.88f,0.46f,0.68f),  // pink
            new Color(0.42f,0.42f,0.52f),  // slate
        };
        for (int i = 0; i < 8; i++)
            s_placeholders[i] = MakePlaceholderSprite(256, 256, palettes[i], i + 1);
    }

    void EnsureTools()
    {
        if (tools == null || tools.Length == 0)
        {
            tools = new ToolConfig[8];
            string[] names = { "Espelho Bucal", "Sonda", "Turbina", "Fórceps",
                                "Seringa", "Destartarizador", "Radiografia", "Escova" };
            for (int i = 0; i < 8; i++)
                tools[i] = new ToolConfig { toolName = names[i],
                    infoText = "[Texto informativo sobre " + names[i] + " — a preencher.]" };
        }
    }

    static Sprite MakeCardBack(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var bg     = new Color(0.10f, 0.36f, 0.52f, 1f);  // dark teal
        var border = new Color(0.06f, 0.22f, 0.33f, 1f);
        var inner  = new Color(0.13f, 0.44f, 0.62f, 1f);
        var cross  = new Color(1f, 1f, 1f, 0.15f);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int brd = 14, hi = 8;
            Color c;
            if (x < brd || x >= w-brd || y < brd || y >= h-brd)           c = border;
            else if (x < brd+hi || x >= w-brd-hi || y < brd+hi || y >= h-brd-hi) c = inner;
            else c = bg;

            // subtle dot grid
            if ((x % 18 < 2) && (y % 18 < 2)) c = Color.Lerp(c, cross, 0.5f);
            tex.SetPixel(x, y, c);
        }

        // Draw tooth silhouette centred
        DrawTooth(tex, w/2, h/2, w * 0.22f, new Color(1f,1f,1f,0.18f));
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0,0,w,h), Vector2.one*0.5f, (float)w);
    }

    static void DrawTooth(Texture2D tex, int cx, int cy, float r, Color col)
    {
        // Simple molar outline: wide top (crown), narrow at roots
        for (int y = 0; y < tex.height; y++)
        for (int x = 0; x < tex.width;  x++)
        {
            float nx = (x - cx) / r;
            float ny = (y - cy) / r;
            // Crown: wide ellipse
            float crown = (nx*nx) / 1.0f + (ny*ny + ny*0.2f) / 0.7f;
            if (crown < 1.0f && crown > 0.6f)
            {
                float a = Mathf.Clamp01((1.0f - crown) / 0.4f * 2f);
                var cur = tex.GetPixel(x, y);
                tex.SetPixel(x, y, Color.Lerp(cur, col, a * col.a));
            }
        }
    }

    static Sprite MakeSolidSprite(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,w,h), Vector2.one*0.5f, (float)w);
    }

    static Sprite MakePlaceholderSprite(int w, int h, Color bg, int num)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var dark = new Color(bg.r*0.65f, bg.g*0.65f, bg.b*0.65f, 1f);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int b = 12;
            tex.SetPixel(x, y, (x<b||x>=w-b||y<b||y>=h-b) ? dark : bg);
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0,0,w,h), Vector2.one*0.5f, (float)w);
    }
}
