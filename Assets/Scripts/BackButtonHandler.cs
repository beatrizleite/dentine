using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class BackButtonHandler : MonoBehaviour
{
    [Header("ExitPopup Panel")]
    public GameObject exitPopup;

    [Header("Main Menu Scene Name")]
    public string menuSceneName = "DentineMenu"; 

    void Start()
    {
        if(exitPopup != null) exitPopup.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (exitPopup.activeSelf)
            {
                ClosePopup();
            }
            else
            {
                OpenPopup();
            }
        }
    }

    public void OpenPopup()
    {
        exitPopup.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosePopup()
    {
        exitPopup.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}