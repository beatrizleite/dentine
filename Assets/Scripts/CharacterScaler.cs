using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterScaler : MonoBehaviour
{
    [Tooltip("0-1: fraction of screen height. 1 = full screen height.")]
    public float heightFraction = 1f;

    [Tooltip("Vertical offset in world units after scaling.")]
    public float verticalOffset = 0f;

    private bool scaled = false;

    void OnEnable()
    {
        // Scale every time the object becomes active
        Scale();
        scaled = true;
    }

    void Start()
    {
        if (!scaled) Scale();
    }

    public void Scale()
    {
        if (Camera.main == null) return;

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        transform.localScale = Vector3.one;

        float spriteHeight = sr.bounds.size.y;
        if (spriteHeight <= 0f) return;

        float visibleHeight = Camera.main.orthographicSize * 2f;
        float targetHeight  = visibleHeight * heightFraction;
        float scale         = targetHeight / spriteHeight;

        transform.localScale = new Vector3(scale, scale, 1f);
        transform.position = new Vector3(
            Camera.main.transform.position.x,
            Camera.main.transform.position.y + verticalOffset,
            transform.position.z);
    }
}