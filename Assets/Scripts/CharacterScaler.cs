using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterScaler : MonoBehaviour
{
    public float heightFraction = 1f;
    public float verticalOffset = 0f;

    bool _scaled;

    void OnEnable() { Scale(); _scaled = true; }
    void Start()    { if (!_scaled) Scale(); }

    public void Scale()
    {
        var cam = Camera.main;
        var sr  = GetComponent<SpriteRenderer>();
        if (cam == null || sr == null || sr.sprite == null) return;

        transform.localScale = Vector3.one;
        float sprH = sr.bounds.size.y;
        if (sprH <= 0f) return;

        float s = cam.orthographicSize * 2f * heightFraction / sprH;
        transform.localScale = new Vector3(s, s, 1f);
        transform.position   = new Vector3(cam.transform.position.x, cam.transform.position.y + verticalOffset, transform.position.z);
    }
}
