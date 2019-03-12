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
    /// Creates, checks, and manages tags in the editor.
    /// </summary>
    public static class TagCreatorEditor
    {
        public static void CreateNewTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
                throw new System.ArgumentNullException("tagName", "New tag name string is either null or empty.");

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            int propertyCount = tags.arraySize;

            SerializedProperty firstEmptyProp = null;
            int i;

            for (i = 0; i < propertyCount; i++)
            {
                SerializedProperty tagProperty = tags.GetArrayElementAtIndex(i);

                string stringValue = tagProperty.stringValue;

                if (stringValue.Equals(tagName))
                {
                    // tag already exists
                    return;
                }

                if (firstEmptyProp == null)
                {
                    firstEmptyProp = tagProperty;
                }
            }

            if (firstEmptyProp == null && propertyCount != 0)
            {
                UnityEngine.Debug.LogError("Maximum limit of " + propertyCount + " tags exceeded. Tag \"" + tagName + "\" not created.");
                return;
            }
            else
            {
                UnityEngine.Debug.LogError("Tag Created!");
            }

            tags.InsertArrayElementAtIndex(i);
            tags.GetArrayElementAtIndex(i).stringValue = tagName;

            tagManager.ApplyModifiedProperties();
        }

        public static void RemoveExistingTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
                throw new System.ArgumentNullException("tagName", "New tag name string is either null or empty.");

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            int propertyCount = tags.arraySize;
            bool deleted = false;

            for (int i = 0; i < propertyCount; i++)
            {
                SerializedProperty tag = tags.GetArrayElementAtIndex(i);

                string stringValue = tag.stringValue;

                if (stringValue.Equals(tagName))
                {
                    tag.stringValue = null;
                    tag = null;
                    deleted = true;
                    break;
                }
            }

            if (!deleted)
            {
                UnityEngine.Debug.LogError("Tag \"" + tagName + "\" not found");
                return;
            }
            else
            {
                UnityEngine.Debug.LogError("Tag \"" + tagName + "\" deleted");
            }

            tagManager.ApplyModifiedProperties();
        }

        public static bool CheckForTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
                throw new System.ArgumentNullException("tagName", "New tag name string is either null or empty.");

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            int propertyCount = tags.arraySize;


            for (int i = 0; i < propertyCount; i++)
            {
                SerializedProperty tag = tags.GetArrayElementAtIndex(i);

                string stringValue = tag.stringValue;

                if (stringValue.Equals(tagName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif

