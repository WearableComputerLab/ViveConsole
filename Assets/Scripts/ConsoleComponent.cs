using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleComponent : MonoBehaviour {
    
    public float rayDist = 10;
    private RaycastHit hit;
    private Ray ray;
    private Ray downRay;
    private Vector3 forwardDirection;
    private Vector3 downDirection;
    private Transform rayTransform;

	// Use this for initialization
	void Start () {
		rayTransform = transform.GetChild(0);
	}
	
	// Update is called once per frame
	void Update () {
        FollowNormal();

	}

    // As you move object it follows normals of mesh
    private void FollowNormal() {

        forwardDirection = transform.TransformDirection(Vector3.forward);
        downDirection = -transform.forward;
        //Debug.DrawRay(transform.position,downDirection,Color.white,rayDist);
        ray = new Ray(rayTransform.position,transform.forward);
        downRay = new Ray(rayTransform.position,downDirection);
		if(Physics.Raycast(ray,out hit,rayDist)) {
            print(hit.transform);
            transform.position = hit.point;
            transform.rotation = Quaternion.FromToRotation(forwardDirection, -hit.normal) * transform.rotation;
        }
        if(Physics.Raycast(downRay,out hit,rayDist)) {
            print(hit.transform);
            transform.position = hit.point;
            transform.rotation = Quaternion.FromToRotation(forwardDirection, -hit.normal) * transform.rotation;
        }
    }
}
