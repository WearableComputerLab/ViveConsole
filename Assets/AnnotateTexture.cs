using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnnotateTexture : MonoBehaviour//, IDragHandler, IBeginDragHandler
{
    MeshRenderer canvas;
    RenderTexture canvasTexture;
    Vector3 prevWorldPos;
    Material lineMaterial;
    readonly float lineWidth = 8;
    Color lastColor = Color.green;

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    RaycastHit hitInfo;
    //    Physics.Raycast(eventData.pressEventCamera.ScreenPointToRay(eventData.pointerCurrentRaycast.screenPosition), out hitInfo);
    //    prevWorldPos = new Vector3(hitInfo.textureCoord.x * 1000 - 500, hitInfo.textureCoord.y * 1000 - 500);
    //    //prevWorldPos = eventData.pointerCurrentRaycast.worldPosition;
    //    eventData.useDragThreshold = false;
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    var localPos = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
    //    var lastLocalPos = prevWorldPos; // transform.InverseTransformPoint(prevWorldPos);

    //    RaycastHit hitInfo;
    //    GetComponent<MeshCollider>().Raycast(eventData.pressEventCamera.ScreenPointToRay(eventData.pointerCurrentRaycast.screenPosition), out hitInfo, float.PositiveInfinity);
    //    localPos = new Vector3(hitInfo.textureCoord.x * 1000 - 500, hitInfo.textureCoord.y * 1000 - 500);
    //    var prevRT = RenderTexture.active;
    //    RenderTexture.active = canvasTexture;

    //    lineMaterial.SetPass(0);
    //    GL.PushMatrix();
    //    GL.LoadPixelMatrix(-500, 500, -500, 500);
    //    GL.MultMatrix(Matrix4x4.Translate(localPos * 1));
    //    GL.Begin(GL.QUADS);
    //    GL.Color(new Color(Random.Range(0, 1.0f), Random.Range(0,1f), Random.Range(0,1f)));
    //    //GL.Color(Color.green);
    //    var posDelta = (lastLocalPos - localPos) * 1;
    //    var width = Vector3.Cross(posDelta, Vector3.forward).normalized * lineWidth / 2;
    //    GL.Vertex(-width);
    //    GL.Vertex(posDelta - width);
    //    GL.Vertex(posDelta + width);
    //    GL.Vertex(width);
    //    GL.Vertex3(0, 0, 0);

    //    GL.End();
    //    GL.PopMatrix();

    //    RenderTexture.active = prevRT;

    //    prevWorldPos = localPos; // eventData.pointerCurrentRaycast.worldPosition;
    //}

    public void OnBeginRay(Ray ray)
    {
        Physics.Raycast(ray, out RaycastHit hitInfo);
        prevWorldPos = new Vector3(hitInfo.textureCoord.x * 1000 - 500, hitInfo.textureCoord.y * 1000 - 500);
        Color.RGBToHSV(lastColor, out float hue, out float sat, out float val);
        lastColor = Color.HSVToRGB(hue + 0.01f, sat, val);
    }

    public void OnUpdateRay(Ray ray)
    {
        //var localPos = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
        var lastLocalPos = prevWorldPos; // transform.InverseTransformPoint(prevWorldPos);

        GetComponent<MeshCollider>().Raycast(ray, out RaycastHit hitInfo, float.PositiveInfinity);
        var localPos = new Vector3(hitInfo.textureCoord.x * 1000 - 500, hitInfo.textureCoord.y * 1000 - 500);
        var prevRT = RenderTexture.active;
        RenderTexture.active = canvasTexture;

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(-500, 500, -500, 500);
        GL.MultMatrix(Matrix4x4.Translate(localPos * 1));
        { // OUTLINE
            GL.Begin(GL.QUADS);
            //GL.Color(new Color(Random.Range(0, 1.0f), Random.Range(0, 1f), Random.Range(0, 1f)));
            GL.Color(Color.black);
            var posDelta = (lastLocalPos - localPos) * 1;
            var width = Vector3.Cross(posDelta, Vector3.forward).normalized * (lineWidth + 4) / 2;
            GL.Vertex(-width);
            GL.Vertex(posDelta - width);
            GL.Vertex(posDelta + width);
            GL.Vertex(width);
            GL.Vertex3(0, 0, 0);

            GL.End();
        }
        { // FILL
            GL.Begin(GL.QUADS);
            //GL.Color(new Color(Random.Range(0, 1.0f), Random.Range(0, 1f), Random.Range(0, 1f)));
            GL.Color(lastColor);
            var posDelta = (lastLocalPos - localPos) * 1;
            var width = Vector3.Cross(posDelta, Vector3.forward).normalized * lineWidth / 2;
            GL.Vertex(-width);
            GL.Vertex(posDelta - width);
            GL.Vertex(posDelta + width);
            GL.Vertex(width);
            GL.Vertex3(0, 0, 0);

            GL.End();
        }
        GL.PopMatrix();

        RenderTexture.active = prevRT;

        prevWorldPos = localPos; // eventData.pointerCurrentRaycast.worldPosition;

    }


    // Start is called before the first frame update
    void Start()
    {
    }

    public void ClearTexture()
    {
        RenderTexture.active = canvasTexture;
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = null;
    }

    public void CreateCanvas(int width, int height, Shader shader = null)
    {
        var meshComponent = gameObject.AddComponent<MeshFilter>();
        var mesh = new Mesh();
        var vertOffset = (Vector3.up + Vector3.right) / 2;
        var verts = new List<Vector3>
        {
            new Vector3(-.5f, -.3f, 0), //Vector3.zero - vertOffset,
            new Vector3( .5f, -.3f, 0), //Vector3.right - vertOffset,
            new Vector3(-.5f,  .3f, 0), //Vector3.up - vertOffset,
            new Vector3( .5f,  .3f, 0), //Vector3.up + Vector3.right - vertOffset
        };
        mesh.SetVertices(verts);
        var uvs = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.SetUVs(0, uvs);
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        meshComponent.mesh = mesh;

        var collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        
        canvas = gameObject.AddComponent<MeshRenderer>();
        canvas.material = new Material(Shader.Find("Unlit/Transparent"));
        var mat = canvas.material;
        var tex = mat.mainTexture;
        //var drawableTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        var renderTextDesc = new RenderTextureDescriptor
        {
            width = tex?.width ?? width,
            height = tex?.height ?? height,
            colorFormat = RenderTextureFormat.ARGB32,
            depthBufferBits = 0,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            sRGB = true,
            autoGenerateMips = true,
            useMipMap = true,
            volumeDepth = 1,
            msaaSamples = 1,
        };
        //var drawableTexture = new RenderTexture(renderTextDesc);
        var drawableTexture = new RenderTexture(
            width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        drawableTexture.useMipMap = true;

        RenderTexture.active = drawableTexture;
        GL.Clear(false, true, Color.clear);
        RenderTexture.active = null;

        canvasTexture = drawableTexture;
        mat.mainTexture = drawableTexture;

        lineMaterial = new Material(shader ?? Shader.Find("Unlit/AnnotateShader"))
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        //mat_.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    // Update is called once per frame
    void Update()
    {
    }


}
