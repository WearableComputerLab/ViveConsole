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
using UnityEditor;
using MatrixIO = UnitySARCommon.IO.MatrixIO;
using SAREditorGUI = UnitySARCommon.Editor.SAREditorGUI;
using RelativePathBase = UnitySARCommon.Tracking.TrackerTransform.RelativePathBase;
using Axis = UnitySARCommon.Tracking.TrackerTransform.Axis;

namespace UnitySARCommon.Tracking.Editor
{
    /// <summary>
    /// Custom editor for TrackerTransform.
    /// </summary>
    [CustomEditor(typeof(TrackerTransform))]
    public class TrackerTransformEditor : UnityEditor.Editor
    {
        // SerializedProperty objects mirroring data members of TrackerTransform.
        SerializedProperty updateInEditor;
        SerializedProperty trackerId;
        SerializedProperty useBoss;
        SerializedProperty trackerBoss;
        SerializedProperty ipFromBoss;
        SerializedProperty trackerIp;
        SerializedProperty cstFromBossProp;
        SerializedProperty cstFromFileProp;
        SerializedProperty cstPathBaseProp;
        SerializedProperty cstPath;
        SerializedProperty cstMatrix;
        SerializedProperty cstTransformMatrix;
        SerializedProperty cstScaleFactor;
        SerializedProperty offsetFromFileProp;
        SerializedProperty offsetPathBaseProp;
        SerializedProperty offsetPath;
        SerializedProperty offsetMatrix;
        SerializedProperty offsetTranslate;
        SerializedProperty offsetRotation;
        SerializedProperty axis;

        public void OnEnable()
        {
            InitializeSerializedProperties();
        }

