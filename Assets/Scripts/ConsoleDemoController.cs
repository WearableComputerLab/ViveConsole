using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConsoleDemoController : MonoBehaviour
{
    Dictionary<string, Scene> sceneCache = new Dictionary<string, Scene>();

    async public void LoadRadialMenuDemo()
    {
        await LoadExtraSceneAsync("RadialMenu");
    }
    async public void LoadVoiceCommandsDemo()
    {
        await LoadExtraSceneAsync("VoiceCommands");
    }
    async public void LoadVoiceAnnotionsDemo()
    {
        await LoadExtraSceneAsync("VoiceAnnotations");
    }

    async public Task LoadExtraSceneAsync(string sceneKey)
    {
        await UnloadExtraScenes();
        SceneManager.LoadScene(sceneCache[sceneKey].buildIndex, LoadSceneMode.Additive);
    }

    private void Start()
    {
        sceneCache = new Dictionary<string, Scene>
        {
            { "RadialMenu", SceneManager.GetSceneByName("RadialMenu") },
            { "VoiceCommands", SceneManager.GetSceneByName("VoiceCommands") },
            { "VoiceAnnotations", SceneManager.GetSceneByName("VoiceAnnotations") }
        };
    }

    async public Task UnloadExtraScenes()
    {
        foreach (var s in sceneCache.Values)
        {
            if (s.isLoaded)
            {
                var unloadOp = SceneManager.UnloadSceneAsync(s);
                while (unloadOp.isDone == false)
                    await Task.Delay(1);
            }
        }
    }
    public void OnActivateDisplaysPressed()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            //Display.displays[2].Activate();
        }
    }

    public void OnClearAllAnnotationsPressed()
    {
        var layers = FindObjectsOfType<AnnotateTexture>();
        foreach(var at in layers)
        {
            at.ClearTexture();
        }
    }
}
