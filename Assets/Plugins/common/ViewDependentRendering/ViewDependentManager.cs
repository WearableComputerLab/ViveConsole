/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
*               2017 Daniel Jackson.
*
* If you use/extend/modify this code, add your name and email address
* to the AUTHORS file in the root directory.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*
*/

#if UNITY_EDITOR
using UnityEditor;
using UnitySARCommon.Editor;
#endif

using UnityEngine;

namespace UnitySARCommon.Rendering
{

    /// <summary>
    /// Generate and Manage a View Dependent Scene.
    /// An Off-Axis Camera captures the scene bounded by a Capture Plane to a RenderTexture.
    /// The result is applied to an associated Render Plane.
    /// </summary>
    public class ViewDependentManager : MonoBehaviour
    {

        [Header("Ground Plane Base Size")]
        public GameObject baseSize;

        [SerializeField] private Vector3 baseScale;

        [Header("Unique VDR Layer Names")]
        public string captureLayerString = "Capture Layer [VDR]";
        public string renderLayerString = "Render Layer";

        [Header("Top Level Container")]
        [SerializeField]
        private GameObject viewAreas;

        [Header("Individual VDR GameObjects")]
        public GameObject captureArea;
        public GameObject renderArea;

        [Header("VDR Camera")]
        [SerializeField]
        private GameObject viewCamera;

        [Header("Captured Image from Camera")]
        [SerializeField]
        private RenderTexture renderTexture;

        [Header("Rendering Material")]
        [SerializeField]
        private Material renderMaterial;

        [Header("Uncheck to Recreate entire VDR Scene")]
        public bool keepCreatedComponents = true;

