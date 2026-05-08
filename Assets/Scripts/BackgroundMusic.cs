using UnityEngine;

// Keeps music playing across scene loads. Destroys duplicates.
public class BackgroundMusic : MonoBehaviour
{
    static BackgroundMusic _instance;

    void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
