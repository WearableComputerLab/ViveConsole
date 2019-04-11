using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using File = System.IO.File;
using Path = System.IO.Path;
using Encoding = System.Text.Encoding;

public partial class ConsoleBase : MonoBehaviour
{
    public int _id;

    public List<ConsoleModule> _currentModules;
    public List<ModulePanel> _modulePanels;

    public ModulePool _modulePool;

    public ModuleLayout _savedLayout;

    public ConsoleModule AddModule(int moduleId, int panelId)
    {
        var newModule = _modulePool?.RequestModule(moduleId);
        var panel = GetPanelWithId(panelId) ?? 
            _modulePanels[Random.Range(0, _modulePanels.Count)];

        newModule.SetPanel(panel);
        newModule.transform.localEulerAngles = new Vector3(0, 180, 0);
        _currentModules.Add(newModule);

        return newModule;
    }

    private ModulePanel GetPanelWithId(int panelId)
    {
        foreach (var mp in _modulePanels)
        {
            if (mp._id == panelId)
                return mp;
        }
        return null;
    }

    // A module layout will comprise of 
    //  - a console base which this layout can be applied to
    //  - a name for this layout
    //  - a list of each module containing the following
    //    - the identifier for the module
    //    - the panel the module is attached to
    //    - the modules local position, scale, and rotation
    [System.Serializable]
    public class ModuleLayout
    {
        [System.Serializable]
        public struct ModuleConfig
        {
            public int id;
            public int panelId;
            public Vector3 position;
            public Vector3 scale;
            public Vector3 eulerAngles;
        }

        public string _layoutName;
        public int _consoleBaseId;
        public List<ModuleConfig> _panelLayouts;

    }

    public void SaveCurrentLayout()
    {
        // Build a list of all modules,
        // which panels they are installed on,
        // what their locaton on that panel is

        var layout = new ModuleLayout();
        layout._layoutName = "Test Layout";
        layout._consoleBaseId = _id;

        layout._panelLayouts = new List<ModuleLayout.ModuleConfig>();

        foreach (var m in _currentModules)
        {
            var config = new ModuleLayout.ModuleConfig()
            {
                id = m._id,
                panelId = m._panel._id,
                position = m.transform.localPosition,
                scale = m.transform.localScale,
                eulerAngles = m.transform.localEulerAngles
            };
            layout._panelLayouts.Add(config);
        }

        _savedLayout = layout;
        var json = JsonUtility.ToJson(layout);
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var jsonFilename = layout._layoutName + "Layout.json";
        var jsonPath = Path.Combine(Application.temporaryCachePath, jsonFilename);
        File.WriteAllBytes(jsonPath, jsonBytes);

        var report = new ReportBuilder();
        report.layout = layout;
        report.SaveReport();

        // TODO: What about annotations? Currently a line renderer, but that is hard to capture in a report document
    }

    public void LoadLayoutFrom()
    {
        
        // Restore layout from a serialized file
        // Modules will need an id so we can create the right ones
        // ConsoleBase game object may also change(?) or should just check that the layout is compatible with this Console
        var jsonPath = Path.Combine(Application.temporaryCachePath, "TempLayout.json");
        if (File.Exists(jsonPath) == false)
            return;

        Debug.Log($"Loading from file {jsonPath}");

        var jsonBytes = File.ReadAllBytes(jsonPath);
        var json = Encoding.UTF8.GetString(jsonBytes);
        var layout = JsonUtility.FromJson<ModuleLayout>(json);

        // remove all current modules
        foreach (var panel in _modulePanels)
        {
            panel._modules.Clear();
        }
        foreach (var mod in _currentModules)
        {
            Destroy(mod.gameObject);
        }
        _currentModules.Clear();

        // load all listed modules in layout
        foreach (var m in layout._panelLayouts)
        {
            var module = AddModule(m.id, m.panelId);
            module.transform.localPosition = m.position;
            module.transform.localScale = m.scale;
            module.transform.localEulerAngles = m.eulerAngles;
        }
    }

    public void Start()
    {
        _modulePanels = new List<ModulePanel>(GetComponentsInChildren<ModulePanel>());

        var installedModules = new List<ConsoleModule>(GetComponentsInChildren<ConsoleModule>());

        print($"Found {installedModules.Count} installed modules");

        foreach (var module in installedModules)
        {
            print(module._id);
        }

        _currentModules = installedModules;
    }

    public void DuplicateRandomModule()
    {
        var targetModule = _currentModules[Random.Range(0, _currentModules.Count)];
        AddModule(targetModule._id, targetModule._panel._id);
    }
}
