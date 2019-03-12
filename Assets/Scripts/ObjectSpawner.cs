using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour {

    public GameObject[] spawnableObjects;
    private TabletBehaviour t;
    private bool add = false;
    private float addDelay = 0.3f;
    private RaycastHit hit;

	// Use this for initialization
	void Start () {
		t = FindObjectOfType<TabletBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {
        if(t == null) {
           t = FindObjectOfType<TabletBehaviour>();
        } else {
            ProcessObjects();
        }
        // get hit transform
        if (Physics.Raycast(transform.position, transform.forward, out hit)) {
        }
	}

    void ProcessObjects() {

        if(t.addObject != string.Empty && !add) {
           print("add");
           addObjectToRay();
           add = true;
           addDelay = 0.3f;
        }

        if(addDelay >= 0) {
           addDelay -= Time.deltaTime;
        }

        if(t.addObject == string.Empty && addDelay <= 0) {
           add = false;
        }

    }

    void addObjectToRay() {
        for(int i = 0; i < spawnableObjects.Length; ++i) {
            if (spawnableObjects[i].name.Equals(t.addObject)) {
                GameObject g = Instantiate(spawnableObjects[i],hit.point,spawnableObjects[i].transform.rotation);
                g.transform.localScale = g.transform.localScale * 0.35f; 
                g.transform.rotation = Quaternion.FromToRotation(g.transform.TransformDirection(Vector3.forward), hit.normal) * g.transform.rotation;
                g.transform.parent = GameObject.FindGameObjectWithTag("Console").transform;
            }
        }
    }

}
