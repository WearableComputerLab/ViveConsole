using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Scale_Object : MonoBehaviour {
    [Range(0.0f, 1)]
    public float scale;
    private bool createdTexture = false;

    private void Update() {
        if(createdTexture == false) {
            CreateTexture();
            createdTexture = true;
        }

    }

    private void OnValidate() {
        //transform.localScale = new Vector3(scale,scale,scale);
        if (GetComponentInChildren<Axis_Object>()) {
            Transform axis = GetComponentInChildren<Axis_Object>().transform;
            axis.localScale = new Vector3(0.2f/scale,0.2f/scale,0.2f/scale);
        }
    }

    // Converts render texture to texture2D
    Texture2D RTImage(Camera cam) {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height,TextureFormat.ARGB32,false);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = currentRT;
        return image;
    }

     public enum BlendMode
     {
         Opaque,
         Cutout,
         Fade,
         Transparent
     }
 
     public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
     {
         switch (blendMode)
         {
             case BlendMode.Opaque:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                 standardShaderMaterial.SetInt("_ZWrite", 1);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = -1;
                 break;
             case BlendMode.Cutout:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                 standardShaderMaterial.SetInt("_ZWrite", 1);
                 standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 2450;
                 break;
             case BlendMode.Fade:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                 standardShaderMaterial.SetInt("_ZWrite", 0);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 3000;
                 break;
             case BlendMode.Transparent:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                 standardShaderMaterial.SetInt("_ZWrite", 0);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 3000;
                 break;
         }
 
     }

    public void CreateTexture() {
        transform.localScale = new Vector3(0.05f,0.05f,0.05f);
        // set up the camera with an orthographic view
        GameObject g = new GameObject("Camera");
        Camera c = g.AddComponent<Camera>();
        g.transform.parent = transform;
        g.transform.localPosition = new Vector3(0,20,0);
        g.transform.localEulerAngles = new Vector3(90,0,0);
        c.orthographic = true;
        c.orthographicSize = 0.04f;
        c.cullingMask = 1 << LayerMask.NameToLayer("Furniture");
        c.clearFlags = CameraClearFlags.SolidColor;

        // Set all children to be of layer Furniture
        for(int i = 0; i < transform.childCount; i++) {
            if(transform.GetChild(i).GetComponent<Axis_Object>() == false) {
                transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Furniture");
            }
        }
        // Set up render texture
        RenderTexture r = new RenderTexture(1024 ,1024 ,24,RenderTextureFormat.ARGB32);
        c.targetTexture = r;
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = transform;
        quad.transform.localPosition = Vector3.zero;
        quad.transform.localEulerAngles = new Vector3(90,0,0);
        quad.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
        quad.name = "Texture";
        quad.layer = LayerMask.NameToLayer("Texture");
        Texture2D t = RTImage(c);
        MeshRenderer meshr = quad.GetComponent<MeshRenderer>();
        ChangeRenderMode(meshr.material,BlendMode.Cutout);
        meshr.material.mainTexture = t;
        meshr.material.color = new Color(0.7f,0.7f,0.7f,1);
        Destroy(g);
        transform.localScale = new Vector3(scale,scale,scale);
        Floor f = FindObjectOfType<Floor>();
        transform.parent = f.transform;
    }


}

