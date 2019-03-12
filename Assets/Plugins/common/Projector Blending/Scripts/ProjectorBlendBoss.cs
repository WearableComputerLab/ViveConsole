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

namespace UnitySARCommon.Projector
{
    /* Global settings for ProjectorBlender components. */
    public class ProjectorBlendBoss : MonoBehaviour
    {
        [TextArea]
        public string maskBasePath; // Base path to locate mask texture images.

        // Triggers mask re-generation on all ProjectorBlender components. Non-blocking, but can take a LONG time.
        public void RegenerateBlendMasks()
        {
            var blenders = FindObjectsOfType<ProjectorBlender>();
            foreach (var blender in blenders)
            {
                blender.StartGeneratingBlendMask();
            }
        }
    }
}
