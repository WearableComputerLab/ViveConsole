using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySARCommon.Tracking;

[SelectionBase]
public class Hand_Grabber : MonoBehaviour {

    public GameObject hand_default;
    public GameObject hand_grab;
    public Tracker_Button_Server buttonHandler;
    private Transform grabbedObject;
    private Transform grabbedParent;
    public int grab = 0;
    private float timeOut = 0;
    private float y_lock = -99;

	// Update is called once per frame
	void Update () {
		if(buttonHandler != null) {
             if(buttonHandler.buttonState == true && timeOut <= 0){
                grab++;
                timeOut = 0.5f;
            }
            if(grab > 1) {
                grab = 0;
            }
            if(grab == 0) {
                hand_default.SetActive(true);
                hand_grab.SetActive(false);
                if(grabbedObject != null) {
                    Axis_Object a = grabbedObject.GetComponentInChildren<Axis_Object>();
                    if(a != null) {
                        a.GetComponent<MeshRenderer>().enabled = false;
                    }
                    grabbedObject.transform.parent = grabbedParent;
                    y_lock = -99;
                    grabbedObject = null;
                }
            }
            if(grab == 1) {
                hand_default.SetActive(false);
                hand_grab.SetActive(true);
                if(grabbedObject != null) {
                    if(y_lock == -99) {
                        y_lock = grabbedObject.transform.position.y;
                    }
                    grabbedObject.transform.position = new Vector3(grabbedObject.transform.position.x,y_lock,grabbedObject.transform.position.z);
                    grabbedObject.transform.parent = transform;


                }
            }
            if(timeOut > 0) {
                timeOut -= Time.deltaTime;
            }
        }
	}

    private void OnCollisionStay(Collision collision) {
        if(collision.gameObject.tag == "Grabbable" && grab == 1 && grabbedObject == null) {
            print("grab");
            grabbedObject = collision.transform;
            grabbedParent = grabbedObject.parent;
            Axis_Object a = grabbedObject.GetComponentInChildren<Axis_Object>();
            if(a != null) {
                a.GetComponent<MeshRenderer>().enabled = true;
            }
        }        
    }
}
