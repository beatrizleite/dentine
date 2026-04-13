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

    [Header("Hint")]
    public GameObject hintArrow;

    [Header("Quiz Config")]
    public string levelID = "Level2";
    public string quizSceneName = "Quiz";

    [Header("Shake to Rinse")]
    public float shakeDurationNeeded = 3f;
    public float shakeThreshold = 2.5f;

    [Header("Hint Manager")]
    public HintManager hintManager;

    private bool  readyToRinse;
    private bool  rinsing;
    private bool  levelDone;
    private float shakeTimer;

    void Awake() { instance = this; }

    void Start()
    {
        ActivateSprite(cleanTeethRenderer, false);
        ActivateSprite(gargleRenderer, false);
        ActivateSprite(dirtyTeethRenderer, true);
        if (hintArrow != null) hintArrow.SetActive(false);
    }

    void Update()
    {
        if (rinsing && !levelDone) ProcessShake();
    }

    // Called by ToothScrubZone every time player scrubs
    public void OnScrubActivity()
    {
        if (hintManager != null) hintManager.OnScrubActivity();
    }

    public void OnCleaningProgress(float progress)
    {
        if (progress > 0f && hintArrow != null) hintArrow.SetActive(false);
        if (progress >= 1f && !readyToRinse) TransitionToRinse();
    }

    void ActivateSprite(SpriteRenderer sr, bool active)
    {
        if (sr != null) sr.gameObject.SetActive(active);
    }

    void TransitionToRinse()
    {
        readyToRinse = true;
        rinsing = true;
        ActivateSprite(dirtyTeethRenderer, false);
        ActivateSprite(gargleRenderer, true);
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
            if (shakeTimer >= shakeDurationNeeded) FinishRinsing();
        }
    }

    void FinishRinsing()
    {
        rinsing = false;
        levelDone = true;
        StartCoroutine(ShowCleanState());
    }

    IEnumerator ShowCleanState()
    {
        ActivateSprite(gargleRenderer, false);
        ActivateSprite(cleanTeethRenderer, true);
        yield return new WaitForSeconds(2f);
        QuizManager.LoadQuizForLevel(levelID, quizSceneName);
    }
}