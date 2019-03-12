/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
*               2018 Daniel Jackson
*
* If you use/extend/modify this code, add your name and email address
* to the AUTHORS file in the root directory.
*
* This code can be used by staff and students of the University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission.
*
*/

using UnityEngine;
using UnitySARCommon.IO;

namespace UnitySARCommon.Tracking
{
    /// <summary>
    /// A tracker management class, supporting tracker solution IP address and the CST location.
    /// </summary>
    public class TrackerBoss : MonoBehaviour
    {
        // Runtime information for connecting to, and processing VRPN data.
        // --Modifiable by external scripts:
        [SerializeField]
        public string trackerIp = "";
        [SerializeField]
        string matrixDirectory = "";
        public string MatrixDirectory { get { return matrixDirectory; } }
        // --Must be set via SetCstMatrix and SetOffsetMatrix:
        [SerializeField]
        Matrix4x4 cstTransformMatrix = Matrix4x4.identity;
        public Matrix4x4 CstTransformMatrix { get { return cstTransformMatrix; } }
        [SerializeField]
        float cstScaleFactor = 1;
        public float CstScaleFactor { get { return cstScaleFactor; } }

        // For automatic loading of data from predefined matricies.
        [SerializeField]
        bool cstFromFile = false;
        [SerializeField]
        TrackerTransform.RelativePathBase cstPathBase = TrackerTransform.RelativePathBase.Absolute;
        [SerializeField]
        string cstPath = "";

#if UNITY_EDITOR
        [SerializeField]
        public Matrix4x4 cstMatrix;
#endif

        private void Start()
        {
            // Load
            if (cstFromFile)
            {
                var basePath = "";
                if (cstPathBase == TrackerTransform.RelativePathBase.ProjectAssets)
                {
                    basePath = Application.dataPath;
                }
                else if (cstPathBase == TrackerTransform.RelativePathBase.TrackerBoss)
                {
                    basePath = MatrixDirectory;
                }
                var path = System.IO.Path.Combine(basePath, cstPath);
                var cstMatrix = MatrixIO.ReadMatrixFromFile(path);
                SetCstMatrix(cstMatrix);
            }
        }

        public void SetCstMatrix(Matrix4x4 cstMatrix)
        {
            cstTransformMatrix = cstMatrix;
            cstScaleFactor = ((Vector3)cstMatrix.GetColumn(0)).magnitude;
            var translation = cstMatrix.GetRow(3);
            for (int i = 0; i < 16; i++)
            {
                cstTransformMatrix[i] /= cstScaleFactor;
            }
            cstTransformMatrix.SetRow(3, translation);
        }
    }
}
