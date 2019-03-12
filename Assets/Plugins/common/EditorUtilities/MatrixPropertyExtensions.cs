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


#if UNITY_EDITOR
using UnityEditor;
using Exception = System.Exception;

/// <summary>
/// Extensions to the SerializedProperty type to provide serialization and deserialization of Matrix4x4 objects.
/// </summary>
public static class MatrixPropertyExtensions
{
    /// <summary>
    /// Parse the property as a Matrix4x4. Returns a valid value when the propertyType is Matrix4x4f.
    /// </summary>
    /// <param name="matrixProperty">A serialized property of type Matrix4x4f.</param>
    /// <returns>A Matrix4x4 containing the values from the property.</returns>
    public static Matrix4x4 Matrix4x4Value(this SerializedProperty matrixProperty)
    {
        // Iterate over the matrix elements, each one should have a value "e<row><column>" in the property.
        var result = Matrix4x4.zero;
        for (var row = 0; row < 4; row++)
        {
            for (var column = 0; column < 4; column++)
            {
                var childPropertyPath = "e" + row + column;
                var elementProperty = matrixProperty.FindPropertyRelative(childPropertyPath); // Get internal matrix data members representing element.
                if (elementProperty == null)
                {
                    throw new Exception("Could not find child property " + childPropertyPath);
                }
                result[row, column] = elementProperty.floatValue; // Copy value from property to result Matrix4x4.
            }
        }
        return result;
    }

    /// <summary>
    /// Store the values from a Matrix4x4 in a property. Results in a valid property when the propertyType is Matrix4x4f.
    /// </summary>
    /// <param name="property">A serialized property of type Matrix4x4f.</param>
    /// <param name="matrix">A matrix, the values of which will be stored in property.</param>
    public static void Matrix4x4Value(this SerializedProperty property, Matrix4x4 matrix)
    {
        // As above in the setter. Will throw if the property doesn't have the expected internal data members.
        for (var row = 0; row < 4; row++)
        {
            for (var column = 0; column < 4; column++)
            {
                var childPropertyPath = "e" + row + column;
                var elementProperty = property.FindPropertyRelative(childPropertyPath);
                if (elementProperty == null)
                {
                    throw new Exception("Could not find child property " + childPropertyPath);
                }
                elementProperty.floatValue = matrix[row, column];
            }
        }
    }
}
#endif