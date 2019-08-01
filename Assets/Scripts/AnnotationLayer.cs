using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationLayer : MonoBehaviour
{
    public Shader annotateShader;
    private AnnotateTexture annotateTexture;

    public ModulePanel ParentPanel { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        // Create an AnnotateTexture component to draw into
        var annotationTextureGO = new GameObject("AnnotationTexture");
        annotationTextureGO.transform.SetParent(transform, false);
        //annotationTextureGO.transform.Translate(Vector3.back * 0.02f, Space.Self);
        annotateTexture = annotationTextureGO.AddComponent<AnnotateTexture>();
        annotateTexture.CreateCanvas(2000, 2000, annotateShader);
        //annotateTexture.enabled = false;

        // We can hold other annotations here too, such as audio or typed text
        
    }

    public void ClearAnnotation()
    {
        annotateTexture.ClearTexture();
    }

    public void ShowAnnotation()
    {
        annotateTexture.gameObject.SetActive(true);
    }

    public void HideAnnotation()
    {
        annotateTexture.gameObject.SetActive(false);
    }
}
