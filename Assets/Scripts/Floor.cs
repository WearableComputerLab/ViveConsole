using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

    private Vector3 velocity;
    private Vector3 mousePrev = Vector3.zero;
    private MonitorViewBehaviour monitor;
    private Transform monitorTransform;
    private Transform duplicateTransform;
    private float magnitudeLimit = 30000f;

    private void Awake() {
        monitor = FindObjectOfType<MonitorViewBehaviour>();    
        monitorTransform = monitor.transform.GetChild(0);
        GameObject g = new GameObject();
        g.name = "orientPan";
        duplicateTransform = g.transform;
    }

    // Update is called once per frame
    private void Update () {
        // limit forward vector to x and z, and avoid adding to y
        duplicateTransform.position = monitorTransform.position;
        duplicateTransform.eulerAngles = new Vector3(0,monitorTransform.eulerAngles.y,0);
		if(Input.GetMouseButton(1)){
		    velocity = Input.mousePosition - mousePrev;
            if(velocity.sqrMagnitude < magnitudeLimit){
                transform.position += (duplicateTransform.forward * velocity.y) * Time.deltaTime;
                transform.position += (duplicateTransform.right * velocity.x) * Time.deltaTime;
            }
            mousePrev = Input.mousePosition;
        } else {
            mousePrev = Input.mousePosition;
        }
	}

}
