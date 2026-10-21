using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{
    public CharacterOption[] characterOptions;
    public GameObject btnAdvance;
    public string nextSceneName = "Level1";

    const float SelectedScale = 1.1f;

    int _selectedId = -1;

    void Start()
    {
        btnAdvance.SetActive(false);
        foreach (var opt in characterOptions)
        {
            opt.initialScale = opt.characterTransform.localScale;
            if (opt.lightObject != null) opt.lightObject.SetActive(false);
        }
    }

    public void SelectCharacter(int id)
    {
        _selectedId = id;
        btnAdvance.SetActive(true);

        foreach (var opt in characterOptions)
        {
            bool chosen = opt.id == id;
            if (opt.lightObject != null) opt.lightObject.SetActive(chosen);
            opt.characterTransform.localScale = chosen ? opt.initialScale * SelectedScale : opt.initialScale;
        }
    }

    public void ConfirmAndPlay()
    {
        if (_selectedId < 0) return;
        PlayerPrefs.SetInt("characterChosen", _selectedId);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nextSceneName);
    }
}

[System.Serializable]
public class CharacterOption
{
    public string     name;
    public int        id;
    public Transform  characterTransform;
    public GameObject lightObject;
    [HideInInspector] public Vector3 initialScale;
}
