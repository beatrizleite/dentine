using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{
    [Header("Character Options")]
    public CharacterOption[] characterOptions;

    [Header("UI")]
    public GameObject btnAdvance;
    public string nextSceneName = "Level1";

    private int selectedId = -1; 
    private readonly float scaleFactor = 1.1f;

    void Start()
    {
        // Start with the "Advance" button hidden
        btnAdvance.SetActive(false);

        // Ensure no character has the light on at the start
        foreach (var charOpt in characterOptions)
        {
            charOpt.initialScale = charOpt.characterTransform.localScale;

            if(charOpt.lightObject != null) charOpt.lightObject.SetActive(false);
            
            charOpt.characterTransform.localScale = charOpt.initialScale;
        }
    }

    // Call this function when you click on a character (Character Button)
    public void SelectCharacter(int id)
    {
        selectedId = id;

        // Show the "Advance" button
        btnAdvance.SetActive(true);

        // Update the visual of ALL characters
        foreach (var charOpt in characterOptions)
        {
            if (charOpt.id == id)
            {
                if(charOpt.lightObject != null) charOpt.lightObject.SetActive(true);
                
                charOpt.characterTransform.localScale = charOpt.initialScale * scaleFactor;
            }
            else
            {
                if(charOpt.lightObject != null) charOpt.lightObject.SetActive(false);
                
                charOpt.characterTransform.localScale = charOpt.initialScale;
            }
        }
    }

    public void ConfirmAndPlay()
    {
        if (selectedId != -1)
        {
            // Now we save the definitive choice
            PlayerPrefs.SetInt("characterChosen", selectedId);
            PlayerPrefs.Save();

            // Change scene
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

[System.Serializable]
public class CharacterOption
{
    public string name;
    public int id;
    public Transform characterTransform;
    public GameObject lightObject;
    [HideInInspector] public Vector3 initialScale;
}
