using UnityEngine;

public partial class ConsoleBase
{
    public Rect windowRect = new Rect(20, 20, 100, 100);
    public Vector2 scrollPos = new Vector2();

    void OnGUI()
    {
        windowRect.width = Screen.safeArea.width / 5;

        // Register the window. Notice the 3rd parameter
        windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "Console Base Debug Data");
    }

    public void DoMyWindow(int windowID)
    {
        if (GUILayout.Button("Duplicate random module"))
            DuplicateRandomModule();

        if (GUILayout.Button("Save current layout"))
            SaveCurrentLayout();

        if (GUILayout.Button("Load layout"))
            LoadLayoutFrom();

        GUILayout.Label("Installed Modules");
        using (var scrollView = new GUILayout.ScrollViewScope(scrollPos, GUILayout.MinHeight(250), GUILayout.MinWidth(Screen.safeArea.width / 4)))
        {
            scrollPos = scrollView.scrollPosition;

            foreach (var panel in _modulePanels)
            {
                GUILayout.Label($"Module Panel {panel._id}");
                GUILayout.BeginVertical("box");
                foreach (var m in panel._modules)
                {
                    GUILayout.Label($"{m.gameObject.name} (id = {m._id})");
                    GUILayout.Label($" - Position: {m.transform.localPosition.ToString("F4")}", "box");
                    GUILayout.Label($" - Scale: {m.transform.localScale.ToString("F4")}", "box");
                    GUILayout.Label($" - Rotation: {m.transform.localEulerAngles}", "box");
                }
                GUILayout.EndVertical();
            }
        }

        GUI.DragWindow();
    }
}
