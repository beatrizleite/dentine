using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Resets the scale and position
        transform.localScale = Vector3.one;

        // Image Size
        float width = sr.bounds.size.x;
        float height = sr.bounds.size.y;

        // Screen Size
        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        // Calculate Scale
        float scaleX = worldScreenWidth / width;
        float scaleY = worldScreenHeight / height;
        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1);

        // Center on the Camera
        transform.position = new Vector3(Camera.main.transform.position.x, 
                                         Camera.main.transform.position.y, 
                                         transform.position.z);
    }
}