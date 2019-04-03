using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TabletBehaviour : NetworkBehaviour {

    public string currentMode;
    public string addObject;
    public bool isOnTablet = false;
    private Console_Pencil_Behaviour pencil;
    private Transform page1, page2, page3, page4;

    private void Start() {
        pencil = FindObjectOfType<Console_Pencil_Behaviour>();
        if (isOnTablet) {
            page1 = transform.GetChild(0).transform.GetChild(0);
            page2 = transform.GetChild(0).transform.GetChild(1);
            page3 = transform.GetChild(0).transform.GetChild(2);
            page4 = transform.GetChild(0).transform.GetChild(3);
        }
    }

    public void send(string s) {
        this.currentMode = s;
    }

    public void sendObject(string s) {
        this.addObject = s;
    }

    public void Update() {
        if (isServer) {
            print("not server");
            CmdSendMessage(currentMode);
            CmdAddObject(addObject);
        }
        if(pencil != null) {
            SwitchModes(); 
        }
        if (isOnTablet) {
            if (currentMode.Equals("Add") ||
                currentMode.Equals("Move") ||
                currentMode.Equals("Vertical") ||
                currentMode.Equals("Horizontal") ||
                currentMode.Equals("Rotate") ||
                currentMode.Equals("Clone") ||
                currentMode.Equals("Copy") ||
                currentMode.Equals("Paste") ||
                currentMode.Equals("Cut"))
            {
                page1.transform.localPosition = Vector3.Lerp(page1.transform.localPosition,new Vector3(0,-0.5f,-0.05f),3 * Time.deltaTime);
                if(currentMode.Equals("Add")){
                    page2.gameObject.SetActive(true);
                }
                if(currentMode.Equals("Move")){
                    page3.gameObject.SetActive(true);
                }
                if(currentMode.Equals("Clone")){
                    page4.gameObject.SetActive(true);
                }
            } else {
                page1.transform.localPosition = Vector3.Lerp(page1.transform.localPosition,new Vector3(0,0,0),3 * Time.deltaTime);
                page2.gameObject.SetActive(false);
                page3.gameObject.SetActive(false);
                page4.gameObject.SetActive(false);
            }
        }
    }

    private void SwitchModes() {
        if(currentMode != string.Empty) {
            switch (currentMode) {
                case "Move":
                    pencil.setMode(1);
                    break;
                case "Pencil":
                    pencil.setMode(0);
                    break;
                case "Scale":
                    pencil.setMode(3);
                    break;
                case "Delete":
                    pencil.setMode(4);
                    break;
                case "Clone":
                    pencil.setMode(5);
                    break;
                case "Vertical":
                    pencil.setMode(6);
                    break;
                case "Horizontal":
                    pencil.setMode(7);
                    break;
                case "Rotate":
                    pencil.setMode(2);
                    break;
            }
        }
    }

    [Command]
    private void CmdSendMessage(string s) {
        RpcUpdate(s);
    }

    [ClientRpc]
    private void RpcUpdate(string s) {
        currentMode = s;
    }

    [Command]
    private void CmdAddObject(string s) {
        RpcAddObject(s);
    }

    [ClientRpc]
    private void RpcAddObject(string s) {
        addObject = s;
    }

 }
