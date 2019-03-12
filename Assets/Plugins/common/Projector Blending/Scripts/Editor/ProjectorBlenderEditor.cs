/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2018 Daniel Jackson.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*/
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnitySARCommon.Rendering;

namespace UnitySARCommon.Projector
{
    [CustomEditor(typeof(ProjectorBlender))]
    public class ProjectorBlenderEditor : UnityEditor.Editor
    {
        private SerializedProperty initMode;
        private SerializedProperty filePath;

        private const BindingFlags privateMemberBinding = BindingFlags.NonPublic | BindingFlags.Instance;

        private void OnEnable()
        {
            InitializeComponentRefs();
            initMode = serializedObject.FindProperty("initMode");
            filePath = serializedObject.FindProperty("filePath");
        }

        // Call this to set up references in target to attached Camera and MaskEffect Components. This prevents null references as these are normally initialized on Start.
        private void InitializeComponentRefs()
        {
            var targetObject = serializedObject.targetObject as Component;
            var type = targetObject.GetType();
            var cam = type.GetField("cam", privateMemberBinding);
            var maskEffect = type.GetField("maskEffect", privateMemberBinding);
            cam.SetValue(targetObject, targetObject.GetComponent<Camera>());
            maskEffect.SetValue(targetObject, targetObject.GetComponent<MaskEffect>());
        }

        // Invokes the ApplyMask(Texture2D) method on the Editor's target object.
        private void ApplyMaskImage(Texture2D img)
        {
            var targetObject = serializedObject.targetObject as Component;
            var type = targetObject.GetType();
            var applyMaskMethod = type.GetMethod("ApplyMask", new System.Type[] { typeof(Texture2D) });
            Undo.RecordObject(targetObject.GetComponent<MaskEffect>(), "Apply MaskEffect");
            applyMaskMethod.Invoke(serializedObject.targetObject, new object[] { img });
        }

        // Invokes the IEnumerator GenerateMask() method on the Editor's target object, shows a cancelable progress bar.
        private void GenerateMaskImage()
        {
            var targetObject = serializedObject.targetObject as ProjectorBlender;
            var type = targetObject.GetType();
            var generateMaskMethod = type.GetMethod("GenerateMask", privateMemberBinding);
            try
            {
                // Display a progress bar and run GenerateMask using the IEnumerator interface. Runs one frame worth of the alorithm then updates the progress bar.
                //  Clicking cancel will stop the process.
                //  An exception in the algorithm will clear the progress bar and display an error.
                var process = generateMaskMethod.Invoke(targetObject, new object[] { }) as IEnumerator;
                var cancel = false;
                while (process.MoveNext() && !cancel)
                {
                    var text = ""; // cast.x + ", " + cast.y
                    cancel = EditorUtility.DisplayCancelableProgressBar("Generating Mask", text, targetObject.GenerateProgress());
                }
                EditorUtility.ClearProgressBar();
                EditorUtility.SetDirty(targetObject.gameObject);
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error Generating Mask", e.Message, "Ok");
            }
        }

