using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySARCommon.IO;
namespace UnitySARCommon.Tracking
{
    public class Tracker_Button_Server : MonoBehaviour {

        public string trackerID = "";//"Cube";

        [Header("Source data from TrackerBoss?")]
        public bool UseTrackerBoss = false;

        public string trackerIP = "";//"10.160.98.83";

        private TrackerBoss trackerBoss;
        public bool buttonState;

        
        void OnValidate()
        {
            if (UseTrackerBoss)
            {
                trackerBoss = FindObjectOfType<TrackerBoss>();
                if (trackerBoss != null)
                {
                    trackerIP = trackerBoss.trackerIp;
                }
            }
            else
            {
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            // Get buttonState from VRPN.
            buttonState = VRPN_Handler.vrpnButton(trackerID + "@" + trackerIP, 0);
        }
    }
}
