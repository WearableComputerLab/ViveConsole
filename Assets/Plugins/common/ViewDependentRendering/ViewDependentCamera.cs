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
#endif
using UnityEngine;

namespace UnitySARCommon.Rendering
{
    [RequireComponent(typeof(Camera), typeof(OffAxisCamera))]
    [ExecuteInEditMode]
    public class ViewDependentCamera : MonoBehaviour
    {
        public bool useTracker;
        public GameObject externalTracker;

        void OnValidate()
        {
            this.GetComponent<Camera>().backgroundColor = new Color(255, 0, 0, 0);

            if (useTracker)
            {
                this.transform.position = externalTracker.transform.position;
                this.transform.rotation = externalTracker.transform.rotation;
            }
            else
            {
                this.transform.position = Vector3.zero + new Vector3(0, 5, 0);
                this.transform.rotation = Quaternion.identity;
            }
        }

        public void ApplyRenderTexture(RenderTexture renderTex)
        {
            this.GetComponent<Camera>().targetTexture = renderTex;
        }

        public void SetRenderMask(string layer)
        {
            this.GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer(layer);
        }

        void Update()
        {
            if (transform.hasChanged)
            {
                OnValidate();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ViewDependentCamera))]
    [CanEditMultipleObjects]
    public class ViewDependentCameraEditor : UnityEditor.Editor
    {
        ViewDependentCamera script;

        SerializedProperty editor_Tracker;
        SerializedProperty editor_UseTracker;

        void OnEnable()
        {
            script = (ViewDependentCamera)target;
            editor_Tracker = serializedObject.FindProperty("externalTracker");
            editor_UseTracker = serializedObject.FindProperty("useTracker");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(editor_UseTracker);
            EditorGUILayout.EndHorizontal();

            if (editor_UseTracker.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(editor_Tracker);
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
