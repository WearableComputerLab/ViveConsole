using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drawing_Pencil : MonoBehaviour {

     // must specify material for drawing
     public Material pencilMaterial;
     private List<Vector3> linePoints = new List<Vector3>();
     private List<GameObject> lineObjects = new List<GameObject>();
     private LineRenderer lineRenderer;
     public float startWidth = 1.0f;
     public float endWidth = 1.0f;
     public float threshold = 0.001f;
     private Camera thisCamera;
     private int lineCount = 0;
     private float lockY;
     private Vector3 lastPos;
     public bool lockYMovement = true;
     public bool draw = true;
     private bool createLine = false;

     
     private void Awake() {
         lockY = transform.position.y-0.2f;
         lastPos = new Vector3(transform.position.x,lockY,transform.position.z);
         draw = false;
     }
 
     private void Update() {
         if(draw == true){
             if(createLine == false) {
                GameObject g = new GameObject();
                lineObjects.Add(g);
                g.name = "Drawing";
                lineRenderer = g.AddComponent<LineRenderer>();
                lineRenderer.material = pencilMaterial;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer.material.color = Color.blue;
                createLine = true;
             }
             Vector3 pos;
             if(lockYMovement){
                pos = new Vector3(transform.position.x,lockY,transform.position.z);
             } else {
                pos = new Vector3(transform.position.x,transform.position.y,transform.position.z);
             }
             //transform.eulerAngles = new Vector3(-130,-40,0);
         
             float dist = Vector3.Distance(lastPos, pos);
             if(dist <= threshold)
                 return;
         
             lastPos = pos;
             if(linePoints == null)
                 linePoints = new List<Vector3>();
             if(pos != Vector3.zero){
                linePoints.Add(pos);        
                UpdateLine();
             }
         } else {
            lineCount = 0;
            linePoints = new List<Vector3>();
            createLine = false;
         }
        if(Input.GetMouseButtonUp(1)) {
           print("MOUSSSSEEEEE!!!!");
           if(lineObjects.Count > 0){
               foreach(GameObject g in lineObjects) {
                    Destroy(g);
               }
               lineObjects = new List<GameObject>();
           }
        }
     }
 
 
     private void UpdateLine() {
         lineRenderer.startWidth = startWidth;
         lineRenderer.endWidth = endWidth;
         lineRenderer.positionCount = linePoints.Count;
 
         for(int i = lineCount; i < linePoints.Count; i++)
         {
             lineRenderer.SetPosition(i, linePoints[i]);
            if (linePoints[i] == Vector3.zero) {
                linePoints.RemoveAt(i);
            }
         }
         lineCount = linePoints.Count;
     }
     private void OnEnable() {
         lineCount = 0;
         linePoints = new List<Vector3>();
     }   
}
