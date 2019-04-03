using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObjectButton : MonoBehaviour {

    public string ObjectName;
    private Vector3 scale;
    private TabletBehaviour t;
	// Use this for initialization
	void Start () {
		scale = transform.localScale;
        t = transform.root.GetComponent<TabletBehaviour>();
	}

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            transform.localScale = scale * 1.2f;
            t.sendObject(ObjectName);
        }
        if (Input.GetMouseButtonUp(0)) {
            transform.localScale = scale;
            t.sendObject(string.Empty);
        }
    }

    private void OnMouseExit() {
        transform.localScale = scale;
    }
}