        public override void OnInspectorGUI()
        {
            // Serialize the current object state.
            serializedObject.Update();
            // Get the object and its type.
            var targetObject = serializedObject.targetObject as ProjectorBlender;
            var type = targetObject.GetType();

            EditorGUILayout.LabelField("Automatic Initialization");
            EditorGUI.indentLevel++;
            {
                // Popup allows user to select what type of initialization should occur at runtime.
                var enumNames = initMode.enumDisplayNames;
                var enumOld = initMode.enumValueIndex;
                var enumNew = EditorGUILayout.Popup(enumOld, enumNames);
                if (enumOld != enumNew)
                {
                    initMode.enumValueIndex = enumNew;
                }
                var enumType = type.GetNestedType("InitMode", System.Reflection.BindingFlags.NonPublic);
                if ((int)System.Enum.Parse(enumType, "File") == enumNew)
                {
                    // If initializing from file, provide field for the path to the mask.
                    filePath.stringValue = EditorGUILayout.TextField("File path", filePath.stringValue);
                    // Button to load a mask from the given file path and apply it in the editor.
                    if (GUILayout.Button("Load"))
                    {
                        // Search path priority: absolute path in filePath, base path from the first ProjectorBlendBoss in scene, application data path.
                        var manager = FindObjectOfType<ProjectorBlendBoss>();
                        var path = filePath.stringValue;
                        if (manager != null)
                        {
                            path = System.IO.Path.Combine(manager.maskBasePath, path);
                        }
                        path = System.IO.Path.Combine(Application.persistentDataPath, path);
                        if (System.IO.File.Exists(path))
                        {
                            var bytes = System.IO.File.ReadAllBytes(path);
                            var tex = new Texture2D(0, 0);
                            tex.LoadImage(bytes);
                            tex.filterMode = FilterMode.Point;
                            ApplyMaskImage(tex);
                            // Force all other Components on gameObject to update their Editors.
                            EditorUtility.SetDirty(targetObject.gameObject);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error loading file.", "File \"" + path + "\" does not exist.", "Ok");
                        }
                    }
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Projector Mask");
            EditorGUI.indentLevel++;
            {
                // Generate a new mask for this projector in the editor.
                if (GUILayout.Button("Generate"))
                {
                    GenerateMaskImage();
                }
                var mask = targetObject.GetComponent<MaskEffect>().Mask;
                // If there is a mask, have a button to clear it.
                EditorGUI.BeginDisabledGroup(mask == null);
                if (GUILayout.Button("Clear Mask"))
                {
                    ApplyMaskImage(null);
                }
                EditorGUI.EndDisabledGroup();
                // If there is a mask, draw it in the inspector.
                if (mask != null)
                {
                    EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(false, 128), mask, null, ScaleMode.ScaleToFit);
                }
                // If there is a mask, allow the user to save it to a PNG.
                EditorGUI.BeginDisabledGroup(mask == null);
                {
                    if (GUILayout.Button("Save Mask..."))
                    {
                        // Default path is derived in the same way as the path to load from on Start (see above)
                        var basePath = Application.persistentDataPath;
                        var manager = FindObjectOfType<ProjectorBlendBoss>();
                        if (manager != null)
                        {
                            if (!string.IsNullOrEmpty(manager.maskBasePath))
                            {
                                basePath = manager.maskBasePath;
                            }
                        }
                        var defaultName = "mask";
                        if (!string.IsNullOrEmpty(filePath.stringValue))
                        {
                            defaultName = filePath.stringValue;
                        }
                        basePath = System.IO.Path.Combine(basePath, defaultName);
                        defaultName = System.IO.Path.GetFileNameWithoutExtension(basePath);
                        basePath = System.IO.Path.GetDirectoryName(basePath);
                        var file = EditorUtility.SaveFilePanel("Save Mask as PNG", basePath, defaultName, "png");
                        if (!string.IsNullOrEmpty(file))
                        {
                            var bytes = mask.EncodeToPNG();
                            System.IO.File.WriteAllBytes(file, bytes);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                // Button to load a mask bitmap from a PNG.
                if (GUILayout.Button("Load Mask..."))
                {
                    // Default path is derived in the same way as the path to load from on Start (see above)
                    var basePath = Application.persistentDataPath;
                    var manager = FindObjectOfType<ProjectorBlendBoss>();
                    if (manager != null)
                    {
                        if (!string.IsNullOrEmpty(manager.maskBasePath))
                        {
                            basePath = manager.maskBasePath;
                        }
                    }
                    var defaultName = "mask";
                    if (!string.IsNullOrEmpty(filePath.stringValue))
                    {
                        defaultName = filePath.stringValue;
                    }
                    basePath = System.IO.Path.Combine(basePath, defaultName);
                    basePath = System.IO.Path.GetDirectoryName(basePath);
                    var file = EditorUtility.OpenFilePanelWithFilters("Load PNG as Mask", basePath, new string[] { "Supported Images", "png", "All files", "*" });
                    if (!string.IsNullOrEmpty(file))
                    {
                        var bytes = System.IO.File.ReadAllBytes(file);
                        var tex = new Texture2D(0, 0);
                        tex.LoadImage(bytes);
                        tex.filterMode = FilterMode.Point;
                        ApplyMaskImage(tex);
                    }
                }
            }
            EditorGUI.indentLevel--;
            // Apply modifications and creates an entry in the Undo queue.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
