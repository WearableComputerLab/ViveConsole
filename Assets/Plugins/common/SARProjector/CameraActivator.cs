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

using UnityEngine;

namespace UnitySARCommon.Projector
{
    /// <summary>
    /// Activates all cameras in a scene.
    /// </summary>
    public class CameraActivator : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            ActivateDisplays();
        }

        void ActivateDisplays()
        {
            for (int i = 0; i < UnityEngine.Display.displays.Length; i++)
            {
                if (!UnityEngine.Display.displays[i].active)
                    UnityEngine.Display.displays[i].Activate();
            }
        }
    }
}