using UnityEngine;
using UnityEngine.UI;

public class RadialMenuController : MonoBehaviour
{
    public Canvas radialMenu;

    Vector2? startTouchPos;
    ModuleSelectionManager selectionManager;
    ConsoleInteractionTest console;

    private void Start()
    {
        selectionManager = FindObjectOfType<ModuleSelectionManager>();
        console = FindObjectOfType<ConsoleInteractionTest>();
    }

    private void Update()
    {
        UpdateRadialMenu();
    }

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
            var arrow = radialMenu.GetComponentInChildren<Image>();
            var touchAngle = Vector2.SignedAngle(Vector2.up, touchPos - startTouchPos.Value) + 45;
            arrow.enabled = Vector2.SqrMagnitude(touchPos - startTouchPos.Value) >= 0.04f;
            arrow.transform.localRotation = Quaternion.AngleAxis(touchAngle, Vector3.forward);

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

    public void EditSelectedModule()
    {
        console.EditSelectedModule();
    }

    public void AnnotateSelectedModule()
    {
        console.AnnotateSelectedModule();
    }

    public void DeleteSelectedModule()
    {
        console.DeleteSelectedModule();
    }

    public void CloneSelectedModule()
    {
        console.CloneSelectedModule();
    }

}