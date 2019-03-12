/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
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

namespace UnitySARCommon.Editor
{
   	/// <summary>
    /// Creates, checks, and manages layers in the editor.
    /// </summary>
    public static class LayerCreatorEditor
    {
        public static void CreateNewLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
                throw new System.ArgumentNullException("layerName", "New layer name string is either null or empty.");

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layerProperties = tagManager.FindProperty("layers");
            int propertyCount = layerProperties.arraySize;

            SerializedProperty firstEmptyProp = null;

            for (int i = 0; i < propertyCount; i++)
            {
                SerializedProperty layerProperty = layerProperties.GetArrayElementAtIndex(i);

                string stringValue = layerProperty.stringValue;

                if (stringValue.Equals(layerName))
                {
                    // layer already exists
                    return;
                }

                if (i < 8 || stringValue != string.Empty)
                {
                    // skip the first 8 layers (system based)
                    // continue until we find an empty one
                    continue;
                }

                if (firstEmptyProp == null)
                {
                    firstEmptyProp = layerProperty;
                }
            }

            if (firstEmptyProp == null)
            {
                UnityEngine.Debug.LogError("Maximum limit of " + propertyCount + " layers exceeded. Layer \"" + layerName + "\" not created.");
                return;
            }
            else
            {
                UnityEngine.Debug.LogError("Layer Created!");
            }

            firstEmptyProp.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
        }

        public static void RemoveExistingLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
                throw new System.ArgumentNullException("layerName", "New layer name string is either null or empty.");

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layerProperties = tagManager.FindProperty("layers");
            int propertyCount = layerProperties.arraySize;
            bool deleted = false;

            for (int i = 0; i < propertyCount; i++)
            {
                SerializedProperty layerProperty = layerProperties.GetArrayElementAtIndex(i);

                string stringValue = layerProperty.stringValue;

                if (i < 8 || stringValue.Equals(string.Empty))
                {
                    // skip the first 8 layers (system based)
                    // continue until we find an empty one
                    continue;
                }
                else if (stringValue.Equals(layerName))
                {
                    layerProperty.stringValue = null;
                    layerProperty = null;
                    deleted = true;
                    break;
                }
            }

            if (!deleted)
            {
                UnityEngine.Debug.LogError("Layer \"" + layerName + "\" not found");
                return;
            }
            else
            {
                UnityEngine.Debug.LogError("Layer \"" + layerName + "\" deleted");
            }

            tagManager.ApplyModifiedProperties();
        }

        public static bool CheckForLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
                throw new System.ArgumentNullException("layerName", "New layer name string is either null or empty.");

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layerProperties = tagManager.FindProperty("layers");
            int propertyCount = layerProperties.arraySize;

            for (int i = 0; i < propertyCount; i++)
            {
                SerializedProperty layerProperty = layerProperties.GetArrayElementAtIndex(i);

                string stringValue = layerProperty.stringValue;

                if (i < 8 || stringValue.Equals(string.Empty))
                {
                    continue;
                }
                else if (stringValue.Equals(layerName))
                {
                    return true;
                }
            }

            return false;
        }
    }
    

}
#endif

