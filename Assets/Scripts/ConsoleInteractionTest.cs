using System;
using UnityEngine;

public class ConsoleInteractionTest : MonoBehaviour
{
    public Camera attachedCamera;

    private ConsoleModule _selectedModule;
    private Console_Pencil_Behaviour pencil;
    private ModuleSelectionManager selectionManager;
    private Vector3 _selectionOffset;
    private Vector3 _localSelectionOffset;
    private ConsoleBase consoleBase;

    private Vector3 previousModulePosition;
    private ModulePanel previousModulePanel;

    void Start()
    {
        attachedCamera = GetComponentInParent<Camera>();
        pencil = FindObjectOfType<Console_Pencil_Behaviour>();
        selectionManager = FindObjectOfType<ModuleSelectionManager>();
        consoleBase = FindObjectOfType<ConsoleBase>();

        KeywordListener.keywordHeard.AddListener(HandleKeywordActions);
    }

    private void HandleKeywordActions(string keyword)
    {
        switch (keyword)
        {
            case "copy":
                CloneSelectedModule();
                break;
            case "move":
                EditSelectedModule();
                break;
            case "annotate":
                AnnotateSelectedModule();
                break;
            case "delete":
                DeleteSelectedModule();
                break;

            case "rotate":
                RotateSelectedModule(); break;
            case "scale":
                ScaleSelectedModule(); break;
            case "done":
                FinishCurrentAction(); break;
            case "cancel":
                CancelCurrentAction(); break;
            case "undo":
                UndoLastAction(); break;
            case "save":
                SaveConfiguration(); break;
            case "load":
                LoadConfiguration(); break;

            case "Test":
                Debug.Log("Keyword detected - Test");
                break;
            case "Check":
                Debug.Log("Keyword detected - Check");
                break;
            default:
                Debug.Log("Unknown keyword detected");
                break;
        }
    }

    private void LoadConfiguration()
    {
        consoleBase.LoadLayoutFrom();
    }

    private void SaveConfiguration()
    {
        consoleBase.SaveCurrentLayout();
    }

    private void UndoLastAction()
    {
        // TODO: Implement an actual undo system for modifications
        throw new NotImplementedException();
    }

    private void CancelCurrentAction()
    {
        if (_selectedModule)
        {
            var mod = _selectedModule;
            DropSelectedModule(DRAG_HEIGHT);
            mod.SetPanel(previousModulePanel);
            mod.transform.localPosition = previousModulePosition;
        }
    }

    private void FinishCurrentAction()
    {
        if (_selectedModule)
            DropSelectedModule(DRAG_HEIGHT);
    }

    private void ScaleSelectedModule()
    {
        throw new NotImplementedException();
    }

    private void RotateSelectedModule()
    {
        var moduleToRotate = _selectedModule ?? selectionManager.SelectedModule;
        moduleToRotate.transform.Rotate(Vector3.forward, 15, Space.Self);
    }

    void Update()
    {
        //Unity.Labs.SuperScience.GizmoModule.instance.DrawCube(Vector3.right * .5f, Quaternion.identity, Vector3.one, new Color(.5f, 1.0f, 1.0f, 0.2f));
        //Unity.Labs.SuperScience.GizmoModule.instance.DrawWedge(Vector3.right * .5f, Quaternion.identity, 10, 100, new Color32(100, 255, 255, 250));
        if (_selectedModule == null)
            UpdateSelection();
        else
            UpdateSelectedModule();
    }

    public void EditSelectedModule()
    {
        if (_selectedModule)
            DropSelectedModule(DRAG_HEIGHT);
        else
        {
            _selectedModule = selectionManager.SelectedModule;
            previousModulePosition = _selectedModule.transform.localPosition;
            previousModulePanel = _selectedModule._panel;
            if (Physics.Raycast(GetScreenRay(), maxDistance: Mathf.Infinity, hitInfo: out RaycastHit modulePanelHit, layerMask: LayerMask.GetMask("Furniture")))
            {
                var colliderPlane = new Plane(modulePanelHit.normal, modulePanelHit.point);
                _selectionOffset = colliderPlane.ClosestPointOnPlane(_selectedModule.transform.position) - modulePanelHit.point;
                _localSelectionOffset = modulePanelHit.collider.transform.InverseTransformVector(_selectionOffset);
                //_selectionOffset.z = 0;
            }
        }
    }

    public void AnnotateSelectedModule()
    {

    }

    public void DeleteSelectedModule()
    {
        if (_selectedModule)
            DropSelectedModule(DRAG_HEIGHT);

        var deleteTarget = selectionManager.SelectedModule;
        selectionManager.SelectedModule = null;
        consoleBase.RemoveModule(deleteTarget);
    }

    public void CloneSelectedModule()
    {
        if (_selectedModule)
            DropSelectedModule(DRAG_HEIGHT);

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

}
