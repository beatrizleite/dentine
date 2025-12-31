using UnityEngine;

public class PlayerLoader : MonoBehaviour
{
    public Sprite spriteChar1;  // 0
    public Sprite spriteChar2;  // 1
    public Sprite spriteChar3;  // 2

    private SpriteRenderer meuRender;

    void Start()
    {
        meuRender = GetComponent<SpriteRenderer>();

        // Goes to the PlayerPrefs to get the chosen character
        int choice = PlayerPrefs.GetInt("characterChosen");

        if (choice == 0)
        {
            meuRender.sprite = spriteChar1;
        }
        else if (choice == 1)
        {
            meuRender.sprite = spriteChar2;
        }
        else
        {
            meuRender.sprite = spriteChar3;
        }
    }
}