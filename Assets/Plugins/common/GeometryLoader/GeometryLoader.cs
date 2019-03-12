/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
*               2017 Daniel Jackson.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using DataLib;

using Exception = System.Exception;
using Process = System.Diagnostics.Process;
using Debug = UnityEngine.Debug;

namespace UnitySARCommon.Geometry
{

    /// <summary>
    /// Uses AiImporter to parse files, then creates a heirachy of GameObjects containing the geometry from the source file. Initialize by adding to a GameObject.
    /// To use GeometryLoader, the executable and dll files in aiImporter/Importer must be somewhere with Application dataPath directory.
    /// </summary>
    public class GeometryLoader : MonoBehaviour
    {
        // Shader to apply to non-transparent materials.
        public Shader opaqueShader;
        // Shader to apply to transparent materials.
        public Shader transparentShader;
        // Material to apply to geometry with no specified material.
        public Material defaultMaterial;

        // Name of the executable parser.
        private const string importerName = "AssimpImporterFull.exe";
        // Path to the parser on the file system.
        private static string importerPath;

        /// <summary>
        /// Run when Component is created. If static data has not been initialize, do it now.
        /// </summary>
        void Awake()
        {
            // Perform first run initialization.
            if (!StaticIsInitialized())
            {
                try
                {
                    StaticInitialize();
                }
                catch (Exception e)
                {
                    Debug.Log("Error initializing GeometryLoader: " + e.Message);
                    throw new Exception("Failed to initialize GeometryLoader", e);
                }
            }
        }

        /// <summary>
        /// Load geometry data from an OBJ, DAE or FBX file into the scene. If the file type supports heirachies then the hierachy will be maintained with the given GameObject as the root.
        /// </summary>
        /// <param name="path">Path to the file to load. Can be absolute or relative to Application.dataPath.</param>
        /// <param name="parent">Existing game object to act as the root in the geometry heirachy. If null, a new object will be created.</param>
        /// <returns>The root GameObject. The same as parent, unless parent is null.</returns>
        public GameObject LoadGeometry(string path, GameObject parent)
        {
            // If user passes a null parent, create a new object to be the root.
            if (parent == null)
            {
                parent = new GameObject("Loaded Geometry");
            }
            // Parse the file using AiImporter and load the geometry data.
            GameScene data;
            try
            {
                data = LoadData(path);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to load file \"{0}\"", path), e);
            }

