using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleSelectionManager : MonoBehaviour
{
    [Range(0.001f, 0.1f)]
    public float borderThickness = 0.05f;
    public Material highlightMaterial;

    ConsoleModule selectedModule;
    GameObject highlight;

    public ConsoleModule SelectedModule
    {
        get => selectedModule;
        set {
            if (value == selectedModule) { return; }
            //RemoveHighlight();
            selectedModule = value;
            HighlightSelected();
        }
    }

    private void RemoveHighlight()
    {
        if (selectedModule == null)
            return;

        var renderers = selectedModule.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            var mats = r.materials;
            UnityEditor.Extension.ArrayUtility.RemoveAt(ref mats, mats.Length - 1);
            r.materials = mats;
        }
    }
    private void HighlightSelected()
    {
        highlight = highlight != null ? highlight : new GameObject("Highlight", typeof(MeshRenderer), typeof(MeshFilter));
        if (selectedModule == null)
        {
            highlight.SetActive(false);
            highlight.transform.SetParent(null, false);
            return;
        }
        highlight.SetActive(true);
        highlight.transform.SetParent(selectedModule.transform, false);

        //var renderers = selectedModule.GetComponentsInChildren<MeshRenderer>();
        //foreach (var r in renderers)
        //{
        //    var mats = r.materials;
        //    UnityEditor.Extension.ArrayUtility.Add(ref mats, highlightMaterial );
        //    r.materials = mats;
        //}

        // Build Mesh
        BuildMesh();
        //highlight.GetComponent<MeshFilter>().mesh = selectedModule.GetComponentInChildren<MeshFilter>().mesh;

        //var renderer = highlight.GetComponent<MeshRenderer>();
        //renderer.material = new Material(Shader.Find("Valve/VR/Silhouette"));
        //renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        //renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //renderer.receiveShadows = false;
        //renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

        var renderer = highlight.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
    }

    private void BuildMesh()
    {
        var bounds = selectedModule.GetComponentInChildren<BoxCollider>().size / 2;

        var corners = new Vector3[] {
            new Vector3(bounds.x, bounds.y), //bounds,
            new Vector3(bounds.x, -bounds.y), //bounds - (Vector3.up * bounds.y * 2),
            new Vector3(-bounds.x, -bounds.y), //-bounds,
            new Vector3(-bounds.x, bounds.y) //-bounds + (Vector3.up * bounds.y * 2)
        };

        for (int i = 0; i < corners.Length; ++i)
        {
            corners[i] = selectedModule.GetComponentInChildren<BoxCollider>().transform.TransformVector(corners[i]);
            corners[i] = selectedModule.transform.InverseTransformVector(corners[i]);
        }

        var vertices = new Vector3[corners.Length * 2];
        for (int i = 0; i < corners.Length; i++)
        {
            var c = corners[i];
            vertices[i] = new Vector3(c.x, c.y, 0);
        }

        for (int i = 0; i < corners.Length; i++)
        {
            int next = (i + 1) % corners.Length;
            int prev = (i + corners.Length - 1) % corners.Length;

            var nextSegment = (vertices[next] - vertices[i]).normalized;
            var prevSegment = (vertices[prev] - vertices[i]).normalized;

            var vert = vertices[i];
            vert += Vector3.Cross(nextSegment, Vector3.back) * borderThickness;
            vert += Vector3.Cross(prevSegment, Vector3.forward) * borderThickness;

            vertices[corners.Length + i] = vert;
        }

        var triangles = new int[]
        {
            0, 4, 1,
            1, 4, 5,
            1, 5, 2,
            2, 5, 6,
            2, 6, 3,
            3, 6, 7,
            3, 7, 0,
            0, 7, 4
        };

        var uv = new Vector2[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 1.0f)
        };

        var color = Color.blue;
        var colors = new Color[]
        {
            color,
            color,
            color,
            color,
            new Color(color.r, color.g, color.b, 0.0f),
            new Color(color.r, color.g, color.b, 0.0f),
            new Color(color.r, color.g, color.b, 0.0f),
            new Color(color.r, color.g, color.b, 0.0f)
        };

        var mesh = new Mesh();
        highlight.GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;
    }
}