        private void InitializeSerializedProperties()
        {
            // Find all the properties of the serialized TrackerTransform.
            updateInEditor = serializedObject.FindProperty("updateInEditor");
            trackerId = serializedObject.FindProperty("trackerId");
            useBoss = serializedObject.FindProperty("useBoss");
            trackerBoss = serializedObject.FindProperty("trackerBoss");
            ipFromBoss = serializedObject.FindProperty("ipFromBoss");
            trackerIp = serializedObject.FindProperty("trackerIp");
            cstFromBossProp = serializedObject.FindProperty("cstFromBoss");
            cstFromFileProp = serializedObject.FindProperty("cstFromFile");
            cstPathBaseProp = serializedObject.FindProperty("cstPathBase");
            cstPath = serializedObject.FindProperty("cstPath");
            cstMatrix = serializedObject.FindProperty("cstMatrix");
            cstTransformMatrix = serializedObject.FindProperty("cstTransformMatrix");
            cstScaleFactor = serializedObject.FindProperty("cstScaleFactor");
            offsetFromFileProp = serializedObject.FindProperty("offsetFromFile");
            offsetPathBaseProp = serializedObject.FindProperty("offsetPathBase");
            offsetPath = serializedObject.FindProperty("offsetPath");
            offsetMatrix = serializedObject.FindProperty("offsetMatrix");
            offsetTranslate = serializedObject.FindProperty("offsetTranslate");
            offsetRotation = serializedObject.FindProperty("offsetRotation");
            axis = serializedObject.FindProperty("axis");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Update the serialized representation of the target.
            var trackerTransform = target as TrackerTransform;
            var sceneBoss = FindObjectOfType<TrackerBoss>(); // First TrackerBoss in the scene. Used if some values inherit from the boss, but no boss object explicitly defined.

            updateInEditor.boolValue = EditorGUILayout.Toggle("Update in editor", updateInEditor.boolValue);
            trackerTransform.runInEditMode = updateInEditor.boolValue;
            // If VRPN is driving the transform in edit mode, disable the transform editor.
            if (updateInEditor.boolValue)
            {
                trackerTransform.transform.hideFlags |= HideFlags.NotEditable;
            }
            else
            {
                trackerTransform.transform.hideFlags &= ~HideFlags.NotEditable;
            }
            EditorGUILayout.DelayedTextField(trackerId);
            var shouldUseBoss = EditorGUILayout.Toggle("Inherit values from Tracker Boss", useBoss.boolValue);
            useBoss.boolValue = shouldUseBoss;
            if (useBoss.boolValue)
            {
                // If using boss, optionally use boss value for ip.
                EditorGUILayout.ObjectField(trackerBoss, typeof(TrackerBoss));
                var shouldUseBossIp = EditorGUILayout.Toggle("IP from Tracker Boss", ipFromBoss.boolValue);
                ipFromBoss.boolValue = shouldUseBossIp;
            }
            else
            {
                ipFromBoss.boolValue = false;
            }
            if(!ipFromBoss.boolValue)
            {
                // Hide if driven by boss.
                EditorGUILayout.DelayedTextField(trackerIp);
            }

            /* Setup CST Matrix */
            if (useBoss.boolValue)
            {
                // If using boss, optionally use boss value for cst.
                cstFromBossProp.boolValue = EditorGUILayout.Toggle("Inherit CST matrix from TrackerBoss", cstFromBossProp.boolValue);
            }
            else
            {
                cstFromBossProp.boolValue = false;
            }
            if(!cstFromBossProp.boolValue)
            {
                cstFromFileProp.boolValue = EditorGUILayout.Toggle("Auto-load CST from file", cstFromFileProp.boolValue);
                if (cstFromFileProp.boolValue)
                {
                    // If we're loading from a file, we need to define the file.
                    var newBase = (RelativePathBase)EditorGUILayout.EnumPopup((RelativePathBase)cstPathBaseProp.enumValueIndex);
                    cstPathBaseProp.enumValueIndex = (int)newBase;
                    cstPath.stringValue = EditorGUILayout.TextField("CST matrix path", cstPath.stringValue);
                    if (GUILayout.Button("Load")) // Button allows the user to load from the specified file in the editor.
                    {
                        var basePath = "";
                        // For relative paths, we look at the users selection.
                        switch ((RelativePathBase)cstPathBaseProp.enumValueIndex)
                        {
                            case RelativePathBase.ProjectAssets:
                                {
                                    basePath = Application.dataPath;
                                    break;
                                }
                            case RelativePathBase.TrackerBoss:
                                {
                                    // First check if the user specified a boss. If not, find one in the scene.
                                    if (trackerBoss.objectReferenceValue == null)
                                    {
                                        if (sceneBoss == null)
                                        {
                                            EditorUtility.DisplayDialog("Load Error", "No TrackerBoss in scene.", "OK");
                                        }
                                        else
                                        {
                                            basePath = sceneBoss.MatrixDirectory;
                                        }
                                    }
                                    else
                                    {
                                        basePath = (trackerBoss.objectReferenceValue as TrackerBoss).MatrixDirectory;
                                    }
                                    break;
                                }
                        }
                        LoadCstMatrix(System.IO.Path.Combine(basePath, cstPath.stringValue)); // Load matrix from file.
                    }
                }
                else
                {
                    if (GUILayout.Button("Load..."))
                    {
                        // Load matrix using a file chooser. Matrix loading is not performed in build.
                        var path = EditorUtility.OpenFilePanelWithFilters("Open File", Application.dataPath, new string[] { "Data files", "dat", "All files", "*" });
                        LoadCstMatrix(path);
                    }
                }
                // Draw matrix and derived parameters, these shouldn't be editred directly, but should be loaded from a file.
                EditorGUI.BeginDisabledGroup(true);
                SAREditorGUI.Matrix4x4Field(cstMatrix);
                EditorGUILayout.DelayedFloatField(cstScaleFactor);
                SAREditorGUI.Matrix4x4Field(cstTransformMatrix);
                EditorGUI.EndDisabledGroup();
            }

            /* Setup offset Matrix */
            offsetFromFileProp.boolValue = EditorGUILayout.Toggle("Auto-load offset from file", offsetFromFileProp.boolValue);
            if (offsetFromFileProp.boolValue)
            {
                var newBase = (RelativePathBase)EditorGUILayout.EnumPopup((RelativePathBase)offsetPathBaseProp.enumValueIndex);
                offsetPathBaseProp.enumValueIndex = (int)newBase;
                offsetPath.stringValue = EditorGUILayout.TextField("Offset matrix path", offsetPath.stringValue);
                if (GUILayout.Button("Load"))
                {
                    var basePath = "";
                    switch ((RelativePathBase)offsetPathBaseProp.enumValueIndex)
                    {
                        case RelativePathBase.ProjectAssets:
                            {
                                basePath = Application.dataPath;
                                break;
                            }
                        case RelativePathBase.TrackerBoss:
                            {
                                if (trackerBoss.objectReferenceValue == null)
                                {
                                    if (sceneBoss == null)
                                    {
                                        EditorUtility.DisplayDialog("Load Error", "No TrackerBoss in scene.", "OK");
                                    }
                                    else
                                    {
                                        basePath = sceneBoss.MatrixDirectory;
                                    }
                                }
                                else
                                {
                                    basePath = (trackerBoss.objectReferenceValue as TrackerBoss).MatrixDirectory;
                                }
                                break;
                            }
                    }
                    LoadOffsetMatrix(System.IO.Path.Combine(basePath, offsetPath.stringValue));
                }
            }
            else
            {
                if (GUILayout.Button("Load..."))
                {
                    var path = EditorUtility.OpenFilePanelWithFilters("Open File", Application.dataPath, new string[] { "Data files", "dat", "All files", "*" });
                    LoadOffsetMatrix(path);
                }
            }
            EditorGUI.BeginDisabledGroup(true);
            SAREditorGUI.Matrix4x4Field(offsetMatrix);
            EditorGUILayout.Vector3Field(offsetTranslate.displayName, offsetTranslate.vector3Value);
            EditorGUILayout.Vector3Field(offsetRotation.displayName, offsetRotation.quaternionValue.eulerAngles);
            EditorGUI.EndDisabledGroup();

            var newAxis = (Axis)EditorGUILayout.EnumPopup("Flip Handedness", (Axis)axis.enumValueIndex);
            axis.enumValueIndex = (int)newAxis;

            // Apply the new properties and create an Undo record.
            serializedObject.ApplyModifiedProperties();
        }

