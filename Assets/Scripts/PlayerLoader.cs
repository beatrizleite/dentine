using UnityEngine;

public class PlayerLoader : MonoBehaviour
{
    public Sprite spriteChar1;
    public Sprite spriteChar2;
    public Sprite spriteChar3;

    private SpriteRenderer myRender;

    void Start()
    {
        myRender = GetComponent<SpriteRenderer>();

        // Goes to the PlayerPrefs to get the chosen character
        int choice = PlayerPrefs.GetInt("characterChosen");

        if (choice == 0)
        {
            myRender.sprite = spriteChar1;
        }
        else if (choice == 1)
        {
            myRender.sprite = spriteChar2;
        }
        else
        {
            myRender.sprite = spriteChar3;
        }
    }
}