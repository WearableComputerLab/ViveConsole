using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using DataLib;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoBuf;
using System;
using System.Runtime.InteropServices;

namespace UnityImporter
{
    public class ModelImport : MonoBehaviour
    {

        /**
		File name that we want aiImporter to generate for us
		We will look for this file and load it once it exists
		It is "scene.bin" by default, change it if you want to load multiple files and want to distinguish them
		**/
        public string lookFor = "scene.bin";

        private GameScene unityScene = new GameScene();

        private GameObject mainObject;

        // Use this for initialization
        void Start()
        {

        }

        void OnGUI()
        {
            if (GUILayout.Button("Load", GUILayout.Width(80)))
            {
                //Path to aiImporter executable
                string exePath = Application.dataPath + "/aiImporter/Importer/AssimpImporterFull.exe";

                //Path to model that we want to load
                //Setting it to "FileDialog" will make the aiImporter open a file browser to choose the file
				string filePath = "FileDialog";

                //You can give a file path without opening the file dialog
                //string filePath = "C:\\Users\\User\\Desktop\\test.fbx";

                //Run with the cmd window
                Process aiImporter = Process.Start(exePath, lookFor + " " + "\"" + filePath + "\"");

                /*
				//Use this if you don't want to see the cmd window. Process will run in the background.
				Process process = new Process();
				process.StartInfo.FileName = exePath;
				process.StartInfo.Arguments = lookFor + " " + "\"" + filePath + "\"";           
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.WorkingDirectory = Application.dataPath + "/aiImporter/Importer/";
				process.StartInfo.UseShellExecute=false;
				process.Start();
				*/

                //Wait for aiImporter to finish its job
                aiImporter.WaitForExit();

                if (aiImporter.ExitCode == 0)
                    LoadFile();
            }
        }


        void LoadFile()
        {
            //Convert file to GameScene Object
            using (var file = File.OpenRead(Application.dataPath + "/aiImporter/Importer/" + lookFor))
            {
                unityScene = Serializer.Deserialize<GameScene>(file);
            }

            //Don't need the file anymore. Can delete it.
            File.Delete(Application.dataPath + "/aiImporter/Importer/" + lookFor);

            //Procedural mesh creation
            CreateScene();

            CancelInvoke();
        }
			

        void CreateScene()
        {
            bool skinned = false;

            string modelName = getModelName(unityScene.filepath);

            mainObject = new GameObject(modelName);

            //Root Node
			Model rootNode = unityScene.rootNode;

			GameObject root = new GameObject(rootNode.name);
			root.transform.parent = mainObject.transform;
			MatrixUtils.SetTransformMatrix(root.transform, rootNode.transformMatrix);

            Animation anim = null;

            //Decide if model is skinned
            if (unityScene.allBones.Count > 0)
                skinned = true;

            Transform[] allbones = new Transform[unityScene.allBones.Count];

            //Skip bone creation if not skinned
            if (skinned)
            {
                anim = mainObject.AddComponent<Animation>();

                //Just create all bones in the scene
                int bonIndex = 0;
                foreach (BoneData bone in unityScene.allBones)
                {
                    allbones[bonIndex] = new GameObject(bone.BoneName).transform;
                    bonIndex++;
                }

                //Now set bone hierarchy and position/rotation values
                bonIndex = 0;
                foreach (BoneData bone in unityScene.allBones)
                {
                    if (bonIndex == 0)
						allbones[bonIndex].parent = root != null ? root.transform : mainObject.transform;
                    else if (bone.ParentName != "")
                        allbones[bonIndex].parent = FindTransform(mainObject.transform, bone.ParentName);

                    //Set transformation matrix
                    MatrixUtils.SetTransformMatrix(allbones[bonIndex].transform, bone.transformMatrix);

                    bonIndex++;
                }
            }

			CreateObjects(rootNode, root.transform, skinned, true);

            foreach (var animName in unityScene.animNames)
            {
				if(anim == null)
					anim = mainObject.AddComponent<Animation>();
				
                AnimationClip clip = new AnimationClip();

			#if !UNITY_4_5 && !UNITY_4_6
                clip.legacy = true;
			#endif

                foreach (BoneData bone in unityScene.allBones)
                {
                    if (bone.animations == null || !bone.animations.ContainsKey(animName))
                        continue;
					
					AnimationData animData = bone.animations[animName];
					CreateClip (clip, animData, bone.BoneName);
                }

				foreach (Model model in unityScene.allModels)
				{
					if (model.animations == null || !model.animations.ContainsKey(animName))
						continue;

					AnimationData animData = model.animations[animName];
					CreateClip (clip, animData, model.name);
				}
					
                clip.EnsureQuaternionContinuity();
                clip.wrapMode = WrapMode.Loop;

                anim.wrapMode = WrapMode.Loop;
                anim.AddClip(clip, animName);
            }



            if (unityScene.animNames != null && unityScene.animNames.Count > 0)
            {
                anim.clip = anim.GetClip(unityScene.animNames[0]);
                anim.Play(unityScene.animNames[0]);
            }

        }