        private void LoadCstMatrix(string filePath)
        {
            // Read a matrix from file and apply it to the TrackerTransform as a CST matrix.
            var matrix = MatrixIO.ReadMatrixFromFile(filePath);
            (target as TrackerTransform).SetCstMatrix(matrix);
            // Re-serialize target, with the cstMatrix parameters updated.
            var newSerialized = new SerializedObject(target);
            // Save the new values of the matrix, and dependant fields, to the original serializedObject.
            cstMatrix.Matrix4x4Value(matrix);
            serializedObject.CopyFromSerializedProperty(newSerialized.FindProperty("cstTransformMatrix"));
            serializedObject.CopyFromSerializedProperty(newSerialized.FindProperty("cstScaleFactor"));
        }

        private void LoadOffsetMatrix(string filePath)
        {
            // Read a matrix from file and apply it to the TrackerTransform as a offset matrix.
            var matrix = MatrixIO.ReadMatrixFromFile(filePath);
            (target as TrackerTransform).SetOffsetMatrix(matrix);
            // Re-serialize target, with the offsetMatrix parameters updated.
            var newSerialized = new SerializedObject(target);
            // Save the new values of the matrix, and dependant fields, to the original serializedObject.
            offsetMatrix.Matrix4x4Value(matrix);
            serializedObject.CopyFromSerializedProperty(newSerialized.FindProperty("offsetTranslate"));
            serializedObject.CopyFromSerializedProperty(newSerialized.FindProperty("offsetRotation"));
        }
    }
}