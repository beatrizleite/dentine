using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Level2Manager : MonoBehaviour
{
    public static Level2Manager instance;

    [Header("Character States")]
    public SpriteRenderer dirtyTeethRenderer;
    public SpriteRenderer cleanTeethRenderer;
    public SpriteRenderer gargleRenderer;
    public SpriteRenderer dirtyTongueRenderer;

    [Header("Toothbrush")]
    public GameObject toothbrush;

    [Header("Hint")]
    public GameObject hintArrow;

    [Header("Quiz Config")]
    public string levelID = "Level2";
    public string quizSceneName = "Quiz";

    [Header("Shake to Rinse")]
    public float shakeDurationNeeded = 1f;
    public float shakeThreshold = 2.5f;

    [Header("Hint Manager")]
    public HintManager hintManager;

    // ── phases ────────────────────────────────────────────────────────────
    public enum Phase { BrushTeeth, Rinse1, BrushTongue, Rinse2, Done }
    private Phase phase = Phase.BrushTeeth;
    public Phase CurrentPhase => phase;

    private bool  rinsing   = false;
    private bool  levelDone = false;
    private float shakeTimer = 0f;

    void Awake() { instance = this; }

    void Start()
    {
        ShowOnly(dirtyTeethRenderer);
        SetInactive(cleanTeethRenderer);
        SetInactive(gargleRenderer);
        SetInactive(dirtyTongueRenderer);
        if (hintArrow != null) hintArrow.SetActive(false);
        SetBrush(true);   // brush visible during first scrub phase
    }

    void Update()
    {
        if (rinsing && !levelDone) ProcessShake();
    }

    // ── called by ToothScrubZone ──────────────────────────────────────────
    public void OnScrubActivity()
    {
        if (hintManager != null) hintManager.OnScrubActivity();
    }

    public void OnCleaningProgress(float progress)
    {
        if (progress >= 1f && phase == Phase.BrushTeeth)
            StartCoroutine(TransitionToRinse1());
    }

    // ── called by TongueScrubZone ─────────────────────────────────────────
    public void OnTongueProgress(float progress)
    {
        if (hintManager != null) hintManager.OnScrubActivity();
        if (progress >= 1f && phase == Phase.BrushTongue)
            StartCoroutine(TransitionToRinse2());
    }

    // ── Phase transitions ─────────────────────────────────────────────────
    IEnumerator TransitionToRinse1()
    {
        phase = Phase.Rinse1;
        SetBrush(false);
        ShowOnly(gargleRenderer);
        StartShake();
        yield return null;
    }

    IEnumerator TransitionToRinse2()
    {
        phase = Phase.Rinse2;
        SetBrush(false);
        ShowOnly(gargleRenderer);
        StartShake();
        yield return null;
    }

    void StartShake()
    {
        rinsing    = true;
        shakeTimer = 0f;
        if (hintManager != null) hintManager.EnterRinsePhase();
    }

    void ProcessShake()
    {
        bool shaking = false;
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current != null)
            shaking = UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed;
#else
        shaking = Input.acceleration.magnitude > shakeThreshold;
#endif
        if (shaking)
        {
            if (hintManager != null) hintManager.OnShakeActivity();
            shakeTimer += Time.deltaTime;
            if (shakeTimer >= shakeDurationNeeded) FinishShake();
        }
    }

    void FinishShake()
    {
        rinsing = false;

        if (phase == Phase.Rinse1)
        {
            phase = Phase.BrushTongue;
            ShowOnly(dirtyTongueRenderer);
            SetBrush(true);    // brush back for tongue phase
            if (hintManager != null) hintManager.ExitRinsePhase();
        }
        else if (phase == Phase.Rinse2)
        {
            phase = Phase.Done;
            levelDone = true;
            StartCoroutine(ShowCleanAndFinish());
        }
    }

    IEnumerator ShowCleanAndFinish()
    {
        SetBrush(false);
        ShowOnly(cleanTeethRenderer);
        yield return new WaitForSeconds(2f);
        QuizManager.LoadQuizForLevel(levelID, quizSceneName);
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    void SetBrush(bool active)
    {
        if (toothbrush != null) toothbrush.SetActive(active);
    }

    void ShowOnly(SpriteRenderer sr)
    {
        SetInactive(dirtyTeethRenderer);
        SetInactive(cleanTeethRenderer);
        SetInactive(gargleRenderer);
        SetInactive(dirtyTongueRenderer);
        if (sr != null) sr.gameObject.SetActive(true);
    }

    void SetInactive(SpriteRenderer sr)
    {
        if (sr != null) sr.gameObject.SetActive(false);
    }
}
