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


using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// </summary>

namespace UnitySARCommon.Display
{
    public class CoordinateAxisCreator
    {
        [MenuItem("SAR/Scene Assets/Coordinate Axis...")]
        static void CreateCoordinateAxis()
        {
            if (EditorUtility.DisplayDialog("Do you want to create a new Coordinate Axis?", "Are you sure you want to create a new axis?", "Yes", "No"))
            {
                GameObject parent = new GameObject("Coordinate Axis");
                CoordinateAxis axis = parent.AddComponent(typeof(CoordinateAxis)) as CoordinateAxis;
                axis.transform.parent = parent.transform;
                axis.CreateCoordinateAxis(parent);
                axis.GetComponent<CoordinateAxis>().DrawCoordinateAxis();
            }
        }

    }
}

