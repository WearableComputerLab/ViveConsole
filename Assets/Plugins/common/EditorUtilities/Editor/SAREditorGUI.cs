/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2018 Daniel Jackson.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*/

using UnityEditor;

namespace UnitySARCommon.Editor
{
    /// <summary>
    /// Class in the style of UnityEditor.EditorGUI. Contains methods for drawing inspector UI elements for types which are not supported by Unity, but which are useful for development of AR applications.
    /// </summary>
    public class SAREditorGUI
    {
        /// <summary>
        /// Display a 4x4 matrix of floating point values.
        /// </summary>
        /// <param name="matrixProperty">The matrix property the field shows. Updated if edited.</param>
        public static void Matrix4x4Field(SerializedProperty matrixProperty)
        {
            var matrixValue = matrixProperty.Matrix4x4Value();
            EditorGUILayout.LabelField(matrixProperty.displayName);
            for(int i = 0; i < 4; i ++)
            {
                EditorGUILayout.BeginHorizontal();
                var row = matrixValue.GetRow(i);
                for(int j = 0; j < 4; j ++)
                {
                    row[j] = EditorGUILayout.FloatField(row[j]);
                }
                matrixValue.SetRow(i, row);
                EditorGUILayout.EndHorizontal();
            }
            matrixProperty.Matrix4x4Value(matrixValue);
        }
    }
}