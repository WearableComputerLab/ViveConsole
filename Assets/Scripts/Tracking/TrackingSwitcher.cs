using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySARCommon.Tracking;

public class TrackingSwitcher : MonoBehaviour {

    private TrackingController controller;

     // VRPN
    private TrackerTransform VRPNTracker;
    private Tracker_Button_Server VRPNButton;

    // SteamVR
    private SARTracker sarTracker; 

	// Use this for initialization
	void Start () {
		controller = FindObjectOfType<TrackingController>();
        VRPNTracker = GetComponent<TrackerTransform>();
        VRPNButton = GetComponent<Tracker_Button_Server>();
        sarTracker = GetComponent<SARTracker>();
	}
	
	// Update is called once per frame
	void Update () {
		if(controller != null) {
            if(controller.VRPNTrackingEnabled == true) {
                VRPNTracker.enabled = true;
                VRPNButton.enabled = true;
                sarTracker.enabled = false;
            } else {
                VRPNTracker.enabled = false;
                VRPNButton.enabled = false;
                sarTracker.enabled = true;
            }
        }
	}
}
