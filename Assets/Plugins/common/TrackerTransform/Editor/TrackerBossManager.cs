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

namespace UnitySARCommon.Tracking
{
    /// <summary>
    /// Support the automatic management of Tracking Data in Unity.
    /// </summary>
    public class TrackerBossManager
    {
        [MenuItem("SAR/Tracking Management/Create a Tracker Boss...")]
        static void CreateTrackerBoss()
        {
            if (EditorUtility.DisplayDialog("Do you want to create a new Tracker Boss?", "You can store global properties to use in TrackerTransforms in the scene: \n -- CST Matrix Location \n -- IP Address", "Yes", "No"))
            {
                TrackerBoss instance = GameObject.FindObjectOfType<TrackerBoss>();

                if (instance == null)
                {
                    GameObject parent = new GameObject("System Management");
                    GameObject boss = new GameObject("TrackerBoss");
                    TrackerBoss trackerBoss = boss.AddComponent(typeof(TrackerBoss)) as TrackerBoss;
                    boss.transform.parent = parent.transform;
                    instance = trackerBoss;
                }
                else
                {
                    Debug.Log("A Tracker Boss already exists!");
                }
            }
        }
    }
}
