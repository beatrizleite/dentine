using UnityEngine;

/// Scales a SpriteRenderer so its height is a set fraction of the camera view height.
/// Called on OnEnable so it re-scales whenever the object becomes active mid-game.
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterScaler : MonoBehaviour
{
    [Tooltip("Fraction of screen height to fill (1 = full height).")]
    public float heightFraction = 1f;

    [Tooltip("Additional vertical offset in world units after scaling.")]
    public float verticalOffset = 0f;

    bool _scaled;

    void OnEnable()
    {
        Scale();
        _scaled = true;
    }

    void Start()
    {
        // Fallback in case OnEnable was already called before Start
        if (!_scaled) Scale();
    }

    public void Scale()
    {
        if (Camera.main == null) return;

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        // Reset first so we measure natural sprite height
        transform.localScale = Vector3.one;
        float sprH = sr.bounds.size.y;
        if (sprH <= 0f) return;

        float targetH = Camera.main.orthographicSize * 2f * heightFraction;
        float s       = targetH / sprH;

        transform.localScale = new Vector3(s, s, 1f);
        transform.position   = new Vector3(
            Camera.main.transform.position.x,
            Camera.main.transform.position.y + verticalOffset,
            transform.position.z);
    }
}
