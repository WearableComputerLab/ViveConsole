using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measuring_Pencil_Behaviour : MonoBehaviour {

     List<Vector3> linePoints = new List<Vector3>();
     LineRenderer lineRenderer;
     public float startWidth = 1.0f;
     public float endWidth = 1.0f;
     public float threshold = 0.001f;
     Camera thisCamera;
     int lineCount = 0;
     float lockY;
     Vector3 lastPos;
     public bool lockYMovement = true;
     public bool draw = true;

     
     void Awake() {
         lineRenderer = GetComponent<LineRenderer>();
         lockY = transform.position.y-0.2f;
         lastPos = new Vector3(transform.position.x,transform.position.y,transform.position.z);
     }
 
     void Update() {
         if(draw == true){
             Vector3 pos;
             if(lockYMovement){
                pos = new Vector3(transform.position.x,lockY,transform.position.z);
             } else {
                pos = new Vector3(transform.position.x,transform.position.y,transform.position.z);
             }
             transform.eulerAngles = new Vector3(-130,-40,0);
         
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
         }
     }
 
 
     void UpdateLine() {
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
