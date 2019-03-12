using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySARCommon.Tracking;

[SelectionBase]
public class BubbleCursor : MonoBehaviour {

    public Transform cursor_default;
    public Tracker_Button_Server buttonHandler;
    public Camera rayCamera;
    public LayerMask furnitureLayer;
    public LayerMask floorLayer;

    private Transform grabbedObject;
    private Transform grabbedParent;
    private Transform parentableObject;
    public int grab = 0;
    private float timeOut = 0;
    private float y_lock = -99;
    private Ray rayfromCursor;
    private RaycastHit furnitureHit, groundHit;
    private Vector3 furniturePoint;

    private void Awake() {
        // Create parentable object for furniture to parent to
        GameObject instParentable = new GameObject("Parentable");
        parentableObject = instParentable.transform;
    }


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
                cursor_default.GetChild(1).gameObject.SetActive(false);
                if(cursor_default.GetChild(0).localScale.x > 0.7f) {
                    cursor_default.GetChild(0).localScale -= new Vector3(2,2,2) * Time.deltaTime;                   
                }
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
                cursor_default.GetChild(1).gameObject.SetActive(true);
                if(cursor_default.GetChild(0).localScale.x < 0.97f) {
                    cursor_default.GetChild(0).localScale += new Vector3(2,2,2) * Time.deltaTime;
                }
                if(grabbedObject != null) {
                    if(y_lock == -99) {
                        y_lock = grabbedObject.transform.position.y;
                    }
                    parentableObject.position = new Vector3(furniturePoint.x,y_lock,furniturePoint.z);
                    parentableObject.rotation = transform.rotation;
                    grabbedObject.transform.parent = parentableObject;
                }
            }
            if(timeOut > 0) {
                timeOut -= Time.deltaTime;
            }

            // Shoots ray from camera screenpoint to furniture
            rayfromCursor = rayCamera.ScreenPointToRay(rayCamera.WorldToScreenPoint(transform.position));
            if(Physics.Raycast(transform.position,rayfromCursor.direction,out furnitureHit,100f,furnitureLayer)) {
                if(furnitureHit.transform.tag == "Grabbable" && grab == 1 && grabbedObject == null) {
                    print("grab");
                    grabbedObject = furnitureHit.transform;
                    grabbedParent = grabbedObject.parent;
                    Axis_Object a = grabbedObject.GetComponentInChildren<Axis_Object>();
                    if(a != null) {
                        a.GetComponent<MeshRenderer>().enabled = true;
                    }
                }  
            }
            if(Physics.Raycast(transform.position,rayfromCursor.direction,out groundHit,100f,floorLayer)) {
                furniturePoint = groundHit.point;
            }

        }
	}
}