            // Iterate through imported material definitions. For each definition, create a Unity Material and add an index, Material pair to a Hashtable for reference.
            Hashtable matLookup = new Hashtable();
            for (int i = 0; i < data.materials.Count; i++)
            {
                MaterialData md = data.materials[i];
                Material mat;
                // Create material using either transparent or opaque shader.
                if (md.HasTransparency)
                {
                    mat = new Material(transparentShader);
                }
                else
                {
                    mat = new Material(opaqueShader);
                }
                mat.name = md.MatName;
                if (md.HasColorDiffuse)
                {
                    mat.color = new Color(md.DiffuseColor.x, md.DiffuseColor.y, md.DiffuseColor.z, md.DiffuseColor.w);
                }
                if (md.HasTextureDiffuse)
                {
                    Texture2D tex = new Texture2D(md.diffuseWidth, md.diffuseHeight);
                    tex.LoadImage(md.DiffuseTexture);
                    mat.mainTexture = tex;
                }
                matLookup.Add(i, mat);
            }
            // Iterate through the geometry heirachy and add each group as a GameObject.
            foreach (Model m in data.allModels)
            {
                AddModel(m, parent.transform, matLookup);
            }
            // Return the root GameObject.
            return parent;
        }

        /// <summary>
        /// Use AiImporter to parse geometry data from an OBJ, DAE or FBX file.
        /// </summary>
        /// <param name="path">Path to the file. Can be absolute, or relative to Application.dataPath.</param>
        /// <returns>Geometry data from file.</returns>
        public GameScene LoadData(string path)
        {
            if (!StaticIsInitialized())
            {
                throw new Exception("GeometryLoad not initialized.");
            }

            // Create absolute paths.
            string sourcePath = Path.Combine(Application.dataPath, path);
            string tempName = "scene.bin";
            string tempPath = Path.Combine(Path.GetDirectoryName(importerPath), tempName);

            // Run with the cmd window.
            Process aiImporter = new Process();
            aiImporter.StartInfo.FileName = importerPath;
            aiImporter.StartInfo.Arguments = string.Format("{0} \"{1}\"", tempName, sourcePath);
            aiImporter.StartInfo.CreateNoWindow = true;
            aiImporter.StartInfo.WorkingDirectory = Application.dataPath;
            aiImporter.StartInfo.UseShellExecute = false;
            aiImporter.Start();

            //Wait for aiImporter to finish its job
            while (!aiImporter.HasExited)
            {
                aiImporter.WaitForExit();
            }

            if (aiImporter.ExitCode == 0)
            {
                GameScene unityScene;
                //Convert file to GameScene Object
                using (FileStream file = File.OpenRead(tempPath))
                {
                    unityScene = Serializer.Deserialize<GameScene>(file);
                }
                //Don't need the file anymore. Can delete it.
                File.Delete(tempPath);
                return unityScene;
            }
            else
            {
                throw new Exception("Failed to parse \"" + path + "\"");
            }
        }

        /// <summary>
        /// Recursivly parse a Model object and add the model and all it's children as children to a given Transform. 
        /// </summary>
        /// <param name="srcModel">The source Model object.</param>
        /// <param name="parent">The Transform to use as a root for the Model.</param>
        /// <param name="materialTable">Hashtable of material indexes to Materials.</param>
        private void AddModel(Model srcModel, Transform parent, Hashtable materialTable)
        {
            GameObject modelObj = new GameObject(srcModel.name);
            // If the model has no name, give it one.
            if (string.IsNullOrEmpty(modelObj.name))
            {
                modelObj.name = "Unnamed Model";
            }
            // Set the parent.
            modelObj.transform.parent = parent;
            modelObj.transform.localPosition = TranslationFromMatrix(srcModel.transformMatrix);
            modelObj.transform.localRotation = RotationFromMatrix(srcModel.transformMatrix);
            modelObj.transform.localScale = ScaleFromMatrix(srcModel.transformMatrix);
            // Iterate over children and to this GameObject.
            foreach (Model child in srcModel.children)
            {
                AddModel(child, modelObj.transform, materialTable);
            }
            // Iterate over geometries and add to GameObject.
            foreach (DataLib.Geometry g in srcModel.geometry)
            {
                AddGeometry(g, modelObj.transform, materialTable);
            }
        }

        /// <summary>
        /// Parse a Geometry and create a GameObject containing a MeshFilter and MeshRender to render the Geometry. The new Geometry will be a child of a given Transform.
        /// </summary>
        /// <param name="srcGeometry">The source Geometry object.</param>
        /// <param name="parent">The Transform which will be the parent of this GameObject.</param>
        /// <param name="materialTable">Hashtable of material indexes to Materials.</param>
        private void AddGeometry(DataLib.Geometry srcGeometry, Transform parent, Hashtable materialTable)
        {
            GameObject geomObj = new GameObject(srcGeometry.Name);
            // If the source Geometry has no name, give it one.
            if (string.IsNullOrEmpty(geomObj.name))
            {
                geomObj.name = "Unnamed Geometry";
            }
            // Parent the new GameObject.
            geomObj.transform.parent = parent;
            // Create the Mesh for the Geometry.
            Mesh mesh = new Mesh();
            // Populate the Mesh's position vertex buffer.
            List<Vector3> vertList = new List<Vector3>();
            if (srcGeometry.vertices != null)
            {
                foreach (Vector3C src in srcGeometry.vertices)
                {
                    vertList.Add(src);
                }
            }
            mesh.vertices = vertList.ToArray();
            // Populate the Mesh's normal vertex buffer.
            List<Vector3> normList = new List<Vector3>();
            if (srcGeometry.normals != null)
            {
                foreach (Vector3C src in srcGeometry.normals)
                {
                    normList.Add(src);
                }
            }
            mesh.normals = normList.ToArray();
            // Populate the Mesh's uv vertex buffer.
            List<Vector2> uvList = new List<Vector2>();
            if (srcGeometry.uv != null)
            {
                foreach (Vector2C src in srcGeometry.uv)
                {
                    uvList.Add(src);
                }
            }
            mesh.uv = uvList.ToArray();
            // Copy the index buffer.
            mesh.triangles = srcGeometry.triangles;
            geomObj.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer mr = geomObj.AddComponent<MeshRenderer>();
            // Apply Material for Geometry. Or default material.
            if (materialTable.ContainsKey(srcGeometry.MaterialIndex))
            {
                mr.material = materialTable[srcGeometry.MaterialIndex] as Material;
            }
            else
            {
                mr.material = defaultMaterial;
            }
        }

        private Vector3 TranslationFromMatrix(Matrix4x4 mat)
        {
            // Last column contains translation.
            return mat.GetColumn(3);
        }

        private Quaternion RotationFromMatrix(Matrix4x4 mat)
        {
            // Create a quaternion using the column 2 as the up vector and column 1 as the forward vector.
            return Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
        }

        private Vector3 ScaleFromMatrix(Matrix4x4 mat)
        {
            // Scale is determined by the magnitude of each column
            Vector3 scale;
            scale.x = mat.GetColumn(0).magnitude;
            scale.y = mat.GetColumn(1).magnitude;
            scale.z = mat.GetColumn(2).magnitude;
            return scale;
        }

        private static void StaticInitialize()
        {
            // Search in all directories below the application data path for the AiImporter parser executable.
            string[] files = Directory.GetFiles(Application.dataPath, importerName, SearchOption.AllDirectories);
            // If nothing is found throw Exception.
            if (files.Length == 0)
            {
                throw new Exception(string.Format("No files found for \"{0}\".", importerName));
            }
            // Set static importerPath.
            importerPath = files[0];
            // If there were multiple matching files log this unexpected behaviour.
            if (files.Length > 1)
            {
                Debug.LogWarning(string.Format("Multiple files found for \"{0}\". Selected \"{1}\".", importerName, importerPath));
            }
        }

        private static bool StaticIsInitialized()
        {
            // Return true if importerPath has been set, indicating the initialization has been completed.
            return !string.IsNullOrEmpty(importerPath);
        }
    }
}