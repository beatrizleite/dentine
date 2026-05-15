using System.Collections;
using UnityEngine;
using TMPro;

public class LevelTitleCard : MonoBehaviour
{
    public string levelLabel;
    public string levelTitle;

    public AudioClip narrationClip;

    public float holdDuration = 2.5f;
    public float fadeDuration = 0.8f;

    public TextMeshProUGUI labelText;
    public TextMeshProUGUI titleText;
    public CanvasGroup canvasGroup;

    public event System.Action OnDismissed;

    void Awake()
    {
        // Make sure the card is fully visible immediately, even before Start runs.
        // This prevents a flash of transparent on the first frame on Android.
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    void Start()
    {
        if (labelText != null) labelText.text = levelLabel;
        if (titleText != null) titleText.text = levelTitle;

        if (narrationClip != null)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.clip = narrationClip;
            src.playOnAwake = false;
            BackgroundMusic.Duck();
            src.Play();
            holdDuration = Mathf.Max(holdDuration, narrationClip.length + 0.3f);
        }

        // Wait one frame before starting so the canvas has rendered at least once.
        StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        yield return null; // one frame — ensures canvas is visible before countdown starts

        if (canvasGroup != null) canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        for (float t = 0f; t < fadeDuration;)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = 1f - t / fadeDuration;
            yield return null;
        }

        BackgroundMusic.Unduck();
        gameObject.SetActive(false);
        OnDismissed?.Invoke();
    }
}
