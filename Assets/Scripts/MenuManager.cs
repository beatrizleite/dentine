using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    public GameObject screenMain;
    public GameObject screenLevels;
    public GameObject screenOptions;
    public GameObject screenVideo;
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (screenVideo != null) screenVideo.SetActive(false);
        if (videoPlayer != null) videoPlayer.loopPointReached += _ => GoToCharacterSelection();
        GoHome();
    }

    public void GoHome()    => SetScreens(true,  false, false, false);
    public void GoToLevels() => SetScreens(false, true,  false, false);
    public void GoToOptions() => SetScreens(false, false, true,  false);

    public void GoToIntro()
    {
        SetScreens(false, false, false, true);
        videoPlayer?.Play();
    }

    public void GoToCharacterSelection() => SceneManager.LoadScene("Screen_CharacterSelect");
    public void GoToLevel1()             => SceneManager.LoadScene("Level1");
    public void GoToLevel2()             => SceneManager.LoadScene("Level2");
    public void GoToLevel3()             => SceneManager.LoadScene("Level3");

    void SetScreens(bool main, bool levels, bool options, bool video)
    {
        if (screenMain    != null) screenMain.SetActive(main);
        if (screenLevels  != null) screenLevels.SetActive(levels);
        if (screenOptions != null) screenOptions.SetActive(options);
        if (screenVideo   != null) screenVideo.SetActive(video);
    }
}
