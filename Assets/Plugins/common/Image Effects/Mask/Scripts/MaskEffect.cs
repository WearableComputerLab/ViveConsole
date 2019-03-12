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

namespace UnitySARCommon.Rendering
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    /* Image Effect. Attach to a gameobject with a Camera. Will mask the render buffer based on the given mask textures red channel.
     * Mask value of 0.0 will output fragment of Camera clear color. Mask value of 1.0 will pass through fragment from Camera's 
     *  render buffer. Intermediate values will output fragment with color linearly interpolated between the two values.
     * Requires a Camera component on game object.
     */
    public class MaskEffect : MonoBehaviour
    {
        private const string maskShader = "Hidden/UnitySARCommon/Mask"; // Name of the shader to apply to the render buffer.

        [SerializeField]
        private Texture2D _mask; // The mask texture.
        public Texture2D Mask
        {
            /* Public getter */
            get
            {
                return _mask;
            }
            /* Public setter. Logs warning when mask size does not match render buffer size.
             * Sets the mask texture to be used to mask the render buffer. A null value will apply a pass-through 
             *  mask which has no effect on output.
             */
            set
            {
                if (value != null)
                {
                    if (value.width != cam.pixelWidth || value.height != cam.pixelHeight)
                    {
                        Debug.LogWarning("Mask dimensions do not match camera dimensions. " +
                            "This could cause image quality degradation.", this);
                    }
                }
                else
                {
                    Debug.LogWarning("Null mask, applying identity mask.", this);
                }
                _mask = value;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(gameObject); // If we're in the editor, make sure the change is reflected.
#endif
            }
        }

        private Material material; // Material to apply to the render buffer, based on maskShader.
        private Camera cam; // Attached Camera.
        private Texture2D nulMask; // Pass through mask. 1x1 texture, all white.

        private void Awake()
        {
            cam = GetComponent<Camera>(); // Cache Camera reference.
            nulMask = new Texture2D(1, 1); // Generate pass through texture.
            nulMask.SetPixel(0, 0, Color.white); // Pass through texture is all white.
            nulMask.Apply();
        }

        private void Start()
        {
            var shader = Shader.Find(maskShader); // Find shader by name.
            if (shader == null)
            {
                Debug.LogError("Could not load shader \"" + maskShader + "\"", this); // Log error and disable component if shader missing.
                enabled = false;
                return;
            }
            material = new Material(shader); // Create material using shader.
            if (material == null)
            {
                Debug.LogError("Could not initialize material.", this); // Log error and disable if material can't be created.
                enabled = false;
                return;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // If Mask is null, use the pass through mask. Else use Mask.
            if (Mask != null)
            {
                material.SetTexture("_Mask", Mask);
            }
            else
            {
                material.SetTexture("_Mask", nulMask);
            }
            material.SetColor("_ClearColor", cam.backgroundColor); // Pull clear color from attached Camera.
            Graphics.Blit(source, destination, material); // Filter render buffer.
        }
    }
}