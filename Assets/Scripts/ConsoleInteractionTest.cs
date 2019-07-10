using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleInteractionTest : MonoBehaviour
{
    public Camera attachedCamera;

    private ConsoleModule _selectedModule;
    private Console_Pencil_Behaviour pencil;
    private ModuleSelectionManager selectionManager;
    private Vector3 _selectionOffset;
    private Vector3 _localSelectionOffset;
    private ConsoleBase consoleBase;

    public Canvas radialMenu;

    void Start()
    {
        attachedCamera = GetComponentInParent<Camera>();
        pencil = FindObjectOfType<Console_Pencil_Behaviour>();
        selectionManager = FindObjectOfType<ModuleSelectionManager>();
        consoleBase = FindObjectOfType<ConsoleBase>();
    }

    void Update()
    {
        //Unity.Labs.SuperScience.GizmoModule.instance.DrawCube(Vector3.right * .5f, Quaternion.identity, Vector3.one, new Color(.5f, 1.0f, 1.0f, 0.2f));
        //Unity.Labs.SuperScience.GizmoModule.instance.DrawWedge(Vector3.right * .5f, Quaternion.identity, 10, 100, new Color32(100, 255, 255, 250));
        if (_selectedModule == null)
            UpdateSelection();
        else
            UpdateSelectedModule();

        UpdateRadialMenu();
    }

    public void EditSelectedModule()
    {
        if (_selectedModule)
            DropSelectedModule(DRAG_HEIGHT);
        else
            _selectedModule = selectionManager.SelectedModule;
    }

    public void AnnotateSelectedModule()
    {

    }

    public void DeleteSelectedModule()
    {
        var deleteTarget = selectionManager.SelectedModule;
        selectionManager.SelectedModule = null;
        consoleBase.RemoveModule(deleteTarget);
    }

    public void CloneSelectedModule()
    {
        var cloneSource = selectionManager.SelectedModule;
        _selectedModule = consoleBase.AddModule(cloneSource._id, cloneSource._panel?._id ?? 0);
        selectionManager.SelectedModule = _selectedModule;
    }

    void UpdateSelection()
    {
        var screenRay = attachedCamera.ScreenPointToRay(Input.mousePosition);
        var controllerRay = new Ray(pencil.transform.position, -pencil.transform.up);

        if (Physics.Raycast(screenRay, maxDistance: Mathf.Infinity, hitInfo: out RaycastHit raycastHit, layerMask: LayerMask.GetMask("Interactable")) ||
            Physics.Raycast(controllerRay, maxDistance: Mathf.Infinity, hitInfo: out raycastHit, layerMask: LayerMask.GetMask("Interactable")))
        {
            // Start check for interactions
            Debug.DrawRay(screenRay.origin, screenRay.direction * 10, Color.blue, 0, true);
            var module = raycastHit.collider.GetComponentInParent<ConsoleModule>();

            if ((Input.GetMouseButtonDown(0) || Valve.VR.SteamVR_Actions.default_GrabGrip.stateDown) &&
                module != null)
            {
                _selectedModule = module;
                if (Physics.Raycast(GetScreenRay(), maxDistance: Mathf.Infinity, hitInfo: out RaycastHit modulePanelHit, layerMask: LayerMask.GetMask("Furniture")))
                {
                    var colliderPlane = new Plane(modulePanelHit.normal, modulePanelHit.point);
                    _selectionOffset = colliderPlane.ClosestPointOnPlane(module.transform.position) - modulePanelHit.point;
                    _localSelectionOffset = modulePanelHit.collider.transform.InverseTransformVector(_selectionOffset);
                    //_selectionOffset.z = 0;
                }
            }

            // Highlight the object
            selectionManager.SelectedModule = module;
            //var gizmos = Unity.Labs.SuperScience.GizmoModule.instance;
            //gizmos.DrawCube(module.transform.position, module.transform.rotation, Vector3.one, new Color(.5f, 1.0f, 1.0f, 0.2f));

        }
        else
            selectionManager.SelectedModule = null;

    }

    const float DRAG_HEIGHT = 0.01f;
    void UpdateSelectedModule()
    {
        if (Input.GetMouseButtonUp(0) ||
            Valve.VR.SteamVR_Actions.default_GrabGrip.stateUp ||
            Valve.VR.SteamVR_Actions.default_InteractUI.stateDown)
        {
            DropSelectedModule(DRAG_HEIGHT);
        }
        else //if (Input.GetMouseButton(0) ||
             //    Valve.VR.SteamVR_Actions.default_GrabGrip.state)
        {
            // drag the selected module
            var screenRay = GetScreenRay();
            if (Physics.Raycast(screenRay, maxDistance: Mathf.Infinity, hitInfo: out RaycastHit raycastHit, layerMask: LayerMask.GetMask("Furniture")))
            {
                var panel = raycastHit.collider.GetComponentInParent<ModulePanel>();
                var currentRotation = _selectedModule.transform.localRotation;
                _selectedModule.SetPanel(panel);
                _selectedModule.transform.position = raycastHit.point + _selectionOffset;
                var fromLocalSelectionOffset = raycastHit.point + raycastHit.collider.transform.TransformVector(_localSelectionOffset);
                _selectedModule.transform.position = fromLocalSelectionOffset;
                _selectedModule.transform.localRotation = currentRotation;
                _selectedModule.transform.Translate(panel.transform.forward * -DRAG_HEIGHT, Space.World);
            }
        }
    }

    private void DropSelectedModule(float DRAG_HEIGHT)
    {
        // drop the selected module
        _selectedModule.transform.Translate(_selectedModule.transform.parent.forward * DRAG_HEIGHT, Space.World);
        _selectedModule = null;
    }

    Ray GetScreenRay()
    {
        if (Input.GetMouseButton(0))
            return attachedCamera.ScreenPointToRay(Input.mousePosition);

        return new Ray(pencil.transform.position, -pencil.transform.up);

    }

            Vector2? startTouchPos;
    void UpdateRadialMenu()
    {
        var showRadialMenu = Valve.VR.SteamVR_Actions.default_ShowRadialMenu.state && selectionManager.SelectedModule != null;
        radialMenu.gameObject.SetActive(showRadialMenu);
        if (showRadialMenu)
        {
            radialMenu.transform.parent.position = selectionManager.SelectedModule.transform.position;
            radialMenu.transform.parent.LookAt(selectionManager.SelectedModule.transform.TransformPoint(Vector3.back));
            //radialMenu.transform.
            var touchPos = Valve.VR.SteamVR_Actions.default_RadialThumb.axis;
            startTouchPos = Vector2.zero; // startTouchPos ?? touchPos;
            var buttonList = radialMenu.GetComponentsInChildren<Button>();
            const float deadzone = 0.2f;
            switch (touchPos - startTouchPos.Value)
            {
                case var t when t.y > deadzone && Mathf.Abs(t.x) < t.y:
                    buttonList[0].Select(); break;
                case var t when t.x > deadzone && Mathf.Abs(t.y) < t.x:
                    buttonList[1].Select(); break;
                case var t when t.x < -deadzone && Mathf.Abs(t.y) < -t.x:
                    buttonList[2].Select(); break;
                case var t when t.y < -deadzone && Mathf.Abs(t.x) < -t.y:
                    buttonList[3].Select(); break;
                default:
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                    break;
            }

            if (Valve.VR.SteamVR_Actions.default_ConfirmRadialMenu.stateDown)
            {
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.GetComponent<Button>().onClick.Invoke();
            }
        }
        else
        {
            startTouchPos = null;
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }
}