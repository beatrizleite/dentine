using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject screenMain;     
    public GameObject screenLevels;   
    public GameObject screenOptions;  

    void Start()
    {
        GoHome(); 
    }

    public void GoHome()
    {
        screenMain.SetActive(true);
        screenLevels.SetActive(false);
        screenOptions.SetActive(false);
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
        SceneManager.LoadScene("Screen_Intro");
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