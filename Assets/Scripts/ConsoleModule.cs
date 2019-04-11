using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ConsoleModule : MonoBehaviour
{
    public int _id;
    public ModulePanel _panel;
    public Rect _extents;
    public string _description;
    public string _productCode;
    public string _designNotes;

    // Start is called before the first frame update
    void Start()
    {
        //_id = Mathf.FloorToInt(transform.position.x * 100);
        //var panels = Physics.OverlapSphere(transform.position, 0.05f, 1 << LayerMask.NameToLayer("Furniture"));
        //if (panels.Length > 0)
        //    _panel = panels[0].GetComponentInParent<ModulePanel>();
    }

    public void SetPanel(ModulePanel panel)
    {
        if (_panel == panel)
            return;

        _panel?._modules.Remove(this);
        _panel = panel;
        _panel._modules.Add(this);
        transform.SetParent(_panel.transform, false);
    }
}
