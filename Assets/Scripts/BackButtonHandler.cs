using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class BackButtonHandler : MonoBehaviour
{
    public GameObject exitPopup;
    public string menuSceneName = "DentineMenu";

    void Start()
    {
        if (exitPopup != null) exitPopup.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetPopupVisible(!exitPopup.activeSelf);
    }

    public void OpenPopup()  => SetPopupVisible(true);
    public void ClosePopup() => SetPopupVisible(false);

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    void SetPopupVisible(bool visible)
    {
        if (exitPopup != null) exitPopup.SetActive(visible);
        Time.timeScale = visible ? 0f : 1f;
    }
}
