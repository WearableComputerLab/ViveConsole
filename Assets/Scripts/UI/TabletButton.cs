using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TabletButton : MonoBehaviour {
    public string ButtonID;
    private TabletBehaviour t;
    private bool clicked;
    public Color defaultColor;

    private void Start() {
        t = transform.root.GetComponent<TabletBehaviour>();
    }
    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            t.send(ButtonID);
        } 
        GetComponent<MeshRenderer>().material.color = Color.cyan;
    }
    private void OnMouseExit() {
        GetComponent<MeshRenderer>().material.color = defaultColor;
    }

    private void OnDisable() {
        GetComponent<MeshRenderer>().material.color = defaultColor;
    }

 }
