/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2018 Daniel Jackson.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*/
using UnityEngine;

/// <summary>
/// Component to manage Projected Textures. Add Component to GameObject 
/// with Renderer and the provided Texture will be rendered into the 
/// scene as if from a media projector with intrinsic and extrinsic 
/// parameters defined by a Camera in the scene. Areas outside the camera's 
/// frustrum are not rendered to.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class ProjectiveTexture : MonoBehaviour
{
    // The projected texture will be projected from this viewport.
    [SerializeField]
    new public Camera camera;
    // The texture to project.
    [SerializeField]
    private Texture _texture;

    private Shader shader; // Cached Shader, loaded via shaderName.
    private Material material; // Material using above Shader.
    private int matrixPropId; // Id of shader property to assign view matrix.

    // The source Shader for the Material.
    private const string shaderName = "Hidden/Projective Texture";

    private void Start()
    {
        // Load Shader and construct Material.
        // Locate required shader properties and assign Material to Renderer.
        // If this initialization fails, log the error and disable the Component.
        try
        {
            shader = Shader.Find(shaderName);
            if (!shader) throw new System.Exception(string.Format("Could not find Shader \"{0}\"", shaderName));
            material = new Material(shader);
            matrixPropId = Shader.PropertyToID("_Projector_VP");
            GetComponent<Renderer>().sharedMaterials = new Material[] { material };
        }
        catch (System.Exception e)
        {
            enabled = false;
            throw e;
        }
    }

    private void OnValidate()
    {
        // When the Component is updated in the editor, apply altered values.
        if (material && material.mainTexture != _texture)
        {
            material.mainTexture = _texture;
        }
    }

    private void Update()
    {
        // If there is a valid Camera assigned, update Material properties on each frame.
        if (camera)
        {
            var vpMatrix = camera.projectionMatrix * camera.worldToCameraMatrix;
            material.SetMatrix(matrixPropId, vpMatrix);
        }
        else
        {
            // If no camera assigned, use identity matrix.
            material.SetMatrix(matrixPropId, Matrix4x4.identity);
        }
    }

    // Public accessors for Texture member.
    public Texture Texture
    {
        get { return _texture; }
        set
        {
            // When the Texture is changed, update the Material properties.
            _texture = value;
            if (material && material.mainTexture != _texture)
            {
                material.mainTexture = _texture;
            }
        }
    }
}
