#if UNITY_ANDROID
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;

// Remove a permissão INTERNET que o Unity acrescenta automaticamente ao manifest do Android.
// O Dentine não usa a rede, por isso a aplicação compilada não deve pedir esta permissão.
public class RemoveInternetPermission : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 999;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifest = Path.Combine(path, "src", "main", "AndroidManifest.xml");
        if (!File.Exists(manifest)) return;

        string xml = File.ReadAllText(manifest);
        string cleaned = Regex.Replace(xml,
            @"\s*<uses-permission[^>]*android\.permission\.INTERNET[^>]*/>", "");
        if (cleaned != xml) File.WriteAllText(manifest, cleaned);
    }
}
#endif
