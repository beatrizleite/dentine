using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// Handles the hardware/keyboard back button.
/// Shows an exit popup and pauses the game; routes to the main menu on confirm.
public class BackButtonHandler : MonoBehaviour
{
    [Header("Exit Popup")]
    public GameObject exitPopup;

    [Header("Scene Names")]
    public string menuSceneName = "DentineMenu";

    // ── Unity ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (exitPopup != null) exitPopup.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            SetPopupVisible(!exitPopup.activeSelf);
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void OpenPopup()  => SetPopupVisible(true);
    public void ClosePopup() => SetPopupVisible(false);

    /// Navigates back to the main menu and resumes time.
    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void SetPopupVisible(bool visible)
    {
        if (exitPopup != null) exitPopup.SetActive(visible);
        Time.timeScale = visible ? 0f : 1f;
    }
}
