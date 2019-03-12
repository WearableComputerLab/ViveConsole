using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AutoJoinNetwork : NetworkBehaviour {
	
    private bool connect = false;

	// Update is called once per frame
	private void Update () {
        if(Network.isClient == false && connect == false){
            NetworkManager.singleton.networkAddress = "10.160.37.202";
            NetworkManager.singleton.networkPort = 1616;
		    NetworkManager.singleton.StartClient();
            connect = true;
        }
	}
    private void OnDisconnectedFromServer(NetworkDisconnection info) {
            connect = false;
    }
}
