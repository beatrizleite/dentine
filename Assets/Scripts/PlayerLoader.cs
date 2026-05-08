using UnityEngine;

public class PlayerLoader : MonoBehaviour
{
    public Sprite spriteChar1;
    public Sprite spriteChar2;
    public Sprite spriteChar3;

    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        int choice = PlayerPrefs.GetInt("characterChosen", 0);
        sr.sprite = choice switch
        {
            0 => spriteChar1,
            1 => spriteChar2,
            _ => spriteChar3,
        };
    }
}
