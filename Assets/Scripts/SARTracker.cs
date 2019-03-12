using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
///     Attach to an object in unity to move and rotate with trackers, controllers, HMDs, etc.
/// </summary>
public class SARTracker : MonoBehaviour {

    // Devices in VR System
    public string[] Devices = new string[10];

    // The Device ID to be tracked
    public int DeviceID;

    // GenericTracker References
    public int[] genericTrackers = new int[10];

    // Reference to the VR System
    private CVRSystem vrSystem;

    public Vector3 trackerPos;

    
	void Start () {
        // define an error
		var error = EVRInitError.None;

        // Initialize OpenVR with Application Type as other
        vrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);

        // If there is an error, handle the error
        if (error != EVRInitError.None) { 
            
        }
	}
	
	void Update () {
        ShowOpenVRDevices();
        QueryPose(DeviceID);
        
	}

    // show vr devices (hmd,tracker..etc) and populate in string array
    private void ShowOpenVRDevices() {
        int index = 0;
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++) {
            var deviceClass = vrSystem.GetTrackedDeviceClass(i);
            if (deviceClass == ETrackedDeviceClass.GenericTracker) {
                var deviceReading = deviceClass.ToString();
                Devices[index] = deviceReading;
                genericTrackers[index] = (int)i;
                index++;
            }
        }
     }

    // Query a Pose and then assign the pose
    private void QueryPose(int deviceID) {
        // this array can be reused 
        TrackedDevicePose_t[] allPoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        
        
        vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, allPoses);
        

        var pose = allPoses[genericTrackers[deviceID]];

        if (pose.bPoseIsValid) {
            var absTracking = pose.mDeviceToAbsoluteTracking;
            var mat = new SteamVR_Utils.RigidTransform(absTracking);
            // Assign positions
            transform.position = mat.pos;
            trackerPos = mat.pos;
            transform.rotation = mat.rot;

        }
    }

    // Returns whether the button is pressed from the vive tracker
    public bool isPressingButton() {
        SteamVR_Controller.Device Controller = SteamVR_Controller.Input(DeviceID);
        return Controller.GetHairTriggerDown();
    }

}
