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

/// <summary>
/// A class to support the automatic management of Projectors in Unity
/// 
/// For Editor:
/// Add the file location of your intrinsic and extrinsic matrices in the editor
/// 
/// For Executables:
/// Store your matrices in the format [Int/Mat]<displayID>.dat inside a top level folder /CalibrationData
/// </summary>
/// 

namespace UnitySARCommon.Projector
{ 
    public class SARProjectorManager
    {
        [MenuItem("SAR/Projector Management/Create a SAR Projector...")]
        static void CreateSingleSARProjector()
        {
            if (EditorUtility.DisplayDialog("Do you want to create a new SARProjector?", "Are you sure you want to create a new projector?", "Yes", "No"))
            {
                GameObject parent = new GameObject("SAR Projectors");
                GameObject projector = new GameObject("Projector");
                SARProjector sar_projector = projector.AddComponent(typeof(SARProjector)) as SARProjector;
                projector.transform.parent = parent.transform;

                ActivateDisplays();
            }
        }

        [MenuItem("SAR/Projector Management/Create 2 SAR Projectors...")]
        static void CreateTwoSARProjectors()
        {
            if (EditorUtility.DisplayDialog("How many projectors do you require?", "Are you sure you want to create 2 new projectors?", "Yes", "No"))
            {
                GameObject parent = new GameObject("SAR Projectors");

                for (int i = 0; i < 2; i++)
                {
                    GameObject projector = new GameObject("Projector " + (i + 1));
                    SARProjector sar_projector = projector.AddComponent(typeof(SARProjector)) as SARProjector;
                    projector.GetComponent<Camera>().targetDisplay = (i);
                    projector.transform.parent = parent.transform;
                }

                ActivateDisplays();
            }
        }

        [MenuItem("SAR/Projector Management/Create 4 SAR Projectors...")]
        static void CreateFoiurSARProjectors()
        {
            if (EditorUtility.DisplayDialog("How many projectors do you require?", "Are you sure you want to create 4 new projectors?", "Yes", "No"))
            {
                GameObject parent = new GameObject("SAR Projectors");

                for (int i = 0; i < 4; i++)
                {
                    GameObject projector = new GameObject("Projector " + (i + 1));
                    SARProjector sar_projector = projector.AddComponent(typeof(SARProjector)) as SARProjector;
                    projector.GetComponent<Camera>().targetDisplay = (i);
                    projector.transform.parent = parent.transform;
                }

                ActivateDisplays();
            }
        }

        static void ActivateDisplays()
        {
            for (int i = 0; i < UnityEngine.Display.displays.Length; i++)
            {
                if (!UnityEngine.Display.displays[i].active)
                    UnityEngine.Display.displays[i].Activate();
            }
        }


    }
}
