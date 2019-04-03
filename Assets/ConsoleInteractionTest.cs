using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleInteractionTest : MonoBehaviour
{
    public Camera attachedCamera;

    private ConsoleModule _selectedModule;

    void Start()
    {
        attachedCamera = GetComponentInParent<Camera>();
    }

    void Update()
    {
        if (_selectedModule == null)
            UpdateSelection();
        else
            UpdateSelectedModule();
    }

    void UpdateSelection()
    {
        var screenRay = attachedCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(screenRay, maxDistance: Mathf.Infinity, hitInfo: out raycastHit, layerMask: LayerMask.GetMask("Interactable")))
        {
            // Highlight the object
            // Start check for interactions
            Debug.DrawRay(screenRay.origin, screenRay.direction, Color.blue, 0, true);
            var module = raycastHit.collider.GetComponentInParent<ConsoleModule>();

            if (Input.GetMouseButtonDown(0) && module != null)
            {
                _selectedModule = module;
            }
        }

    }

    void UpdateSelectedModule()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // drop the selected module
            _selectedModule.transform.Translate(Vector3.back * 0.05f);
            _selectedModule = null;
        }
        else
        {
            // drag the selected module
            var screenRay = attachedCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            if (Physics.Raycast(screenRay, maxDistance: Mathf.Infinity, hitInfo: out raycastHit, layerMask: LayerMask.GetMask("Furniture")))
            {
                var panel = raycastHit.collider.GetComponentInParent<ModulePanel>();
                var currentRotation = _selectedModule.transform.localRotation;
                _selectedModule.SetPanel(panel);
                _selectedModule.transform.position = raycastHit.point;
                _selectedModule.transform.localRotation = currentRotation;
                _selectedModule.transform.Translate(Vector3.forward * 0.05f);
            }
        }
    }
}