using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class RequiresScene : MonoBehaviour
{
    [Tooltip("Full path to scene file relative to project (i.e. Assets/Scenes/scene.unity)")]
    public string scenePath;

#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
    }

    private void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (gameObject.scene == scene)
        {
            if (string.IsNullOrEmpty(scenePath) == false &&
                SceneManager.GetSceneByName(scenePath).isLoaded == false)
            {
                if (System.IO.Path.GetExtension(scenePath) != ".unity")
                    scenePath += ".unity";
                EditorSceneManager.OpenScene(scenePath, mode == OpenSceneMode.AdditiveWithoutLoading ? mode : OpenSceneMode.Additive);
            }
        }
    }

    private void OnDisable()
    {
        EditorSceneManager.sceneOpened -= EditorSceneManager_sceneOpened;
    }

#endif

}
