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

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
    }
}