using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Console_Pencil_Behaviour : MonoBehaviour
{

    public enum Mode { Draw, Move, Rotate, Scale, Delete, Clone, Vertical, Horizontal, Copy, Paste, Cut }
    public Mode mode;
    private int currentMode = 1;
    private float rayDist = 10;
    private float rotation;
    private bool drag = false;
    private Ray ray;
    private Ray downRay;
    private Vector3 forwardDirection;
    private Vector3 downDirection;
    private Vector3 hitPos;
    private Vector3 axisScale;
    private Transform rayTransform;
    private Transform deleteObject;
    private Transform cone;
    private Transform console;
    private Transform pivotPoint;
    private Transform cursor;
    private Transform moveObject;
    private MeshRenderer axis;
    private MeshRenderer cursorMesh;
    private MeshRenderer tabletButton;
    private MeshRenderer eraser;
    private MeshRenderer clone_tool;
    private Color coneColor;
    private Color[] cursorColors;
    public float dropTime = 1f;
    public float upTime = 1f;
    private Vector3 preVelocity;
    private Vector3 postVelocity;
    private Drawing_Pencil drawing_pencil;
    private ObjectScaler object_scaler;
    public TextMesh textMesh;
    private GameObject rotationOffset;


    // Use this for initialization
    private void Start()
    {
        cone = transform.GetChild(0);
        coneColor = cone.GetComponent<MeshRenderer>().material.color;
        cursorColors = new Color[2];
        drawing_pencil = GetComponentInChildren<Drawing_Pencil>();
        object_scaler = FindObjectOfType<ObjectScaler>();
        drawing_pencil.draw = false;
        pivotPoint = new GameObject("Pivot").GetComponent<Transform>();
        pivotPoint.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        rotationOffset = new GameObject("rotationOffset");
        cursor = transform.GetChild(2);
        cursorMesh = cursor.GetComponent<MeshRenderer>();
        axis = FindObjectOfType<Axis_Object>()?.GetComponent<MeshRenderer>();
        if (axis) axis.enabled = false;
        eraser = transform.GetChild(3).GetComponent<MeshRenderer>();
        clone_tool = transform.GetChild(4).GetComponent<MeshRenderer>();
        cursorColors[0] = cursorMesh.material.GetColor("_TintColor");
        cursorColors[1] = new Color32(208, 12, 12, 18);
    }

    private void Update()
    {


        //Cursor.visible = false;
        //pivotPoint.position = cursor.position;

        preVelocity = cursor.position;

        if (moveObject != null)
        {
            FollowNormal();
            if (axis)
            {
                axis.enabled = true;
                axis.transform.position = moveObject.position + (axis.transform.TransformDirection(Vector3.forward) * 0.1f);
                axis.transform.rotation = moveObject.rotation;

            }
            if (mode == Mode.Vertical)
            {
                pivotPoint.position = new Vector3(pivotPoint.position.x, cursor.position.y, cursor.position.z);
                print("Vertical");
            }
            if (mode == Mode.Horizontal)
            {
                pivotPoint.position = new Vector3(cursor.position.x, pivotPoint.position.y, pivotPoint.position.z);
            }
        }
        else
        {
            pivotPoint.transform.position = transform.position;
        }
        if (Input.GetMouseButtonDown(0) && moveObject != null && dropTime <= 0.1f)
        {
            moveObject.gameObject.GetComponent<BoxCollider>().enabled = true;
            moveObject.gameObject.GetComponentInChildren<Axis_Object>().gameObject.SetActive(true);
            moveObject = null;
            if (axis) axis.enabled = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            cone.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
        if (Input.GetMouseButton(0) && mode == Mode.Draw)
        {
            drawing_pencil.draw = true;
        }
        if (mode == Mode.Draw)
        {
            drawing_pencil.GetComponent<MeshRenderer>().enabled = true;
            cone.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            drawing_pencil.GetComponent<MeshRenderer>().enabled = false;
        }
        if (mode == Mode.Move)
        {
            drawing_pencil.GetComponent<MeshRenderer>().enabled = false;
        }
        if (mode == Mode.Clone)
        {
            clone_tool.enabled = true;
        }
        else
        {
            clone_tool.enabled = false;
        }
        if (mode == Mode.Delete)
        {
            eraser.enabled = true;
            if (deleteObject != null)
            {
                if (deleteObject.localScale.x > 0.01f)
                {
                    deleteObject.localScale -= new Vector3(0.3f, 0.3f, 0.3f) * Time.deltaTime;
                }
                else
                {
                    Destroy(deleteObject.gameObject);
                    deleteObject = null;
                }
            }
        }
        else
        {
            eraser.enabled = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            cone.GetComponent<MeshRenderer>().material.color = coneColor;
            drawing_pencil.draw = false;
            if (mode == Mode.Scale)
            {
                object_scaler.ScaleObject = null;
            }
        }
        if (dropTime > 0)
        {
            dropTime -= Time.deltaTime;
        }
        if (upTime > 0)
        {
            upTime -= Time.deltaTime;
        }
        SwitchMode();
        MoveCursor();

        Vector3 q = new Vector3(transform.eulerAngles.x + 180, transform.eulerAngles.y + 180, transform.eulerAngles.z);
        rotationOffset.transform.eulerAngles = q;
        rotation = rotationOffset.transform.eulerAngles.y + 180;
        postVelocity = transform.position - preVelocity;
    }

    // As you move object it follows normals of mesh
    private void FollowNormal()
    {
        forwardDirection = cone.TransformDirection(Vector3.forward);
        Debug.DrawRay(cone.position, forwardDirection, Color.yellow, 0, true);
        ray = new Ray(cone.position, forwardDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDist, LayerMask.GetMask("Default")) && drag == false && mode != Mode.Scale)
        {
            moveObject.localPosition = new Vector3(0, 0.05f, 0.05f);
            pivotPoint.rotation = Quaternion.FromToRotation(pivotPoint.TransformDirection(Vector3.forward), hit.normal) * pivotPoint.rotation;
            if (mode == Mode.Rotate)
            {
                moveObject.localRotation = Quaternion.identity * Quaternion.Euler(0, 0, rotation);
            }
        }
    }

    private void SwitchMode()
    {
        switch (currentMode)
        {
            case 0:
                mode = Mode.Draw;
                break;
            case 1:
                mode = Mode.Move;
                cursorMesh.material.SetColor("_TintColor", cursorColors[0]);
                break;
            case 2:
                mode = Mode.Rotate; // rotate
                break;
            case 3:
                mode = Mode.Scale; // scale
                break;
            case 4:
                mode = Mode.Delete;
                cursorMesh.material.SetColor("_TintColor", cursorColors[1]);
                break;
            case 5:
                mode = Mode.Clone;
                break;
            case 6:
                mode = Mode.Vertical;
                break;
            case 7:
                mode = Mode.Horizontal;
                break;

        }
    }

    private void MoveCursor()
    {
        Vector3 f = cone.TransformDirection(Vector3.forward);
        Debug.DrawRay(cone.position, f, Color.yellow, 0, true);
        Ray r = new Ray(cone.position, f);
        if (Physics.Raycast(r, out RaycastHit hit, rayDist, LayerMask.GetMask("Furniture")))
        {
            if (Input.GetMouseButton(1) || Valve.VR.SteamVR_Actions.default_InteractUI.state)
            {
                var annotationLayer = hit.transform.GetComponentInParent<AnnotationLayer>();
                var annotateTex = annotationLayer.GetComponentInChildren<AnnotateTexture>();
                // update ray with annotateTexture
                if (Input.GetMouseButtonDown(1) || Valve.VR.SteamVR_Actions.default_InteractUI.stateDown)
                {
                    // start a ray with the annotateTexture
                    annotateTex.OnBeginRay(r);
                }
                else
                    annotateTex.OnUpdateRay(r);
            }

            cursor.position = hit.point + hit.normal * 0.01f; ;
            clone_tool.transform.position = hit.point;
            drawing_pencil.transform.position = hit.point + hit.normal * 0.01f;
            cursor.rotation = Quaternion.FromToRotation(cursor.TransformDirection(Vector3.forward), hit.normal) * cursor.rotation;
            if (hit.transform.tag == "Grabbable")
            {
                if (axis)
                {
                    axis.transform.gameObject.SetActive(true);
                    axis.transform.position = hit.transform.position + (axis.transform.TransformDirection(Vector3.forward) * 0.1f);
                    axis.transform.rotation = hit.transform.rotation;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    if (moveObject == null && mode != Mode.Delete && mode != Mode.Clone && mode != Mode.Scale)
                    {
                        moveObject = hit.transform;
                        moveObject.transform.parent = pivotPoint;
                        hit.transform.gameObject.GetComponent<BoxCollider>().enabled = false;
                        pivotPoint.position = cursor.position;
                        upTime = 1f;
                    }
                    if (mode == Mode.Scale)
                    {
                        object_scaler.ScaleObject = hit.transform;
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (moveObject == null && mode == Mode.Clone && upTime < 0.1f)
                    {
                        Transform t = hit.transform;
                        Vector3 point = hit.point;
                        GameObject g = hit.transform.gameObject;
                        GameObject inst = Instantiate(g, g.transform.position, g.transform.rotation);
                        moveObject = inst.transform;
                        moveObject.transform.SetParent(pivotPoint);
                        moveObject.transform.localScale = t.localScale;
                        moveObject.position = point;
                        upTime = 1f;
                    }
                    if (mode == Mode.Scale)
                    {
                        object_scaler.ScaleObject = null;
                    }
                }
                if (deleteObject == null && mode == Mode.Delete)
                {
                    eraser.transform.position = hit.point;
                    if (Input.GetMouseButtonDown(0))
                    {
                        deleteObject = hit.transform;
                    }
                }

            }
            if (hit.transform.tag == "Console")
            {
                drag = false;
                console = hit.transform;
                cone.GetComponent<MeshRenderer>().material.color = coneColor;
                print("console was found");
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (pivotPoint.transform.childCount > 0)
                {
                    foreach (Transform t in pivotPoint)
                    {
                        t.parent = console;
                    }
                }
                if (moveObject != null && Input.GetMouseButtonUp(0))
                {
                    print("drop");
                    moveObject.gameObject.GetComponent<BoxCollider>().enabled = true;
                    moveObject.transform.parent = console;
                    moveObject = null;
                    dropTime = 1f;
                }

            }
        }
    }

    public void setMode(int mode)
    {
        currentMode = mode;
    }

}