		private void CreateClip(AnimationClip clip, AnimationData animData, string nodeName)
		{
			int i = 0;

			//Scale Curve
			AnimationCurve scaCurveX = new AnimationCurve();
			AnimationCurve scaCurveY = new AnimationCurve();
			AnimationCurve scaCurveZ = new AnimationCurve();

			if (animData.scaleValues != null)
			{
				for (i = 0; i < animData.AnimTimes.Count; i++)
				{
					Vector3 importScale = animData.scaleValues[i];

					scaCurveX.AddKey(animData.AnimTimes[i], importScale.x);
					scaCurveY.AddKey(animData.AnimTimes[i], importScale.y);
					scaCurveZ.AddKey(animData.AnimTimes[i], importScale.z);
				}
			}

			//Position Curve
			AnimationCurve posCurveX = new AnimationCurve();
			AnimationCurve posCurveY = new AnimationCurve();
			AnimationCurve posCurveZ = new AnimationCurve();

			if (animData.positionValues != null)
			{
				for (i = 0; i < animData.AnimTimes.Count; i++)
				{
					Vector3 importTrans = animData.positionValues[i];

					posCurveX.AddKey(animData.AnimTimes[i], importTrans.x);
					posCurveY.AddKey(animData.AnimTimes[i], importTrans.y);
					posCurveZ.AddKey(animData.AnimTimes[i], importTrans.z);
				}
			}


			AnimationCurve curveX = new AnimationCurve();
			AnimationCurve curveY = new AnimationCurve();
			AnimationCurve curveZ = new AnimationCurve();
			AnimationCurve curveW = new AnimationCurve();

			if (animData.rotationValues != null)
			{
				for (i = 0; i < animData.AnimTimes.Count; i++)
				{
					Quaternion importRot = animData.rotationValues[i];

					curveX.AddKey(animData.AnimTimes[i], importRot.x);
					curveY.AddKey(animData.AnimTimes[i], importRot.y);
					curveZ.AddKey(animData.AnimTimes[i], importRot.z);
					curveW.AddKey(animData.AnimTimes[i], importRot.w);
				}
			}

			string relPath = getRelativePath(nodeName, mainObject.name, mainObject.transform);

			if (animData.rotationValues != null)
			{
				clip.SetCurve(relPath, typeof(Transform), "m_LocalRotation.x", curveX);
				clip.SetCurve(relPath, typeof(Transform), "m_LocalRotation.y", curveY);
				clip.SetCurve(relPath, typeof(Transform), "m_LocalRotation.z", curveZ);
				clip.SetCurve(relPath, typeof(Transform), "m_LocalRotation.w", curveW);
			}

			if (animData.positionValues != null)
			{
				clip.SetCurve(relPath, typeof(Transform), "m_LocalPosition.x", posCurveX);
				clip.SetCurve(relPath, typeof(Transform), "m_LocalPosition.y", posCurveY);
				clip.SetCurve(relPath, typeof(Transform), "m_LocalPosition.z", posCurveZ);
			}

			if (animData.scaleValues != null)
			{
				clip.SetCurve(relPath, typeof(Transform), "localScale.x", scaCurveX);
				clip.SetCurve(relPath, typeof(Transform), "localScale.y", scaCurveY);
				clip.SetCurve(relPath, typeof(Transform), "localScale.z", scaCurveZ);
			}
		}

