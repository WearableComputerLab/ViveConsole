using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


public class SAR_Testing_Script : MonoBehaviour {

    public string[] Devices;
    private CVRSystem vrSystem;

    // Use this for initialization
    void Start () {
		var error = EVRInitError.None;
        vrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);
        if (error != EVRInitError.None) { 
            // handle init error 
        }
	}
	
	// Update is called once per frame
	void Update () {
        ShowOpenVRDevices();
        QueryPose(2);
	}

    private void ShowOpenVRDevices() {
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++) {
            var deviceClass = vrSystem.GetTrackedDeviceClass(i);
            if (deviceClass != ETrackedDeviceClass.Invalid) {
                var deviceReading = deviceClass.ToString();
                Devices[i] = deviceReading;
                }
            }
     }

    private void QueryPose(int deviceID) {
        // this array can be reused 
        TrackedDevicePose_t[] allPoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, allPoses);
        var pose = allPoses[deviceID];
        if (pose.bPoseIsValid) {
            var absTracking = pose.mDeviceToAbsoluteTracking;
            var mat = new SteamVR_Utils.RigidTransform(absTracking);
            Debug.Log(mat.pos + " " + mat.rot);
        }
    }

}
