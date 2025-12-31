using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    // This method is called when a character is selected
    public void SelectCharacter(int heroId)
    {
        PlayerPrefs.SetInt("characterChosen", heroId);
        PlayerPrefs.Save(); 

        // SceneManager.LoadScene("Screen_Intro");
        SceneManager.LoadScene("Level1");
    }
}