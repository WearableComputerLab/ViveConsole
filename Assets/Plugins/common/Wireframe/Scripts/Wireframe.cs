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
using System.Collections.Generic;

using Exception = System.Exception;

namespace UnitySARCommon.Geometry
{

    /// <summary>
    /// Behaviour script, when applied to a GameObject with a MeshFilter and MeshRenderer it allows them to be rendered using a shader based wireframe mode.
    /// </summary>
    public class Wireframe : MonoBehaviour
    {
        // Reference to the Wireframe Material. This should be a Material using the WireShader Shader.
        public Material wireMaterial;

        private bool wireEnabled = false; // True if object is rendering in wireframe mode, else false.
        private Material[] materialCache; // Array of Materials. When applying wireframe Material to MeshRenderer, the existing list of Materials is cached here and restored when wireframe is disabled again.

        public void Start()
        {
            // When first added, prepare the mesh.
            UpdateMesh();
        }

        /// <summary>
        /// Turn on wireframe rendering. This switches all Materials in this GameObject's MeshRenderer to the Wireframe Material.
        /// </summary>
        public void EnableWire()
        {
            // Get the MeshRenderer component. Throw Exception if there is no MeshRenderer.
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null)
            {
                throw new Exception("GameObject has no MeshRender.");
            }
            if (!wireEnabled)
            {
                List<Material> materialList = new List<Material>();
                // Create a new Material list, replacing each Material from the Mesh with the Wireframe Material.
                for (int i = 0; i < mr.sharedMaterials.Length; i++)
                {
                    materialList.Add(wireMaterial);
                }
                // Cache the original Materials.
                materialCache = mr.sharedMaterials;
                mr.sharedMaterials = materialList.ToArray();
                wireEnabled = true;
            }
        }

        public void DisableWire()
        {
            // Get the MeshRenderer component. Throw Exception if there is no MeshRenderer.
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null)
            {
                throw new Exception("GameObject has no MeshRender.");
            }
            if (wireEnabled)
            {
                // Restore original Materials.
                mr.sharedMaterials = materialCache;
                wireEnabled = false;
            }
        }

        /// <summary>
        /// Process the Mesh assigned to this GameObjects MeshFilter component to allow it to render using the Wireframe Shader.
        /// This should be called every time there is a change made to the Mesh attribute of the MeshFilter attached to the Wireframe's GameObject.
        /// </summary>
        public void UpdateMesh()
        {
            // Get the MeshFilter component. Throw Exception if there is no MeshFilter.
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                throw new Exception("GameObject has no MeshFilter.");
            }
            // Ensure all verticies are referenced just once in the index buffer. The Wireframe Shader requires the color channel to be populated with unique data for every corner of every tri in the Mesh.
            Mesh processedMesh = MakeVertsUnique(mf.mesh);
            // Populates the Meshes Color channel with the data needed to determine barycentric coordinates of all corners of all tris.
            AddBarycenterChannel(ref processedMesh);
            // Do any post-processing required by the engine when a Mesh is altered programmatically.
            Finalize(ref processedMesh);
            // Assign the processed Mesh.
            mf.mesh = processedMesh;
            // Meshes should start with their original Materials.
            wireEnabled = false;
        }

        private static Mesh MakeVertsUnique(Mesh m)
        {
            // Returns a Mesh with the same geometry as the given Mesh, 
            //	but with no shared vertex data.
            Mesh result = new Mesh();
            int triCount = m.triangles.Length;

            int absIdx = 0;

            // Initialize temporary buffers.
            Vector3[] dstPosVerts = new Vector3[triCount];
            Vector3[] dstNormVerts = new Vector3[triCount];
            Vector2[] dstUvVerts = new Vector2[triCount];

            result.vertices = dstPosVerts;
            result.normals = dstNormVerts;
            result.uv = dstUvVerts;

            // For performance reasons, cache source buffers.
            Vector3[] srcPosVerts = m.vertices;
            Vector3[] srcNormVerts = m.normals;
            Vector2[] srcUvVerts = m.uv;

            // Set the new meshes submesh count.
            result.subMeshCount = m.subMeshCount;

            for (int subIdx = 0; subIdx < m.subMeshCount; subIdx++)
            {
                // For each source sub-mesh, create a new index buffer, and copy the contents of the vertex buffers.
                // Create a new index buffer, of the same size as the old buffer.
                int[] dstTris = new int[m.GetTriangles(subIdx).Length];
                int[] srcTris = m.GetTriangles(subIdx);
                for (int subTriIdx = 0; subTriIdx < srcTris.Length; subTriIdx++)
                {
                    // For each index in the old submesh index buffer, copy the vertex data from 
                    //	that index in the old vertex buffers into the new vertex buffers, then 
                    //	set the index in the new index buffer to point to the data in the new vertex buffers.
                    // Get old index for triangle point.
                    int tri = srcTris[subTriIdx];
                    // Copy old values into new vert buffers.
                    dstPosVerts[absIdx] = srcPosVerts[tri];
                    if (srcNormVerts.Length != 0)
                    {
                        dstNormVerts[absIdx] = srcNormVerts[tri];
                    }
                    if (srcUvVerts.Length != 0)
                    {
                        dstUvVerts[absIdx] = srcUvVerts[tri];
                    }
                    // Set new index for this point to the index into the new vert buffers.
                    dstTris[subTriIdx] = absIdx;
                    // Increment index into new vert buffers.
                    absIdx++;
                }
                // Set new triangles for the submesh.
                result.SetTriangles(dstTris, subIdx);
            }

            // Copy new buffers into new mesh.
            result.vertices = dstPosVerts;
            result.normals = dstNormVerts;
            result.uv = dstUvVerts;

            return result;
        }

        private static void AddBarycenterChannel(ref Mesh mesh)
        {
            // Create a new buffer for colors.
            Color[] colors = new Color[mesh.triangles.Length];

            int faceIndex = 0; // Current index's position in its triangle.
            foreach (int i in mesh.triangles)
            {
                // For each index in the index buffer, add data to the color 
                // buffer, each color channel representing one component of 
                // the barycentric coordinate of the point. Interpolating in 
                // rasterization gives the barycentric coordinates of the fragment.
                switch (faceIndex)
                {
                    case 0:
                        colors[i] = Color.red;
                        break;
                    case 1:
                        colors[i] = Color.green;
                        break;
                    case 2:
                        colors[i] = Color.blue;
                        break;
                }
                // Wrap index relative to triangle 0-2
                faceIndex = (faceIndex + 1) % 3;
            }
            mesh.colors = colors; // Add to Mesh.
        }

        private static void Finalize(ref Mesh mesh)
        {
            // Finalize a changed Mesh by re-generating intermediate 
            //	data and re-optimizing.
            mesh.RecalculateBounds();
        }
    }
}