using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTracker : MonoBehaviour {
    public Transform tracker;
    public Vector3 offsetPosition;
    public bool lockYAxis = false;
    public bool Smoothing = false;
    public float smoothingTime = 8f;

	// Update is called once per frame
	void Update () {
         if(tracker != null){
             if(Smoothing){
                transform.position = Vector3.Lerp(transform.position,tracker.position + offsetPosition, smoothingTime * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation,tracker.rotation,smoothingTime *Time.deltaTime);
             } else {
                transform.position = tracker.position + offsetPosition;
             }
             if(lockYAxis == true){
                transform.eulerAngles = new Vector3(0,tracker.eulerAngles.y,0);
             }
             if(transform.childCount > 0){
                 if(transform.GetChild(0).childCount > 0) {
                    transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                 }
             }
         }       
	}
}
