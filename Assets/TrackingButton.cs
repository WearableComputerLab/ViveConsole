using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingButton : MonoBehaviour {

    public TrackingButton otherTrackingButton;
    private MeshRenderer ButtonRenderer;
    public Color Active, Inactive;
    private bool trackingOn = false;
    public bool defaultTracker;
    private TrackingController trackingController;

	// Use this for initialization
	private void Start () {
		ButtonRenderer = GetComponent<MeshRenderer>();
        if (defaultTracker) {
            trackingOn = true;
        }
        trackingController = FindObjectOfType<TrackingController>();
	}
	
	// Update is called once per frame
	private void Update () {
        if (trackingOn) {
            ButtonRenderer.material.color = Active;
            if (defaultTracker) {
                trackingController.VRPNTrackingEnabled = false;
            } else {
                trackingController.VRPNTrackingEnabled = true;
            }
        } else {
            ButtonRenderer.material.color = Inactive;
        }
	}
    private void OnMouseOver() {
        ButtonRenderer.material.color = Active;
        if (Input.GetMouseButtonDown(0)) {
            trackingOn = true;
            otherTrackingButton.trackingOn = false;
        }
    }
    private void OnMouseExit() {
        ButtonRenderer.material.color = Inactive;
    }
 }
