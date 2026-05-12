using System.Collections;
using UnityEngine;
using TMPro;

// Full-screen title card shown at level start. Fires OnDismissed when fully gone.
public class LevelTitleCard : MonoBehaviour
{
    [Header("Titles")]
    public string levelLabel;
    public string levelTitle;

    [Header("Narration")]
    public AudioClip narrationClip;

    [Header("Timing")]
    public float holdDuration = 2.5f;
    public float fadeDuration = 0.8f;

    [Header("References")]
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI titleText;
    public CanvasGroup     canvasGroup;

    public event System.Action OnDismissed;

    void Start()
    {
        if (labelText != null) labelText.text = levelLabel;
        if (titleText != null) titleText.text = levelTitle;

        if (narrationClip != null)
        {
            var src        = gameObject.AddComponent<AudioSource>();
            src.clip       = narrationClip;
            src.playOnAwake = false;
            src.Play();
            holdDuration = Mathf.Max(holdDuration, narrationClip.length + 0.3f);
        }

        StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(holdDuration);

        for (float t = 0f; t < fadeDuration;)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - t / fadeDuration;
            yield return null;
        }

        gameObject.SetActive(false);
        OnDismissed?.Invoke();
    }
}
