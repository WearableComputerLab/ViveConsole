using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitySARCommon.Tracking;

public class Ruler_Behaviour : MonoBehaviour {

    [Range(0.0f, 1)]
    public float scale;
    public Transform dest;
    public Tracker_Button_Server button;
    public Transform[] arrowHeads;
    public Vector3 rotationOffset;
    public Transform rulerTransform;
    public Text rulerText;
    public GameObject pencil;
    public float test;
    private LineRenderer linerenderer;
    private float distance = 0;
    private float preDist = -1;
    private int state = 0;
    private float timeOut = 0.2f;
    private FurnitureScaler scaler;

	// Use this for initialization
	private void Start () {
		linerenderer = GetComponent<LineRenderer>();
        scaler = FindObjectOfType<FurnitureScaler>();
	}
	
	// Update is called once per frame
	private void Update () {
        scale = scaler.scale;
        if (transform.hasChanged) {
            UpdatePosition();
        }
        if(state == 0) {
            linerenderer.enabled = true;
            pencil.SetActive(false);
        }
        if(state == 1) {
            linerenderer.enabled = false;
            pencil.SetActive(true);
        }
        if(state > 1) {
            state = 0;
        }
        if(timeOut <= 0 && button.buttonState == true) {
            state++;
            timeOut = 0.2f;
        }
        if(timeOut >= 0) {
            timeOut -= Time.deltaTime;
        }

	}
    private void OnValidate() {
        UpdatePosition();
    }

    private void UpdatePosition() {
        linerenderer = GetComponent<LineRenderer>();
		linerenderer.SetPosition(0,transform.position);
        linerenderer.SetPosition(1,dest.position);
        float x = transform.position.x + dest.position.x;
        float y = transform.position.y + dest.position.y + 0.2f;
        float z = transform.position.z + dest.position.z;
        Vector3 euler = Quaternion.LookRotation((transform.position - dest.position).normalized).eulerAngles;
        rulerTransform.eulerAngles = new Vector3(90,euler.y,0) + rotationOffset;
        if(state == 0){
            rulerTransform.position = new Vector3(x/2,y/2,z/2);
            distance = Vector3.Distance(transform.position,dest.position);
        } else {
            LineRenderer r = pencil.GetComponent<LineRenderer>();
            distance = ((r.startWidth + r.endWidth) * r.positionCount);
            rulerTransform.position = new Vector3(r.GetPosition(0).x,y/2,r.GetPosition(0).z);
        }
        if(Mathf.Abs(distance - preDist) > 0.001f){
            // multiply by 1000 to convert to mm
           rulerText.text = (int)(distance*(1/scale)*1000)+" mm";
        }
        arrowHeads[0].eulerAngles = new Vector3(90,euler.y,0) + new Vector3(0,-90,0);
        arrowHeads[1].eulerAngles = new Vector3(90,euler.y,0) + new Vector3(0,90,0);

        preDist = distance;
    }

 }
