using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DiplomaManager : MonoBehaviour
{
    public string menuScene = "DentineMenu";

    [Header("World-space text references")]
    public TextMeshPro titleTmp;
    public TextMeshPro congTmp;
    public TextMeshPro msgTmp;

    [Header("Size as fraction of camera height")]
    public float titleFraction = 0.40f;
    public float congFraction  = 0.24f;
    public float msgFraction   = 0.10f;

    void Start() => ScaleText();

    void ScaleText()
    {
        var cam = Camera.main;
        if (cam == null) return;
        float h = cam.orthographicSize * 2f;
        if (titleTmp != null) { titleTmp.fontSize = h * titleFraction; titleTmp.fontSizeMax = titleTmp.fontSize; }
        if (congTmp  != null) { congTmp.fontSize  = h * congFraction;  congTmp.fontSizeMax  = congTmp.fontSize;  }
        if (msgTmp   != null) { msgTmp.fontSize   = h * msgFraction;   msgTmp.fontSizeMax   = msgTmp.fontSize;   }
    }

    public void GoToMenu() => SceneManager.LoadScene(menuScene);

    public void DownloadDiploma() => StartCoroutine(SaveScreenshot());

    IEnumerator SaveScreenshot()
    {
        yield return new WaitForEndOfFrame();
        var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();
        byte[] png = tex.EncodeToPNG();
        Destroy(tex);

        string path = Path.Combine(Application.persistentDataPath, "Dentine_Diploma.png");
        File.WriteAllBytes(path, png);

#if UNITY_ANDROID && !UNITY_EDITOR
        using (var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent   = new AndroidJavaObject("android.content.Intent", "android.intent.action.MEDIA_SCANNER_SCAN_FILE"))
        using (var uriClass = new AndroidJavaClass("android.net.Uri"))
        using (var file     = new AndroidJavaObject("java.io.File", path))
        {
            intent.Call("setData", uriClass.CallStatic<AndroidJavaObject>("fromFile", file));
            activity.Call("sendBroadcast", intent);
        }
#endif
    }
}
