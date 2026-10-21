using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Level2Manager : MonoBehaviour
{
    public static Level2Manager instance;

    public SpriteRenderer dirtyTeethRenderer;
    public SpriteRenderer cleanTeethRenderer;
    public SpriteRenderer gargleRenderer;
    public SpriteRenderer dirtyTongueRenderer;
    public GameObject toothbrush;
    public HintManager hintManager;

    public string levelID       = "Level2";
    public string quizSceneName = "Quiz";

    public float shakeDurationNeeded = 2f;
    public float shakeThreshold      = 1.2f;

    public enum Phase { BrushTeeth, Rinse1, BrushTongue, Rinse2, Done }

    Phase _phase = Phase.BrushTeeth;
    public Phase CurrentPhase => _phase;

    bool _rinsing;
    bool _done;
    float _shakeTimer;

    void Awake() => instance = this;

    void Start()
    {
        ShowOnly(dirtyTeethRenderer);
        SetBrush(true);

        var card = Object.FindFirstObjectByType<LevelTitleCard>();
        if (card != null)
            card.OnDismissed += OnTitleCardDone;
        else
            OnTitleCardDone();
    }

    void OnTitleCardDone()
    {
        hintManager?.StartHinting();
    }

    void Update()
    {
        if (_rinsing && !_done) HandleShake();
    }

    public void OnScrubActivity() => hintManager?.OnScrubActivity();

    public void OnCleaningProgress(float progress)
    {
        if (progress >= 1f && _phase == Phase.BrushTeeth)
            StartCoroutine(GoToRinse1());
    }

    public void OnTongueProgress(float progress)
    {
        hintManager?.OnScrubActivity();
        if (progress >= 1f && _phase == Phase.BrushTongue)
            StartCoroutine(GoToRinse2());
    }

    IEnumerator GoToRinse1()
    {
        _phase = Phase.Rinse1;
        SetBrush(false);
        ShowOnly(gargleRenderer);
        StartRinse();
        yield return null;
    }

    IEnumerator GoToRinse2()
    {
        _phase = Phase.Rinse2;
        SetBrush(false);
        ShowOnly(gargleRenderer);
        StartRinse();
        yield return null;
    }

    void StartRinse()
    {
        _rinsing    = true;
        _shakeTimer = 0f;
        hintManager?.EnterRinsePhase();
    }

    void HandleShake()
    {
        bool shaking;
#if UNITY_EDITOR
        shaking = UnityEngine.InputSystem.Keyboard.current != null &&
                  UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed;
#else
        shaking = Input.acceleration.magnitude > shakeThreshold;
#endif
        if (!shaking) return;

        hintManager?.OnShakeActivity();
        _shakeTimer += Time.deltaTime;
        if (_shakeTimer >= shakeDurationNeeded) FinishRinse();
    }

    void FinishRinse()
    {
        _rinsing = false;

        if (_phase == Phase.Rinse1)
        {
            _phase = Phase.BrushTongue;
            ShowOnly(dirtyTongueRenderer);
            SetBrush(true);
            hintManager?.ExitRinsePhase();
        }
        else if (_phase == Phase.Rinse2)
        {
            _phase = Phase.Done;
            _done  = true;
            StartCoroutine(Finish());
        }
    }

    IEnumerator Finish()
    {
        SetBrush(false);
        ShowOnly(cleanTeethRenderer);
        yield return new WaitForSeconds(2f);
        QuizManager.LoadQuizForLevel(levelID, quizSceneName);
    }

    void SetBrush(bool active)
    {
        if (toothbrush != null) toothbrush.SetActive(active);
    }

    void ShowOnly(SpriteRenderer target)
    {
        SetActive(dirtyTeethRenderer, false);
        SetActive(cleanTeethRenderer, false);
        SetActive(gargleRenderer,     false);
        SetActive(dirtyTongueRenderer, false);
        if (target != null) target.gameObject.SetActive(true);
    }

    void SetActive(SpriteRenderer sr, bool active)
    {
        if (sr != null) sr.gameObject.SetActive(active);
    }
}
