using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleDemoController : MonoBehaviour
{
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
