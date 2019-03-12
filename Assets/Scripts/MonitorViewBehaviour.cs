using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorViewBehaviour : MonoBehaviour {

    private Vector3 velocity;
    private Vector3 mousePrev = Vector3.zero;
	
	// Update is called once per frame
	private void Update () {
        if(Input.GetMouseButton(0)){
		    velocity = Input.mousePosition - mousePrev;
            transform.eulerAngles += new Vector3(velocity.y * (2 * Time.deltaTime),velocity.x,0);
            mousePrev = Input.mousePosition;
        } else {
            if(Input.GetMouseButton(2)==false)
            mousePrev = Input.mousePosition; 
        }
        if(Input.GetMouseButton(2)){
		    velocity = Input.mousePosition - mousePrev;
            transform.localPosition += new Vector3(0,velocity.y * Time.deltaTime,0);
            mousePrev = Input.mousePosition;
        }
        transform.GetChild(0).localPosition += new Vector3(0,0,Input.GetAxis("Mouse ScrollWheel"));
	}

}
