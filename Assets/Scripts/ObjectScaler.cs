using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScaler : MonoBehaviour {

    public Transform ScaleObject;
    private Transform centrePivot, x_axis, y_axis;
    private Console_Pencil_Behaviour console_pencil;
    private Transform pencil_transform;
    private Vector3 prevPos = Vector3.zero;
    private Vector3 currentPos;
    private Vector3 velocity;
    private float time;
    private float delay = 0.1f;
    public float axisDist = 0.1f;

	// Use this for initialization
	void Start () {
		centrePivot = transform.GetChild(0);
        y_axis = transform.GetChild(1);
        x_axis = transform.GetChild(2);
        console_pencil = FindObjectOfType<Console_Pencil_Behaviour>();
        pencil_transform = console_pencil.transform;
        time = delay;
	}
	
	// Update is called once per frame
	void Update () {
        delayTime();
        if(velocity.x > 8f || velocity.x < -8f) {
            x_axis.transform.localScale = new Vector3(1,1.3f,1);
        } else {
            x_axis.transform.localScale = new Vector3(1,1,1);
        }

        if(velocity.y > 8f || velocity.y < -8f) {
            y_axis.transform.localScale = new Vector3(1,1.3f,1);
        } else {
            y_axis.transform.localScale = new Vector3(1,1,1);
        }

        if(ScaleObject != null) {
            centrePivot.gameObject.SetActive(true);
            x_axis.gameObject.SetActive(true);
            y_axis.gameObject.SetActive(true);

            ScaleObject.localScale += (velocity * (0.05f * Time.deltaTime)); 
            ScaleObject.localScale += (new Vector3(velocity.z,velocity.z,velocity.z) * (0.05f * Time.deltaTime));
            transform.position = ScaleObject.position +  (ScaleObject.TransformDirection(Vector3.forward) * axisDist);

        } else {
            centrePivot.gameObject.SetActive(false);
            x_axis.gameObject.SetActive(false);
            y_axis.gameObject.SetActive(false);
        }
	}

    void CalculateVelocity() {
        currentPos = pencil_transform.position;
        velocity = (currentPos - prevPos) / Time.deltaTime;
        //console_pencil.textMesh.text = velocity+"";
        prevPos = pencil_transform.position;
    }

    void delayTime() {
        if(time > 0) {
            time -= Time.deltaTime;
        } else {
            time = delay;
            CalculateVelocity();
        }
    }
}
