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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnitySARCommon.Display
{
    /// <summary>
    /// Create the famous WCL Box from the Editor menu.
    /// </summary>
    public class WCLBoxCreator
    {
        [MenuItem("SAR/Scene Assets/WCL Box...")]
        static void CreateWCLBoxProjector()
        {
            if (EditorUtility.DisplayDialog("Do you want to create a new SARBox?", "Are you sure you want to create a new box?", "Yes", "No"))
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = ("WCL Box");
                WCLBox wclcube = cube.AddComponent<WCLBox>();
                //cube.transform.position += wclcube.GetOffset();
                wclcube.UpdateSizeOnMeasurement();
            }
        }
    }
}