        public void Bootstrap()
        {
            // Name GameObject Component
            this.name = "View Dependent Manager";

            #if UNITY_EDITOR
            // Create Required Layers
            if (!LayerCreatorEditor.CheckForLayer(captureLayerString))
            {
                LayerCreatorEditor.CreateNewLayer(captureLayerString);
            }

            if (!LayerCreatorEditor.CheckForLayer(renderLayerString))
            {
                LayerCreatorEditor.CreateNewLayer(renderLayerString);
            }
            #endif

            if (baseSize)
            {
                GameObject baseSizeCopy = new GameObject();
                baseSizeCopy.transform.position = baseSize.transform.position;
                baseSizeCopy.transform.rotation = baseSize.transform.rotation;
                baseSizeCopy.transform.localScale = baseSize.transform.localScale;

                baseSizeCopy.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                if (baseSize.GetComponent<MeshFilter>().sharedMesh.ToString().Equals("Plane (UnityEngine.Mesh)"))
                    baseScale = baseSizeCopy.transform.localScale;
                else
                    baseScale = Vector3.one;

                GameObject boundingVolume = new GameObject("WIM Bounding Volume");
                boundingVolume.transform.parent = this.transform.parent;
                boundingVolume.transform.localScale = baseSize.transform.localScale;
                BoxCollider b = boundingVolume.AddComponent<BoxCollider>();
                b.size = new Vector3(1, 2.5f, 1);
                b.center = new Vector3(0, b.size.y * 0.5f, 0);

                DestroyImmediate(baseSizeCopy);


            }

            if (!keepCreatedComponents)
            {
                // Create the Render Texture & Material
                if (renderTexture != null)
                {
                    DestroyImmediate(renderTexture);
                    DestroyImmediate(renderMaterial);
                }

                renderTexture = new RenderTexture(2048, 2048, 16, RenderTextureFormat.ARGB32);
                renderTexture.name = "Render Area Texture";
                renderTexture.Create();

                renderMaterial = new Material(Shader.Find("Diffuse"));
                renderMaterial.SetTexture("_MainTex", renderTexture);

                // Create the View Camera
                if (viewCamera != null)
                {
                    DestroyImmediate(viewCamera);
                }

                viewCamera = new GameObject();
                viewCamera.name = "View Dependent Camera";
                viewCamera.AddComponent<ViewDependentCamera>();
                viewCamera.GetComponent<ViewDependentCamera>().ApplyRenderTexture(renderTexture);
                viewCamera.GetComponent<ViewDependentCamera>().SetRenderMask(captureLayerString);
                viewCamera.transform.parent = this.transform;

                // Destroy the View Area Containers
                if (viewAreas != null)
                {
                    DestroyImmediate(viewAreas);
                }

                if (captureArea != null)
                {
                    DestroyImmediate(captureArea);
                }

                if (renderArea != null)
                {
                    DestroyImmediate(renderArea);
                }

                viewAreas = new GameObject("View Dependent Planes");
                viewAreas.transform.parent = this.transform;

                captureArea = GameObject.CreatePrimitive(PrimitiveType.Plane);
                captureArea.name = "Capture Area";
                captureArea.transform.parent = viewAreas.transform;
                captureArea.layer = LayerMask.NameToLayer(captureLayerString);
                captureArea.GetComponent<MeshRenderer>().materials = new Material[0];

                renderArea = GameObject.CreatePrimitive(PrimitiveType.Plane);
                renderArea.name = "Render Area";
                renderArea.transform.parent = viewAreas.transform;
                renderArea.transform.localScale = new Vector3(renderArea.transform.localScale.x * -1, renderArea.transform.localScale.y, renderArea.transform.localScale.z * -1);
                renderArea.GetComponent<Renderer>().material = renderMaterial;
                renderArea.layer = LayerMask.NameToLayer(renderLayerString);

                if (baseSize)
                {
                    captureArea.transform.position = baseSize.transform.position;
                    captureArea.transform.rotation = baseSize.transform.rotation;
                    captureArea.transform.localScale = baseScale;

                    renderArea.transform.position = baseSize.transform.position;
                    renderArea.transform.rotation = baseSize.transform.rotation;
                    renderArea.transform.localScale = baseScale;
                }
                
            }
            else
            {
                if (renderTexture == null)
                {
                    renderTexture = new RenderTexture(2048, 2048, 16, RenderTextureFormat.ARGB32);
                    renderTexture.name = "Render Area Texture";
                    renderTexture.Create();

                    renderMaterial = new Material(Shader.Find("Diffuse"));
                    renderMaterial.SetTexture("_MainTex", renderTexture);
                }

                if (viewCamera == null)
                {
                    viewCamera = new GameObject();
                    viewCamera.name = "View Dependent Camera";
                    viewCamera.AddComponent<ViewDependentCamera>();
                    viewCamera.GetComponent<ViewDependentCamera>().ApplyRenderTexture(renderTexture);
                    viewCamera.transform.parent = this.transform;
                }

                if (viewAreas == null)
                {
                    viewAreas = new GameObject("View Dependent Planes");
                    viewAreas.transform.parent = this.transform;
                }

                if (captureArea == null)
                {
                    captureArea = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    captureArea.name = "Capture Area";
                    captureArea.transform.parent = viewAreas.transform;
                    captureArea.layer = LayerMask.NameToLayer(captureLayerString);
                    captureArea.GetComponent<MeshRenderer>().materials = new Material[0];

                    if (baseSize)
                    {
                        captureArea.transform.position = baseSize.transform.position;
                        captureArea.transform.rotation = baseSize.transform.rotation;
                        captureArea.transform.localScale = baseScale;
                    }
                }

                if (renderArea == null)
                {
                    renderArea = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    renderArea.name = "Render Area";
                    renderArea.transform.parent = viewAreas.transform;
                    renderArea.transform.localScale = new Vector3(renderArea.transform.localScale.x * -1, renderArea.transform.localScale.y, renderArea.transform.localScale.z * -1);
                    renderArea.GetComponent<Renderer>().material = renderMaterial;
                    renderArea.layer = LayerMask.NameToLayer(renderLayerString);

                    if (baseSize)
                    {
                        renderArea.transform.position = baseSize.transform.position;
                        renderArea.transform.rotation = baseSize.transform.rotation;
                        renderArea.transform.localScale = baseScale;
                    }
                }
            }

            #if UNITY_EDITOR
            if (!(captureArea.GetComponent<MeshFilter>().sharedMesh.ToString().Equals("Plane (UnityEngine.Mesh)")))
            {
                captureArea = null;
                EditorUtility.DisplayDialog("Invalid GameObject Attached", "View Dependent Operations Require a Plane Mesh", "OK");
                viewCamera.GetComponent<OffAxisCamera>().SetImagePlane(captureArea);
            }
            else
            {
                viewCamera.GetComponent<OffAxisCamera>().SetImagePlane(captureArea);
            }

            if (!(renderArea.GetComponent<MeshFilter>().sharedMesh.ToString().Equals("Plane (UnityEngine.Mesh)")))
            {
                renderArea = null;
                EditorUtility.DisplayDialog("Invalid GameObject Attached", "View Dependent Operations Require a Plane Mesh", "OK");
                viewCamera.GetComponent<OffAxisCamera>().SetImagePlane(renderArea);
            }
            #endif
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            // Only Planes are allowed!
            //if (!(captureArea.GetComponent<MeshFilter>().sharedMesh.ToString().Equals("Plane (UnityEngine.Mesh)")))
            //{
            //    captureArea = null;
            //    EditorUtility.DisplayDialog("Invalid GameObject Attached", "View Dependent Operations Require a Plane Mesh", "OK");
            //}
            //else
            //{
                //viewCamera.GetComponent<OffAxisCamera>().SetImagePlane(captureArea);
            //}

            //if (!(renderArea.GetComponent<MeshFilter>().sharedMesh.ToString().Equals("Plane (UnityEngine.Mesh)")))
            //{
            //    renderArea = null;
            //    EditorUtility.DisplayDialog("Invalid GameObject Attached", "View Dependent Operations Require a Plane Mesh", "OK");
            //    viewCamera.GetComponent<OffAxisCamera>().SetImagePlane(renderArea);
            //}
                //viewCamera.GetComponent<OffAxisCamera>().SetImagePlane(captureArea);
#endif
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(ViewDependentManager))]
    [CanEditMultipleObjects]
    public class ViewDependentManagerEditor : UnityEditor.Editor
    {
        ViewDependentManager script;

        void OnEnable()
        {
            script = (ViewDependentManager)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUILayout.Button("Prepare Components"))
            {
                script.Bootstrap();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
} 
