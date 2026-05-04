using UnityEngine;
using UnityEngine.SceneManagement;

/// Manages character selection on the character-select screen.
/// Highlights the chosen character, stores the choice in PlayerPrefs, and
/// advances to the next scene.
public class CharacterController : MonoBehaviour
{
    [Header("Characters")]
    public CharacterOption[] characterOptions;

    [Header("UI / Navigation")]
    public GameObject btnAdvance;
    public string     nextSceneName = "Level1";

    // How much to scale up the selected character
    const float SelectedScale = 1.1f;

    int _selectedId = -1;

    // ── Unity ─────────────────────────────────────────────────────────────

    void Start()
    {
        btnAdvance.SetActive(false);

        foreach (var opt in characterOptions)
        {
            opt.initialScale = opt.characterTransform.localScale;
            if (opt.lightObject != null) opt.lightObject.SetActive(false);
        }
    }

    // ── Public API (wired to UI buttons) ──────────────────────────────────

    /// Called by each character button with its unique id.
    public void SelectCharacter(int id)
    {
        _selectedId = id;
        btnAdvance.SetActive(true);

        foreach (var opt in characterOptions)
        {
            bool chosen = opt.id == id;
            if (opt.lightObject != null) opt.lightObject.SetActive(chosen);
            opt.characterTransform.localScale = chosen
                ? opt.initialScale * SelectedScale
                : opt.initialScale;
        }
    }

    /// Saves the choice and loads the next scene.
    public void ConfirmAndPlay()
    {
        if (_selectedId < 0) return;

        PlayerPrefs.SetInt("characterChosen", _selectedId);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nextSceneName);
    }
}

/// Serialisable data for a single selectable character.
[System.Serializable]
public class CharacterOption
{
    public string    name;
    public int       id;
    public Transform characterTransform;
    public GameObject lightObject;

    [HideInInspector] public Vector3 initialScale;
}
