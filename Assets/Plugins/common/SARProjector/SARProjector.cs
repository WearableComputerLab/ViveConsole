/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
*               2017 Daniel Jackson.
*               2012 Michael Marner.
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
using System.IO;
using UnitySARCommon.IO;
using UnitySARCommon.Editor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnitySARCommon.Projector
{
    /// <summary>
    /// A class to support the use of Projectors in Unity.  Provides precomputed intrinsic and extrinsic matrices to a camera.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [System.Serializable]
    public class SARProjector : MonoBehaviour
    {
        // turn on debug mode
        [System.Obsolete("version 1.0 variable")]
        private bool debug = false;

        // the unity display id to be associated with this projector
        private int displayID;

        // the filepath to be used for loading matrices
        [System.Obsolete("version 1.0 variable")]
        private string extrinsicMatrixFilePath;
        [System.Obsolete("version 1.0 variable")]
        private string intrinsicMatrixFilePath;

        [SerializeField] private string intAbsoluteFilePath;
        [SerializeField] private string intAssetsFilePath;
        [SerializeField] private string intCustomFilePath;

        [SerializeField] private string extAbsoluteFilePath;
        [SerializeField] private string extAssetsFilePath;
        [SerializeField] private string extCustomFilePath;

        // These are the default values associated with FriendlyCalibration
        [ReadOnly] [SerializeField] public const float calibrationNear = 0.1f;
        [ReadOnly] [SerializeField] public const float calibrationFar = 10000f;

        /// <summary>
        /// Available source options for using a matrix in SARProjector
        /// </summary>
        private enum LoadMatrixFrom
        {
            File,
            Camera
        };
        [SerializeField] LoadMatrixFrom loadMatrixFrom;

        /// <summary>
        /// Available source options for loading a matrix in SARProjector
        /// </summary>
        private enum LoadFileFromSource
        {
            Off,
            Absolute,
            ProjectAssets,
            ProjectorManagerCustom
        };
        [SerializeField] LoadFileFromSource loadFileInt;
        [SerializeField] LoadFileFromSource loadFileExt;

        // Camera variables for setting the SARProjector Transform
        [SerializeField] private Vector3 cameraPos;
        [SerializeField] private Quaternion cameraRot;
        [SerializeField] private Vector3 cameraScale;

        [SerializeField] private int cameraSelection;

        //-- OLD SARPROJECTOR VARIABLES

        /// For Editor:
        /// Add the file location of your intrinsic and extrinsic matrices in the editor
        /// 
        /// For Executables:
        /// Store your matrices in the format [Int/Mat]<displayID>.dat inside a top level folder /CalibrationData

        //bool debug = false;
        //private int displayID;

        [System.Obsolete("version 1.0 variable")]
        public bool renderFromMatrices = false;

        [Tooltip("Absolute File Location from Top Directory \n (the folder containing /Assets & /Project)")]
        [System.Obsolete("version 1.0 variable")]
        public string intrinsicMatrixFile;

        [Tooltip("Absolute File Location from Top Directory \n (the folder containing /Assets & /Project)")]
        [System.Obsolete("version 1.0 variable")]
        public string extrinsicMatrixFile;

        [SerializeField] Matrix4x4 intMat = Matrix4x4.identity;
        [SerializeField] Matrix4x4 extMat = Matrix4x4.identity;

        [System.Obsolete("version 1.0 variable")]
        private const float calibNear = 0.1f;
        [System.Obsolete("version 1.0 variable")]
        private const float calibFar = 10000f;

        //-- OLD SARPROJECTOR VARIABLES -- END

        #if UNITY_EDITOR
        /// <summary>
        /// Add the SARProjector option to the heirarchy menu.
        /// Right click and create a SARProjector game object.
        /// </summary>
        [MenuItem("GameObject/SARProjector", false, 12)]
        static void CreateSARProjector()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<SARProjector>().name = "SARProjector";
        }
        #endif

        void Start()
        {
            UpdateCameraParameters();
        }

        void Update()       { }
        void OnValidate()   { }

        public int DisplayID
        {
            get { return displayID; }
            set { displayID = value; }
        }


        /// <summary>
        /// Load an intrinsic matrix from the provided file.
        /// The currently accepted format is a .dat file with 4 x 4 values separated by spaces.
        /// </summary>
        /// <param name="path">The file path for the matrix</param>
        /// <returns>Success of the load</returns>
        public bool LoadIntrinsicMatrix(string path)
        {
            intMat = MatrixFromFile(path);

            if (intMat.isIdentity)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Load an extrinsic matrix from the provided file.
        /// The currently accepted format is a .dat file with 4 x 4 values separated by spaces.
        /// </summary>
        /// <param name="path">The file path for the matrix</param>
        /// <returns>Success of the load</returns>
        public bool LoadExtrinsicMatrix(string path)
        {
            extMat = MatrixFromFile(path);

            if (extMat.isIdentity)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Load an intrinsic matrix from the provided file.
        /// The currently accepted format is a .dat file with 4 x 4 values separated by spaces.
        /// </summary>
        /// <param name="path">The file path for the matrix</param>
        /// <returns>The loaded matrix for Serialisation</returns>
        public Matrix4x4 LoadIntrinsicMatrixEditor(string path)
        {
            intMat = MatrixFromFile(path);

            return intMat;
        }

        /// <summary>
        /// Load an extrinsic matrix from the provided file.
        /// The currently accepted format is a .dat file with 4 x 4 values separated by spaces.
        /// </summary>
        /// <param name="path">The file path for the matrix</param>
        /// <returns>The loaded matrix for Serialisation</returns>
        public Matrix4x4 LoadExtrinsicMatrixEditor(string path)
        {
            extMat = MatrixFromFile(path);

            return extMat;
        }


        /// <summary>
        /// Update the Transform properties of the SARProjector given an extrinsic matrix.
        /// </summary>
        public void UpdateProjectorTransform()
        {
            cameraPos = extMat.inverse.MultiplyPoint(Vector3.zero);
            cameraRot = Quaternion.LookRotation(-extMat.transpose.GetColumn(2), extMat.transpose.GetColumn(1));
            cameraScale = new Vector3(extMat.GetColumn(0).magnitude, extMat.GetColumn(1).magnitude, extMat.GetColumn(2).magnitude);
        }

        /// <summary>
        /// Updates the attached camera component parameters with information stored in the local intrinsic and extrinsic matrix.
        /// These matrices can either be loaded from file, applied from another camera, or manually added in the editor.
        /// </summary>
        public void UpdateCameraParameters()
        {
            // set the camera's intrinsic and extrinsic properties
            GetComponent<Camera>().projectionMatrix = intMat;
            GetComponent<Camera>().worldToCameraMatrix = extMat;

            // set the camera's transform properties
            GetComponent<Transform>().position = cameraPos;
            GetComponent<Transform>().rotation = cameraRot;
            GetComponent<Transform>().localScale = cameraScale;

            // set the camera's calibration near/far Planes
            GetComponent<Camera>().nearClipPlane = calibrationNear;
            GetComponent<Camera>().farClipPlane = calibrationFar;
        }

        /// <summary>
        /// Resets the attached camera component parameters with default unity values
        /// </summary>
        /// <param name="projectionMatrix">Disable the projection / intrinsic matrix</param>
        /// <param name="worldCameraMatrix">Disable the world-camera / extrinsic matrix</param>
        public void DisableMatrix(bool projectionMatrix, bool worldCameraMatrix)
        {
            if (projectionMatrix)
            {
                GetComponent<Camera>().ResetProjectionMatrix();
                //intMat = Matrix4x4.identity;
            }

            if (worldCameraMatrix)
            {
                GetComponent<Camera>().ResetWorldToCameraMatrix();
                //extMat = Matrix4x4.identity;
            }
        }

        [System.Obsolete("New Editor function loads Intrinsic & Extrinsic Matrices separately.  Use LoadInstrinsicMatrix(string), LoadExtrinsicMatrix(string), and UpdateCameraParameters() instead")]
        public void LoadMatrices()
        {
            Quaternion q = this.transform.rotation;
            Vector3 v = this.transform.localScale;
            Vector3 t = this.transform.position;

            if (debug)
                Debug.Log("SARProjector: Loading Matrices for Projector " + (displayID + 1));

            // Attempt to load Default 
            intMat = MatrixFromFile("CalibrationData/IntMat" + displayID + ".dat");
            extMat = MatrixFromFile("CalibrationData/ExtMat" + displayID + ".dat");

            if (intMat.isIdentity)
            {
                if (intrinsicMatrixFile == null || intrinsicMatrixFile.Length == 0)
                {
                    if (debug)
                        Debug.Log("SARProjector: No intrinsic matrix set for " + name);
                }
                else if (!File.Exists(intrinsicMatrixFile))
                {
                    if (debug)
                        Debug.Log("SARProjector: File \"" + intrinsicMatrixFile + "\" not found");
                }
                else
                {
                    // Load intrinsic matrix from file
                    if (debug)
                        Debug.Log("SARProjector: Success! Loading Intrinsic Matrix from \"" + intrinsicMatrixFile + "\"");
                    intMat = MatrixFromFile(intrinsicMatrixFile);
                }
            }

            if (extMat.isIdentity)
            {
                if (extrinsicMatrixFile == null || extrinsicMatrixFile.Length == 0)
                {
                    if (debug)
                        Debug.Log("SARProjector: No extrinsic matrix set for " + name);
                }
                else if (!File.Exists(extrinsicMatrixFile))
                {
                    if (debug)
                        Debug.Log("SARProjector: File \"" + extrinsicMatrixFile + "\" not found");
                }
                else
                {
                    // Load extrinsic matrix from file
                    if (debug)
                        Debug.Log("SARProjector: Success! Loading Extrinsic Matrix from \"" + extrinsicMatrixFile + "\"");
                    extMat = MatrixFromFile(extrinsicMatrixFile);
                }
            }

            // set the unity camera's intrinsic and extrinsic properties
            GetComponent<Camera>().projectionMatrix = intMat;
            GetComponent<Camera>().worldToCameraMatrix = extMat;

            // set the transform editor properties 
            Vector3 cameraPos = extMat.inverse.MultiplyPoint(Vector3.zero);
            //Quaternion cameraRot = Quaternion.identity;
            //Vector3 cameraScale = Vector3.one;
            Quaternion cameraRot = q;
            Vector3 cameraScale = v;

            GetComponent<Transform>().position = cameraPos;
            GetComponent<Transform>().rotation = cameraRot;
            GetComponent<Transform>().localScale = cameraScale;

            // set the calibration Near/Far Planes
            GetComponent<Camera>().nearClipPlane = calibNear;
            GetComponent<Camera>().farClipPlane = calibFar;
        }


        public void setTargetDisplay(int display)
        {
            displayID = display;
        }

        private Matrix4x4 MatrixFromFile(string file)
        {
            if (file == null || file.Length == 0)
            {
                if (debug)
                    Debug.Log("SARProjector: No calibration matrix file set");
                return Matrix4x4.identity;
            }
            else if (!File.Exists(file))
            {
                if (debug)
                    Debug.Log("SARProjector: File \"" + file + "\" not found");
                return Matrix4x4.identity;
            }
            else
            {
                if (debug)
                    Debug.Log("SARProjector: Success! Loading Matrix from \"" + file + "\"");
                Matrix4x4 mat = MatrixIO.ReadMatrixFromFile(file);
                return mat;
            }
        }



        #if UNITY_EDITOR
        [CustomEditor(typeof(SARProjector))]
        public class SARProjectorEditor : UnityEditor.Editor
        {
            SARProjector projectorScript;
            Camera cameraScript;

            SerializedProperty m_cameraSelection;

            SerializedProperty m_intMat;
            SerializedProperty m_extMat;
            SerializedProperty m_loadMatrixFrom;
            SerializedProperty m_loadIntMatrix;
            SerializedProperty m_loadExtMatrix;

            SerializedProperty m_intAbsoluteFilePath;
            SerializedProperty m_intAssestsFilePath;
            SerializedProperty m_intCustomFilePath;
            SerializedProperty m_extAbsoluteFilePath;
            SerializedProperty m_extAssestsFilePath;
            SerializedProperty m_extCustomFilePath;

            // Camera Parameters
            private string[] cameraDisplayStrings = { "Display 1", "Display 2", "Display 3", "Display 4", "Display 5", "Display 6", "Display 7", "Display 8" };
            private int[] cameraDisplayValues = { 0, 1, 2, 3, 4, 5, 6, 7 };

            protected void OnEnable()
            {
                projectorScript = (SARProjector)target;
                cameraScript = projectorScript.GetComponent<Camera>();

                m_cameraSelection = this.serializedObject.FindProperty("cameraSelection");

                m_intMat = this.serializedObject.FindProperty("intMat");
                m_extMat = this.serializedObject.FindProperty("extMat");

                m_loadMatrixFrom = this.serializedObject.FindProperty("loadMatrixFrom");
                m_loadIntMatrix = this.serializedObject.FindProperty("loadFileInt");
                m_loadExtMatrix = this.serializedObject.FindProperty("loadFileExt");

                m_intAbsoluteFilePath = this.serializedObject.FindProperty("intAbsoluteFilePath");
                m_intAssestsFilePath = this.serializedObject.FindProperty("intAssetsFilePath");
                m_intCustomFilePath = this.serializedObject.FindProperty("intCustomFilePath");
                m_extAbsoluteFilePath = this.serializedObject.FindProperty("extAbsoluteFilePath");
                m_extAssestsFilePath = this.serializedObject.FindProperty("extAssetsFilePath");
                m_extCustomFilePath = this.serializedObject.FindProperty("extCustomFilePath");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

               
                if (cameraScript != null)
                {
                    // If the projector script is enabled, disable the attached camera parameters
                    // Update the Helpbox with appropriate message and show the SARProjector properties
                    // If the projector script is disabled, hide all SARProjector information and update the Helpbox with relevant information
                    if (projectorScript.enabled)
                    {
                        projectorScript.GetComponent<Camera>().hideFlags = HideFlags.NotEditable;
                        UnityEditorInternal.ComponentUtility.MoveComponentUp(projectorScript);
                        //UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(projectorScript.GetComponent<Camera>(), false);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("Changing camera parameters are not allowed while using SARProjector.  \nPlease modify the available camera parameters below.", MessageType.Info);
                        EditorGUILayout.EndHorizontal();

                        SetCameraParameters();

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("SARProjector Properties", EditorStyles.boldLabel, GUILayout.MinWidth(100f));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        projectorScript.GetComponent<Camera>().hideFlags = HideFlags.None;
                        //UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(projectorScript.GetComponent<Camera>(), true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("SARProjector is no longer controlling the camera.  \nReenabling the SARProjector will revert back to SARProjector control.", MessageType.Info);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();

                    // Set the appopriate load from layout matrices
                    SetMatrixFromLayout(projectorScript.loadMatrixFrom);

                    // Provide the user with options for loading an intrinsic matrix
                    //EditorGUILayout.BeginHorizontal();
                    //EditorGUILayout.LabelField("Use Matrices From", GUILayout.MinWidth(100f));
                    //EditorGUILayout.Space();
                    //projectorScript.loadMatrixFrom = (SARProjector.LoadMatrixFrom)EditorGUILayout.EnumPopup(projectorScript.loadMatrixFrom, GUILayout.MinWidth(250f));
                    //EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_loadMatrixFrom, new GUIContent("Use Matrices From"), GUILayout.MinWidth(250f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        int loadMatrixFrom = m_loadMatrixFrom.enumValueIndex;
                        m_loadMatrixFrom.enumValueIndex = loadMatrixFrom;
                    }

                    EditorGUILayout.Space();

                    /// <summary>
                    /// Update the SARProjector based on using a matrix from different sources.
                    /// Camera: Utilise an existing projection/world matrices from another camera in the scene
                    /// File: Load a matrix from a file source.
                    /// </summary>
                    switch (projectorScript.loadMatrixFrom)
                    {
                        case SARProjector.LoadMatrixFrom.Camera:
                            {
                                string[] cameraStrings = new string[Camera.allCamerasCount];
                                Camera[] cameras = Camera.allCameras;

                                for (int i = 0; i < Camera.allCamerasCount; i++)
                                {
                                    if (cameras[i].gameObject != projectorScript.gameObject)
                                        cameraStrings[i] = string.Format("Camera: {0}", cameras[i].name);
                                }

                                if (cameraStrings.Length > 0)
                                {
                                    //EditorGUILayout.BeginHorizontal();
                                    //EditorGUILayout.LabelField("Camera parameters copied from: ", GUILayout.MinWidth(100f));
                                    //EditorGUILayout.Space();
                                    //int cameraSelection = EditorGUILayout.Popup(projectorScript.cameraSelection, cameraStrings);
                                    ////projectorScript.cameraSelection = EditorGUILayout.Popup(projectorScript.cameraSelection, cameraStrings);
                                    //EditorGUILayout.EndHorizontal();

                                    EditorGUI.BeginChangeCheck();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Camera parameters copied from: ", GUILayout.MinWidth(100f));
                                    EditorGUILayout.Space();
                                    int cameraSelection = EditorGUILayout.Popup(projectorScript.cameraSelection, cameraStrings);
                                    EditorGUILayout.EndHorizontal();

                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        m_cameraSelection.intValue = cameraSelection;
                                    }


                                    // update the matrices in the editor
                                    projectorScript.intMat = cameras[projectorScript.cameraSelection].projectionMatrix;
                                    projectorScript.extMat = cameras[projectorScript.cameraSelection].worldToCameraMatrix;

                                    // update the transform properties
                                    projectorScript.cameraPos = cameras[projectorScript.cameraSelection].transform.position;
                                    projectorScript.cameraRot = cameras[projectorScript.cameraSelection].transform.rotation;
                                    projectorScript.cameraScale = cameras[projectorScript.cameraSelection].transform.localScale;

                                    // render the matrix layout with the updated int/ext matrices
                                    SetMatrixParentLayout(false);
                                    projectorScript.UpdateCameraParameters();
                                }
                                else
                                {
                                   EditorGUILayout.HelpBox("No alternative cameras are available to select from inside the scene", MessageType.Error);
                                }                               

                                break;
                            }
                        case SARProjector.LoadMatrixFrom.File:
                            {
                                // If the projector script is enabled, render options to the editor window
                                if (projectorScript.enabled)
                                {
                                    /////////////////////////////////////// Intrinsic Matrix ///////////////////////////////////////

                                    EditorGUILayout.Space();
                                    string mString = "Intrinsic";

                                    // Set the appopriate load layout for intrinsic matrices
                                    SetMatrixLoadLayout(projectorScript.loadFileInt, mString);

                                    // Show the file load layout for an intrinsic matrix
                                    EditorGUI.BeginChangeCheck();
                                    switch (projectorScript.loadFileInt)
                                    {
                                        case LoadFileFromSource.Absolute:
                                            SetLoadMatrixFromFileLayout(ref projectorScript.loadFileInt, mString, ref projectorScript.intAbsoluteFilePath);
                                            break;
                                        case LoadFileFromSource.ProjectAssets:
                                            SetLoadMatrixFromFileLayout(ref projectorScript.loadFileInt, mString, ref projectorScript.intAssetsFilePath);
                                            break;
                                        case LoadFileFromSource.ProjectorManagerCustom:
                                            SetLoadMatrixFromFileLayout(ref projectorScript.loadFileInt, mString, ref projectorScript.intCustomFilePath);
                                            break;
                                        default:
                                            SetLoadMatrixFromFileLayout(ref projectorScript.loadFileInt, mString, ref projectorScript.intCustomFilePath);
                                            break;
                                    }

                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        m_loadIntMatrix.enumValueIndex = (int)projectorScript.loadFileInt;
                                        m_intAbsoluteFilePath.stringValue = projectorScript.intAbsoluteFilePath;
                                        m_intAssestsFilePath.stringValue = projectorScript.intAssetsFilePath;
                                        m_intCustomFilePath.stringValue = projectorScript.intCustomFilePath;
                                    }

                                    //SetLoadMatrixFromFileLayout(ref projectorScript.loadFileInt, mString, ref projectorScript.intrinsicMatrixFilePath);


                                    /////////////////////////////////////// Extrinsic Matrix ///////////////////////////////////////

                                    EditorGUILayout.Space();
                                    mString = "Extrinsic";

                                    // Set the appopriate load layout for Extrinsic matrices
                                    SetMatrixLoadLayout(projectorScript.loadFileExt, mString);

                                    // Show the file load layout for an intrinsic matrix
                                    EditorGUI.BeginChangeCheck();
                                    switch (projectorScript.loadFileExt)
                                    {
                                        case LoadFileFromSource.Absolute:
                                            SetLoadMatrixFromFileLayout(ref projectorScript.loadFileExt, mString, ref projectorScript.extAbsoluteFilePath);
                                            break;
                                        case LoadFileFromSource.ProjectAssets:
                                            SetLoadMatrixFromFileLayout(ref projectorScript.loadFileExt, mString, ref projectorScript.extAssetsFilePath);
                                            break;
                                        case LoadFileFromSource.ProjectorManagerCustom:
                                            SetLoadMatrixFromFileLayout(ref projectorScript.loadFileExt, mString, ref projectorScript.extCustomFilePath);
                                            break;
                                        default:
                                            SetLoadMatrixFromFileLayout(ref projectorScript.loadFileExt, mString, ref projectorScript.extCustomFilePath);
                                            break;
                                    }

                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        m_loadExtMatrix.enumValueIndex = (int)projectorScript.loadFileExt;
                                        m_extAbsoluteFilePath.stringValue = projectorScript.extAbsoluteFilePath;
                                        m_extAssestsFilePath.stringValue = projectorScript.extAssetsFilePath;
                                        m_extCustomFilePath.stringValue = projectorScript.extCustomFilePath;
                                    }

                                    //SetLoadMatrixFromFileLayout(ref projectorScript.loadFileExt, mString, ref projectorScript.extrinsicMatrixFilePath);


                                    /////////////////////////////////////// Matrix Layout ///////////////////////////////////////

                                    SetMatrixParentLayout(false);

                                    // Update the transform properties of the projector
                                    projectorScript.UpdateProjectorTransform();

                                    // Update the camera associated properties of the projector
                                    projectorScript.UpdateCameraParameters();
                                }

                                break;
                            }
                        default:
                            break;
                    }
                   
                    serializedObject.ApplyModifiedProperties();
                }
                else  // cameraScript == null // this is bad!
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("SARProjector requires a Camera component to function. \nSARProjector will not work until this is resolved.", MessageType.Error);
                    EditorGUILayout.EndHorizontal();
                }
            }

            /// <summary>
            /// Set up and layout the file load parameters for a given matrix.
            /// </summary>
            /// <param name="source">The source to loadfrom</param>
            /// <param name="mString">The matrix string to append into the UI</param>
            /// <param name="mFilePath">The filepath to update from the UI</param>
            void SetLoadMatrixFromFileLayout(ref LoadFileFromSource source, string mString, ref string mFilePath)
            {
                EditorGUI.BeginChangeCheck();

                // Provide the user with options for loading an intrinsic matrix
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("Load {0} Matrix", mString), GUILayout.MinWidth(100f));
                EditorGUILayout.Space();
                source = (LoadFileFromSource)EditorGUILayout.EnumPopup(source, GUILayout.MinWidth(250f));
                EditorGUILayout.EndHorizontal();

                if ((source == LoadFileFromSource.Absolute) || (source == LoadFileFromSource.ProjectAssets) || (source == LoadFileFromSource.ProjectorManagerCustom))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("{0} Matrix File Path", mString), GUILayout.MinWidth(100f));
                    EditorGUILayout.Space();
                    
                    mFilePath = EditorGUILayout.TextField(mFilePath, GUILayout.MinWidth(250f));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();

                    // disable the button if no filepath is provided
                    if (mFilePath != null)
                    {
                        GUI.enabled = true;
                    }
                    else
                    {
                        GUI.enabled = false;
                    }

                    // The load button
                    if (GUILayout.Button(string.Format("Load {0} Matrix", mString), GUILayout.MinWidth(150f), GUILayout.MaxWidth(150f)))
                    {
                        string loadPath = mFilePath;

                        switch (source)
                        {
                            case LoadFileFromSource.ProjectAssets:
                                {
                                    loadPath = Application.dataPath +"/"+ mFilePath;
                                    break;
                                }
                            case LoadFileFromSource.ProjectorManagerCustom:
                                {
                                    SARProjectorBoss projBoss = FindObjectOfType<SARProjectorBoss>();

                                    if (projBoss != null)
                                    {
                                        loadPath = projBoss.matrixDirectory + "/" + mFilePath;
                                    }
                                    else
                                    {
                                        if (EditorUtility.DisplayDialog("Missing Projector Boss", "There is no Projector Boss currently added to the scene.  " +
                                                "To use custom file paths, you will require one.  Do you want to add one into your scene?", "Create", "Cancel"))
                                        {
                                            GameObject pb = new GameObject();
                                            pb.AddComponent<SARProjectorBoss>().name = "SARProjector Manager";

                                            return;
                                        }
                                    }
                                    break;
                                }
                            default:
                                break;
                        }

                        if (mString.Equals("Intrinsic"))
                        {
                            Matrix4x4 intMat = projectorScript.LoadIntrinsicMatrixEditor(loadPath);

                            if (!intMat.isIdentity)
                            {
                                EditorUtility.DisplayDialog("Successful Load", "The intrinsic matrix was loaded successfully", "OK");
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Load Error", "There was an issue loading the intrinsic matrix.  " +
                                    "\nPlease check for errors in: \n\n--the filename \n--the location of your matrix \n--the formatting of your matrix file", "OK");
                            }

                            m_intMat.Matrix4x4Value(intMat);
                        }
                        else // "Extrinsic"
                        {
                            Matrix4x4 extMat = projectorScript.LoadExtrinsicMatrixEditor(loadPath);

                            if (!extMat.isIdentity)
                            {
                                EditorUtility.DisplayDialog("Successful Load", "The extrinsic matrix was loaded successfully", "OK");
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Load Error", "There was an issue loading the extrinsic matrix.  " +
                                    "\nPlease check for errors in: \n\n--the filename \n--the location of your matrix \n--the formatting of your matrix file", "OK");
                            }

                            m_extMat.Matrix4x4Value(extMat);
                        }
                    }

                    GUI.enabled = true;

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
            }

            /// <summary>
            /// Set up and layout the help box for the various LoadMatrixFrom enum variables.
            /// </summary>
            /// <param name="source">The source to load from</param>
            void SetMatrixFromLayout(SARProjector.LoadMatrixFrom source)
            {
                switch (source)
                {
                    case SARProjector.LoadMatrixFrom.Camera:
                        {
                            EditorGUILayout.HelpBox("Matricies will be loaded from an existing camera in the scene.  \nPlease select a camera from the available options.", MessageType.Info);
                            break;
                        }
                    case SARProjector.LoadMatrixFrom.File:
                        {
                            EditorGUILayout.HelpBox("Matricies will be loaded from file.  \nPlease enter the required locations then press Load Matrix for each matrix.", MessageType.Info);
                            break;
                        }
                    default:
                        break;
                }
            }

            /// <summary>
            /// Set up and layout the help box for the various LoadFileFromSource enum variables.
            /// </summary>
            /// <param name="source">The file source to load from</param>
            /// <param name="matrixString">The matrix string to append into the helpbox</param>
            void SetMatrixLoadLayout(SARProjector.LoadFileFromSource source, string matrixString)
            {
                switch (source)
                {
                    case SARProjector.LoadFileFromSource.Off:
                        {
                            EditorGUILayout.HelpBox(matrixString + " Matrix is not currently being loaded from file", MessageType.Warning);
                            break;
                        }
                    case SARProjector.LoadFileFromSource.Absolute:
                        {
                            EditorGUILayout.HelpBox(matrixString + " Matrix will be loaded from an absolute path", MessageType.Info);
                            break;
                        }
                    case SARProjector.LoadFileFromSource.ProjectAssets:
                        {
                            EditorGUILayout.HelpBox(matrixString + " Matrix will be loaded from a relative path to this project's Assets folder", MessageType.Info);
                            EditorGUILayout.HelpBox("Files NOT located inside Assets/Resources/ will not be loaded in a build", MessageType.Error);
                            break;
                        }
                    case SARProjector.LoadFileFromSource.ProjectorManagerCustom:
                        {
                            EditorGUILayout.HelpBox(matrixString + " Matrix will be loaded from a relative path to the custom path stored in ProjectorManager", MessageType.Info);
                            break;
                        }
                    default:
                        break;
                }
            }

            /// <summary>
            /// Set up and layout the parent for 4x4 matrices used by the SARProjector.  
            /// The parent handles the matrix labels, and determines whether the matrices are enabled for edit.
            /// </summary>
            /// <param name="edit">Whether the matrices are enabled for editting</param>
            void SetMatrixParentLayout(bool edit)
            {
                // Render the loaded Intrinsic & Extrinsic Matrices
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // Intrinsic Matrix
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Intrinsic Matrix", EditorStyles.boldLabel, GUILayout.MinWidth(50f));
                EditorGUILayout.EndHorizontal();

                if (!edit)
                {
                    GUI.enabled = false;
                    SetMatrixLayout(projectorScript.intMat);
                    GUI.enabled = true;
                }
                else
                {
                    projectorScript.intMat = SetMatrixLayoutEdit(projectorScript.intMat);
                }

                // Extrinsic Matrix
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Extrinsic Matrix", EditorStyles.boldLabel, GUILayout.MinWidth(50f));
                EditorGUILayout.EndHorizontal();

                if (!edit)
                {
                    GUI.enabled = false;
                    SetMatrixLayout(projectorScript.extMat);
                    GUI.enabled = true;
                }
                else
                {
                    projectorScript.extMat = SetMatrixLayoutEdit(projectorScript.extMat);
                }

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

            /// <summary>
            /// Set up and layout the 4x4 matrix used by the SARProjector.  
            /// </summary>
            /// <param name="mat">The matrix to populate the GUI matrix</param>
            void SetMatrixLayout(Matrix4x4 mat)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("00", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m00, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("01", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m01, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("02", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m02, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("03", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m03, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("10", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m10, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("11", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m11, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("12", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m12, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("13", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m13, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("20", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m20, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("21", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m21, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("22", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m22, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("23", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m23, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("30", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m30, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("31", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m31, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("32", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m32, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("33", GUILayout.Width(20f));
                EditorGUILayout.FloatField(mat.m33, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
            }

            /// <summary>
            /// Set up and layout the 4x4 matrix used by the SARProjector.  
            /// </summary>
            /// <param name="mat">The matrix to populate the GUI matrix</param>
            /// <returns>The updated matrix from UI input</returns>
            Matrix4x4 SetMatrixLayoutEdit(Matrix4x4 mat)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("00", GUILayout.Width(20f));
                mat.m00 = EditorGUILayout.FloatField(mat.m00, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("01", GUILayout.Width(20f));
                mat.m01 = EditorGUILayout.FloatField(mat.m01, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("02", GUILayout.Width(20f));
                mat.m02 = EditorGUILayout.FloatField(mat.m02, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("03", GUILayout.Width(20f));
                mat.m03 = EditorGUILayout.FloatField(mat.m03, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("10", GUILayout.Width(20f));
                mat.m10 = EditorGUILayout.FloatField(mat.m10, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("11", GUILayout.Width(20f));
                mat.m11 = EditorGUILayout.FloatField(mat.m11, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("12", GUILayout.Width(20f));
                mat.m12 = EditorGUILayout.FloatField(mat.m12, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("13", GUILayout.Width(20f));
                mat.m13 = EditorGUILayout.FloatField(mat.m13, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("20", GUILayout.Width(20f));
                mat.m20 = EditorGUILayout.FloatField(mat.m20, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("21", GUILayout.Width(20f));
                mat.m21 = EditorGUILayout.FloatField(mat.m21, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("22", GUILayout.Width(20f));
                mat.m22 = EditorGUILayout.FloatField(mat.m22, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("23", GUILayout.Width(20f));
                mat.m23 = EditorGUILayout.FloatField(mat.m23, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("30", GUILayout.Width(20f));
                mat.m30 = EditorGUILayout.FloatField(mat.m30, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("31", GUILayout.Width(20f));
                mat.m31 = EditorGUILayout.FloatField(mat.m31, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("32", GUILayout.Width(20f));
                mat.m32 = EditorGUILayout.FloatField(mat.m32, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("33", GUILayout.Width(20f));
                mat.m33 = EditorGUILayout.FloatField(mat.m33, GUILayout.Width(80f));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                return mat;
            }

            /// <summary>
            /// Set up and layout the attached camera parameters which are overloaded for use by the SARProjector.  
            /// </summary>
            void SetCameraParameters()
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Override Attached Camera Component Properties", EditorStyles.boldLabel, GUILayout.MinWidth(100f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Clear Flags", GUILayout.MinWidth(100f));
                cameraScript.clearFlags = (CameraClearFlags)EditorGUILayout.EnumPopup(cameraScript.clearFlags, GUILayout.MinWidth(250f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Background", GUILayout.MinWidth(100f));
                cameraScript.backgroundColor = EditorGUILayout.ColorField(cameraScript.backgroundColor, GUILayout.MinWidth(250f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Culling Mask", GUILayout.MinWidth(100f));
                cameraScript.cullingMask = EditorGUILayout.MaskField(cameraScript.cullingMask, UnityEditorInternal.InternalEditorUtility.layers, GUILayout.MinWidth(250f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Depth", GUILayout.MinWidth(100f));
                cameraScript.depth = EditorGUILayout.FloatField(cameraScript.depth, GUILayout.MinWidth(250f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rendering Path", GUILayout.MinWidth(100f));
                cameraScript.renderingPath = (RenderingPath)EditorGUILayout.EnumPopup(cameraScript.renderingPath, GUILayout.MinWidth(250f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target Texture", GUILayout.MinWidth(100f));
                cameraScript.targetTexture = (RenderTexture)EditorGUILayout.ObjectField(cameraScript.targetTexture, typeof(RenderTexture), true, GUILayout.MinWidth(250f));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target Display", GUILayout.MinWidth(100f));
                cameraScript.targetDisplay = EditorGUILayout.IntPopup(cameraScript.targetDisplay, cameraDisplayStrings, cameraDisplayValues, GUILayout.MinWidth(250f));
                // = target + 1;
                EditorGUILayout.EndHorizontal();
            }
        }
        #endif
    }
}
