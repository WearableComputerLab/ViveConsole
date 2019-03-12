using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class CustomBuild : EditorWindow
{
	string exeName = "Test";
	BuildTarget target = BuildTarget.StandaloneWindows64;
		
	[MenuItem("Window/aiImporter - Custom Build")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(CustomBuild));
	}
	
	Object selectedObject = null;
	
	void OnGUI ()
	{
		GUILayout.Label ("This will copy the aiImporter files to your executable's data directory, so that you don't have to copy them manually each time you build your scene.", EditorStyles.wordWrappedLabel);

		target = (BuildTarget)EditorGUILayout.EnumPopup ("Build Target :", target);
		exeName = EditorGUILayout.TextField ("Executable Name :", exeName);

		GUILayout.Label ("Select Scene", EditorStyles.boldLabel);

		string commandName = Event.current.commandName;
		if (commandName == "ObjectSelectorClosed") {
			selectedObject = EditorGUIUtility.GetObjectPickerObject ();
		}

		EditorGUILayout.ObjectField (selectedObject, typeof(Object), true);

		if (selectedObject != null)
			GUI.enabled = AssetDatabase.GetAssetPath(selectedObject).Contains (".unity");
		else
			GUI.enabled = false;

		if (GUILayout.Button ("Build Scene")) 
			sceneBuild ();

		GUI.enabled = true;
	}

	void sceneBuild()
	{
		if (PlayerSettings.apiCompatibilityLevel != ApiCompatibilityLevel.NET_2_0)
			PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
		
		string[] levels = new string[]{AssetDatabase.GetAssetPath(selectedObject)};
		BuildPipeline.BuildPlayer(levels, "Build/" + exeName + ".exe", BuildTarget.StandaloneWindows64, BuildOptions.ShowBuiltPlayer);
		
		Directory.CreateDirectory("Build/" + exeName + "_Data/aiImporter");
		FileUtil.CopyFileOrDirectory("Assets/aiImporter/Importer", "Build/" + exeName+ "_Data/aiImporter/Importer");
	}
}