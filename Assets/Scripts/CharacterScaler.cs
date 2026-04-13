using UnityEngine;

/// Attach to DirtyTeeth, CleanTeeth and GargleChild.
/// Scales the sprite so its height matches the camera's visible height exactly,
/// then centres it on the camera. Works on any screen size / resolution.
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterScaler : MonoBehaviour
{
    [Tooltip("0-1: fraction of screen height the sprite should occupy. 1 = full screen height.")]
    public float heightFraction = 1f;

    [Tooltip("Vertical offset in world units after scaling (positive = up).")]
    public float verticalOffset = 0f;

    void Start()
    {
        Scale();
    }

    public void Scale()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        // Reset scale so bounds reflect natural sprite size
        transform.localScale = Vector3.one;

        float spriteHeight = sr.bounds.size.y;
        if (spriteHeight <= 0f) return;

        float visibleHeight = Camera.main.orthographicSize * 2f;
        float targetHeight  = visibleHeight * heightFraction;

        float scale = targetHeight / spriteHeight;
        transform.localScale = new Vector3(scale, scale, 1f);

        // Centre on camera
        transform.position = new Vector3(
            Camera.main.transform.position.x,
            Camera.main.transform.position.y + verticalOffset,
            transform.position.z
        );
    }
}