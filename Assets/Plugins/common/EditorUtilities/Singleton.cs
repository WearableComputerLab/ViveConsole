/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 20178 Andrew Irlitti.
*
* If you use/extend/modify this code, add your name and email address
* to the AUTHORS file in the root directory.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*
*/

using UnityEngine;

/// <summary>
/// A basic Singleton to extend for MonoBehaviour applications
/// </summary>
/// <typeparam name="T">The Singleton type</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : class
{
    private static T instance = null;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (instance == null)
                    Debug.LogError("Singleton<T>: Could not found GameObject of type " + typeof(T).Name);
            }
            return instance;
        }
    }
}
