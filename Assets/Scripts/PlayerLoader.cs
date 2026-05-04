using UnityEngine;

/// Reads the character choice stored in PlayerPrefs and assigns the matching
/// sprite to this object's SpriteRenderer at startup.
public class PlayerLoader : MonoBehaviour
{
    [Header("Character Sprites")]
    public Sprite spriteChar1;
    public Sprite spriteChar2;
    public Sprite spriteChar3;

    void Start()
    {
        int choice = PlayerPrefs.GetInt("characterChosen", 0);
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.sprite = choice switch
        {
            0 => spriteChar1,
            1 => spriteChar2,
            _ => spriteChar3,
        };
    }
}
