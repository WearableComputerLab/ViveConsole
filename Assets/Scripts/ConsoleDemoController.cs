using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleDemoController : MonoBehaviour
{
    public void OnActivateDisplaysPressed()
    {
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
    }
}
