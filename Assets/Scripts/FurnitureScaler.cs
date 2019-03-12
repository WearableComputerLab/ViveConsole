using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FurnitureScaler : MonoBehaviour {

    [Range(0.0f, 1)]
    public float scale;
    public TextMesh scaleText;
    public Transform plus, minus, exit, help;
    public GameObject helpScreen;
    private Vector3 mousePosition;
    private Floor[] scaledObjects;
    private int changeMode = 0;
    public Camera cam;

    private void Start() {
        scaledObjects = FindObjectsOfType<Floor>();    
    }

    // Update is called once per frame
    private void Update () {

        transform.localScale = new Vector3(scale,transform.localScale.y,transform.localScale.z);

        if (scaleText != null) {
            float scaleFactor = 1/scale;
            scaleText.text = "1:"+(int)scaleFactor;

        }
        if(scale <= 0.99f){
            if(Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus)) {
                scale += 0.5f * Time.deltaTime;
                AlterScale();
            }
        }
        if(scale > 0.1f) {
            if(Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) {
                scale -= 0.5f * Time.deltaTime;
                AlterScale();
            }
        }

        float plusDist = (Input.mousePosition - cam.WorldToScreenPoint(plus.position)).magnitude;
        float minusDist = (Input.mousePosition - cam.WorldToScreenPoint(minus.position)).magnitude;
        float exitDist = (Input.mousePosition - cam.WorldToScreenPoint(exit.position)).magnitude;
        float helpDist = (Input.mousePosition - cam.WorldToScreenPoint(help.position)).magnitude;

        if(plusDist < 15) {
            plus.transform.localScale = new Vector3(0.015f,0.015f,0.015f);
            if(Input.GetMouseButton(0) && scale <= 0.99f) {
                scale += 0.5f * Time.deltaTime;
                AlterScale();
            }
        } else {
            plus.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        }
        if(minusDist < 15) {
            minus.transform.localScale = new Vector3(0.015f,0.015f,0.015f);
            if(Input.GetMouseButton(0) && scale > 0.1f) {
                scale -= 0.5f * Time.deltaTime;
                AlterScale();
            }
        } else {
            minus.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
        }
        if(exitDist < 15) {
            exit.transform.localScale = new Vector3(0.06f,0.06f,0.06f);
            if(Input.GetMouseButtonDown(0) && changeMode != 1) {
                GameObject g = FindObjectOfType<TrackingController>().gameObject;
                // Destroy the tracking controller to make sure not more than one exists in app 
                Destroy(g);
                SceneManager.LoadScene(0);
            }
            if(Input.GetMouseButtonDown(0) && changeMode == 1) {
                helpScreen.SetActive(false);
                changeMode = 0;
            }
        } else {
            exit.transform.localScale = new Vector3(0.05f,0.05f,0.05f);
        }
        if(helpDist < 15) {
            help.transform.localScale = new Vector3(-0.06f,0.06f,-0.06f);
            if(Input.GetMouseButtonUp(0) && changeMode == 0) {
                helpScreen.SetActive(true);
                changeMode = 1;
            }

        } else {
            help.transform.localScale = new Vector3(-0.05f,0.05f,-0.05f);
        }


        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
	}

    private void AlterScale() {
        foreach(Floor s in scaledObjects){
            Transform t = s.transform;
            t.localScale = new Vector3(scale,scale,scale);
        }
    }
}
