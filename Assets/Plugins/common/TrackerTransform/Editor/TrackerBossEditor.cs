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

namespace UnitySARCommon.Tracking.Editor
{
    /// <summary>
    /// Custom editor for TrackerBoss.
    /// </summary>
    [CustomEditor(typeof(TrackerBoss))]
    public class TrackerBossEditor : UnityEditor.Editor
    {
        // SerializedProperty objects mirroring data members of TrackerTransform.
        SerializedProperty trackerIpProperty;
        SerializedProperty matrixDirectoryProperty;
        SerializedProperty cstTransformMatrixProperty;
        SerializedProperty cstScaleFactorProperty;
        SerializedProperty cstFromFileProperty;
        SerializedProperty cstPathBaseProperty;
        SerializedProperty cstPathProperty;
        SerializedProperty cstMatrixProperty;

        private void OnEnable()
        {
            trackerIpProperty = serializedObject.FindProperty("trackerIp");
            matrixDirectoryProperty = serializedObject.FindProperty("matrixDirectory");
            cstTransformMatrixProperty = serializedObject.FindProperty("cstTransformMatrix");
            cstScaleFactorProperty = serializedObject.FindProperty("cstScaleFactor");
            cstFromFileProperty = serializedObject.FindProperty("cstFromFile");
            cstPathBaseProperty = serializedObject.FindProperty("cstPathBase");
            cstPathProperty = serializedObject.FindProperty("cstPath");
            cstMatrixProperty = serializedObject.FindProperty("cstMatrix");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.DelayedTextField(trackerIpProperty);
            EditorGUILayout.LabelField("Matrix directory");
            matrixDirectoryProperty.stringValue = EditorGUILayout.TextArea(matrixDirectoryProperty.stringValue);

            cstFromFileProperty.boolValue = EditorGUILayout.Toggle("Auto-load CST from file", cstFromFileProperty.boolValue);
            if (cstFromFileProperty.boolValue)
            {
                // If we're loading from a file, we need to define the file.
                var newBase = (RelativePathBase)EditorGUILayout.EnumPopup((RelativePathBase)cstPathBaseProperty.enumValueIndex);
                cstPathBaseProperty.enumValueIndex = (int)newBase;
                cstPathProperty.stringValue = EditorGUILayout.TextField("CST matrix path", cstPathProperty.stringValue);
                if (GUILayout.Button("Load")) // Button allows the user to load from the specified file in the editor.
                {
                    var basePath = "";
                    // For relative paths, we look at the users selection.
                    switch ((RelativePathBase)cstPathBaseProperty.enumValueIndex)
                    {
                        case RelativePathBase.ProjectAssets:
                            {
                                basePath = Application.dataPath;
                                break;
                            }
                        case RelativePathBase.TrackerBoss:
                            {
                                basePath = matrixDirectoryProperty.stringValue;
                                break;
                            }
                    }
                    LoadCstMatrix(System.IO.Path.Combine(basePath, cstPathProperty.stringValue)); // Load matrix from file.
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

            EditorGUI.BeginDisabledGroup(true);
            SAREditorGUI.Matrix4x4Field(cstMatrixProperty);
            EditorGUILayout.DelayedFloatField(cstScaleFactorProperty);
            SAREditorGUI.Matrix4x4Field(cstTransformMatrixProperty);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void LoadCstMatrix(string filePath)
        {
            // Read a matrix from file and apply it to the TrackerTransform as a CST matrix.
            var matrix = MatrixIO.ReadMatrixFromFile(filePath);
            (target as TrackerBoss).SetCstMatrix(matrix);
            // Re-serialize target, with the cstMatrix parameters updated.
            var newSerialized = new SerializedObject(target);
            // Save the new values of the matrix, and dependant fields, to the original serializedObject.
            cstMatrixProperty.Matrix4x4Value(matrix);
            serializedObject.CopyFromSerializedProperty(newSerialized.FindProperty("cstTransformMatrix"));
            serializedObject.CopyFromSerializedProperty(newSerialized.FindProperty("cstScaleFactor"));
        }
    }
}