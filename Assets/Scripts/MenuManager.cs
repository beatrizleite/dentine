using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject screenMain;     
    public GameObject screenLevels;   
    public GameObject screenOptions;

    [Header("Video Intro")]
    public GameObject screenVideo;
    public VideoPlayer videoPlayer;

    void Start()
    {
        GoHome(); 

        if (screenVideo != null) screenVideo.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    public void GoHome()
    {
        screenMain.SetActive(true);
        screenLevels.SetActive(false);
        screenOptions.SetActive(false);
        if(screenVideo != null) screenVideo.SetActive(false);
    }

    public void GoToLevels()
    {
        screenMain.SetActive(false);
        screenLevels.SetActive(true);
        screenOptions.SetActive(false);
    }

    public void GoToOptions()
    {
        screenMain.SetActive(false);
        screenLevels.SetActive(false);
        screenOptions.SetActive(true);
    }

    public void GoToCharacterSelection()
    {
        SceneManager.LoadScene("Screen_CharacterSelect");
    }

    public void GoToIntro()
    {
        screenMain.SetActive(false);
        screenVideo.SetActive(true);
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        GoToCharacterSelection();
    }

    public void GoToLevel1()
    {
        SceneManager.LoadScene("Level1");
    }

    public void GoToLevel2()
    {
        SceneManager.LoadScene("Level2");
    }

    public void GoToLevel3()
    {
        SceneManager.LoadScene("Level3");
    }
}