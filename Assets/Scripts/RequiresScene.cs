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

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
#else
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneOpened -= EditorSceneManager_sceneOpened;
#else
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
#endif
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (gameObject.scene == arg0)
        {
            var buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
            var requiredScene = SceneManager.GetSceneByBuildIndex(buildIndex);

            if (string.IsNullOrEmpty(scenePath) == false && 
                requiredScene.isLoaded == false)
            {
                SceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
            }
        }
    }

#if UNITY_EDITOR
    private void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (UnityEditor.EditorApplication.isPlaying)
            return;

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
#endif

}
