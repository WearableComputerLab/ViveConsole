using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Button : MonoBehaviour {

    public int Scene;

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();    
        }
    }

    private void OnMouseOver() {
        transform.localScale = new Vector3(0.11f,0.11f,0.11f);
        if (Input.GetMouseButtonDown(0)) {
            SceneManager.LoadScene(Scene);
        }        
    }

    private void OnMouseExit() {
        transform.localScale = new Vector3(0.1f,0.1f,0.1f);
    }

}
