using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReportScreenshot : MonoBehaviour
{
    private static ReportScreenshot instance_;
    public static ReportScreenshot Instance { get { if (instance_ == null) CreateInstance(); return instance_; } }

    private Camera camera_;

    private static void CreateInstance()
    {
        var go = new GameObject("Screenshot Camera");
        instance_ = go.AddComponent<ReportScreenshot>();
        instance_.camera_ = go.AddComponent<Camera>();
        instance_.camera_.orthographic = true;
        instance_.gameObject.SetActive(false);
    }

    public static void TakeScreenshot(GameObject subject)
    {
        Instance.TakeScreenshotInternal(subject);
    }

    public void TakeScreenshotInternal(GameObject subject)
    {
        var bounds = subject.GetComponentInChildren<Collider>();

        transform.SetPositionAndRotation(subject.transform.position, subject.transform.rotation);
        transform.Translate(Vector3.back * 1, subject.transform);

        camera_.orthographicSize = bounds.transform.localScale.y / 2;

        var rt = RenderTexture.GetTemporary(new RenderTextureDescriptor(Mathf.FloorToInt(bounds.transform.localScale.x * 1000), Mathf.FloorToInt(bounds.transform.localScale.y * 1000), RenderTextureFormat.ARGB32));
        camera_.targetTexture = rt;
        camera_.Render();

        var pngScreenshot = rt.toTexture2D().EncodeToPNG();
        var screenshotPath = Path.Combine(Application.persistentDataPath, "screenshot.png");
        File.WriteAllBytes(screenshotPath, pngScreenshot);

        camera_.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);
    }

}

public static class ExtensionMethod
{
    public static Texture2D toTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        var oldRT = RenderTexture.active;
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = oldRT;
        return tex;
    }
}