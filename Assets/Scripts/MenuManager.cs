using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// Controls the main menu navigation: home, level select, options, and intro video.
public class MenuManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject screenMain;
    public GameObject screenLevels;
    public GameObject screenOptions;

    [Header("Intro Video")]
    public GameObject  screenVideo;
    public VideoPlayer videoPlayer;

    // ── Unity ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (screenVideo != null) screenVideo.SetActive(false);
        if (videoPlayer != null) videoPlayer.loopPointReached += OnVideoFinished;
        GoHome();
    }

    // ── Screen navigation ─────────────────────────────────────────────────

    public void GoHome()
    {
        SetScreens(main: true, levels: false, options: false, video: false);
    }

    public void GoToLevels()
    {
        SetScreens(main: false, levels: true, options: false, video: false);
    }

    public void GoToOptions()
    {
        SetScreens(main: false, levels: false, options: true, video: false);
    }

    /// Plays the intro video; navigates to character select when it ends.
    public void GoToIntro()
    {
        SetScreens(main: false, levels: false, options: false, video: true);
        videoPlayer?.Play();
    }

    public void GoToCharacterSelection() => SceneManager.LoadScene("Screen_CharacterSelect");
    public void GoToLevel1()             => SceneManager.LoadScene("Level1");
    public void GoToLevel2()             => SceneManager.LoadScene("Level2");
    public void GoToLevel3()             => SceneManager.LoadScene("Level3");

    // ── Helpers ───────────────────────────────────────────────────────────

    void SetScreens(bool main, bool levels, bool options, bool video)
    {
        if (screenMain    != null) screenMain.SetActive(main);
        if (screenLevels  != null) screenLevels.SetActive(levels);
        if (screenOptions != null) screenOptions.SetActive(options);
        if (screenVideo   != null) screenVideo.SetActive(video);
    }

    void OnVideoFinished(VideoPlayer vp) => GoToCharacterSelection();
}