		void CreateObjects(Model model, Transform parentT, bool skinned, bool root)
		{
			GameObject temp = mainObject;

			if (!root) 
			{
				//Create nodes
				temp = new GameObject (model.name);

				if (parentT == null)
					temp.transform.parent = mainObject.transform;
				else
					temp.transform.parent = parentT;

				MatrixUtils.SetTransformMatrix (temp.transform, model.transformMatrix);
			} else
				temp = parentT.gameObject;


			foreach (Geometry gMesh in model.geometry)
			{
				//Rest is procedural mesh creation. Reference : http://docs.unity3d.com/ScriptReference/Mesh.html
				GameObject meshHolder = new GameObject("Mesh");

				meshHolder.transform.parent = temp.transform;
				meshHolder.transform.localPosition = new Vector3(0f, 0f, 0f);
				meshHolder.transform.localScale = new Vector3(1f, 1f, 1f);
				meshHolder.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);


				//We need different components for skinned and non-skinned meshes
				if (skinned)
					meshHolder.AddComponent<SkinnedMeshRenderer>();
				else
				{
					meshHolder.AddComponent<MeshRenderer>();
					meshHolder.AddComponent<MeshFilter>();
				}

				//Set required mesh data
				Mesh mesh = new Mesh();

				mesh.vertices = gMesh.vertices != null ? gMesh.vertices.ConvertAll(item => (Vector3)item).ToArray() : null;

				#if UNITY_5
				switch (gMesh.uvCount) 
				{
				case 4:
					mesh.uv4 = gMesh.uv4.ConvertAll(item => (Vector2)item).ToArray();
					goto case 3;
				case 3:
					mesh.uv3 = gMesh.uv3.ConvertAll(item => (Vector2)item).ToArray();
					goto case 2;
				case 2:
					mesh.uv2 = gMesh.uv2.ConvertAll(item => (Vector2)item).ToArray();
					goto case 1;
				case 1:
					mesh.uv = gMesh.uv != null  ? gMesh.uv.ConvertAll(item => (Vector2)item).ToArray() : null;
					break;
				case 0:
					break;
				default:
					goto case 4;
				}
				#else
				switch (gMesh.uvCount)
				{
				case 4:
				case 3:
					mesh.uv2 = gMesh.uv3.ConvertAll(item => (Vector2)item).ToArray();
					goto case 2;
				case 2:
					mesh.uv2 = gMesh.uv2.ConvertAll(item => (Vector2)item).ToArray();
					goto case 1;
				case 1:
					mesh.uv = gMesh.uv != null ? gMesh.uv.ConvertAll(item => (Vector2)item).ToArray() : null;
					break;
				case 0:
					break;
				default:
					goto case 3;
				}
				#endif

				mesh.triangles = gMesh.triangles;

				//Load vertex colors
				//You may want to use one of these shaders for vertex colors
				//http://wiki.unity3d.com/index.php/VertexColorUnlit

				if (gMesh.vertexColors != null && gMesh.vertexColors.Count > 0)
				{
					Color[] colors = new Color[mesh.vertices.Length];
					int colorIndex = 0;
					while (colorIndex < mesh.vertices.Length)
					{
						Vector4 colorArray = gMesh.vertexColors[colorIndex];
						colors[colorIndex] = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
						colorIndex++;
					}
					mesh.colors = colors;
				}

				mesh.normals = gMesh.normals != null ? gMesh.normals.ConvertAll(item => (Vector3)item).ToArray() : null;

				// Using the normal calculation function of Unity is another option
				// mesh.RecalculateNormals();

				mesh.tangents = gMesh.tangents != null ? gMesh.tangents.ConvertAll(item => (Vector4)item).ToArray() : null;

				//Start with Materials
				MaterialData material = unityScene.materials[gMesh.MaterialIndex];

				//Cast renderers to base class so that we can set renderer materials with the same code
				Renderer renderer = skinned ? (Renderer)meshHolder.GetComponent<SkinnedMeshRenderer>() : (Renderer)meshHolder.GetComponent<MeshRenderer>();

				//Decide our shader based on material mapping
				if (material.HasTextureNormal)
				{
					if (material.HasTransparency || material.HasTransTexture)
						renderer.material.shader = Shader.Find("Transparent/Bumped Diffuse");
					else
						renderer.material.shader = Shader.Find("Bumped Diffuse");
				}
				else if (material.HasTransparency || material.HasTransTexture)
					renderer.material.shader = Shader.Find("Transparent/Diffuse");
				else
					renderer.material.shader = Shader.Find("Diffuse");


				//Set material texture and color if we have those values
				if (material.HasTextureDiffuse)
				{
					Texture2D texture = new Texture2D(material.diffuseWidth, material.diffuseHeight, TextureFormat.DXT1, false);
					texture.LoadImage(material.DiffuseTexture);
					renderer.material.mainTexture = texture;
				}

				if (material.HasColorDiffuse)
					renderer.material.SetVector("_Color", material.DiffuseColor);

				// Only set alpha color is there is no transparency texture.
				if (material.HasTransparency && !material.HasTransTexture)
				{
					Color matColor = renderer.material.color;
					matColor.a = material.AlphaValue;

					renderer.material.color = matColor;
				}

				//Set normal map if we have it
				if (material.HasTextureNormal)
				{
					Texture2D loadedTexture = new Texture2D(material.normalWidth, material.normalHeight, TextureFormat.RGB24, false);
					loadedTexture.LoadImage(material.NormalTexture);

					renderer.material.SetTexture("_BumpMap", NormalMapToUnity(loadedTexture));
				}

				if (skinned)
				{
					//Load bone weights for each vertex
					if (gMesh.weights != null)
					{
						BoneWeight[] weights = new BoneWeight[mesh.vertexCount];

						for (int g = 0; g < mesh.vertexCount; g++)
						{
							if (g >= gMesh.weights.Count)
								break;
							
							List<WeightData> vertexWeight = gMesh.weights[g].weightList;

							for (int i = 0; i < vertexWeight.Count; i++)
							{
								if (i > 3)
									break;

								switch (i)
								{
								case 0:
									weights[g].boneIndex0 = vertexWeight[i].BoneID;
									weights[g].weight0 = vertexWeight[i].Weight;
									break;
								case 1:
									weights[g].boneIndex1 = vertexWeight[i].BoneID;
									weights[g].weight1 = vertexWeight[i].Weight;
									break;
								case 2:
									weights[g].boneIndex2 = vertexWeight[i].BoneID;
									weights[g].weight2 = vertexWeight[i].Weight;
									break;
								case 3:
									weights[g].boneIndex3 = vertexWeight[i].BoneID;
									weights[g].weight3 = vertexWeight[i].Weight;
									break;
								default:
									break;
								}
							}
						}

						mesh.boneWeights = weights;
					}

					if (gMesh.bones != null)
					{
						//Set bindposes
						Transform[] bones = new Transform[gMesh.bones.Count];
						Matrix4x4[] bindPoses = new Matrix4x4[gMesh.bones.Count];
						int boneIndex = 0;
						foreach (BoneData bone in gMesh.bones)
						{
							bones[boneIndex] = FindTransform(mainObject.transform, bone.BoneName);

							bindPoses[boneIndex] = bone.transformMatrix;

							boneIndex++;
						}

						mesh.bindposes = bindPoses;

						//Pass required skinning information to our SkinnedMeshRenderer
						((SkinnedMeshRenderer)renderer).bones = bones;
					}
				}

				//Finally set the mesh to make it visible
				if (skinned)
					((SkinnedMeshRenderer)renderer).sharedMesh = mesh;
				else
					meshHolder.GetComponent<MeshFilter>().mesh = mesh;
			}


