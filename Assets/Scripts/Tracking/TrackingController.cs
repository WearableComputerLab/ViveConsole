using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingController : MonoBehaviour {

    // Controls whether tracking is done through VRPN or SteamVR
    public bool VRPNTrackingEnabled;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);
	}
	
}