			foreach (Model child in model.children)
				CreateObjects(child, temp.transform, skinned, false);
		}

        #region HelperFunctions
        public Texture2D NormalMapToUnity(Texture2D loadedTexture)
        {
            Texture2D normalTexture = new Texture2D(loadedTexture.width, loadedTexture.height, TextureFormat.ARGB32, false);
            Color theColour = new Color();
            for (int x = 0; x < loadedTexture.width; x++)
            {
                for (int y = 0; y < loadedTexture.height; y++)
                {
                    theColour.r = loadedTexture.GetPixel(x, y).g;
                    theColour.g = theColour.r;
                    theColour.b = theColour.r;
                    theColour.a = loadedTexture.GetPixel(x, y).r;
                    normalTexture.SetPixel(x, y, theColour);
                }
            }
            normalTexture.Apply();

            return normalTexture;
        }

        string getModelName(string path)
        {
            path = path.Replace("\\", "/");

            string temp = path.Substring(path.LastIndexOf("/") + 1);
            temp = temp.Substring(0, temp.LastIndexOf("."));

            return temp;
        }

        public static Transform FindTransform(Transform parent, string name)
        {
            if (parent.name.Equals(name)) return parent;
            foreach (Transform child in parent)
            {
                Transform result = FindTransform(child, name);
                if (result != null) return result;
            }
            return null;
        }

        string getRelativePath(string boneName, string stop, Transform root)
        {
            string path = "";
            Transform bone = FindTransform(root, boneName);

			while (bone != root)
            {
                if (path == "")
                    path += bone.name;
                else
                    path = bone.name + "/" + path;

                bone = bone.parent;
            }

            return path;
        }

        #endregion
    }
}
