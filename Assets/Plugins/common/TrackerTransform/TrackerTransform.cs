/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2018 Daniel Jackson.
*               2017 Andrew Irlitti.
*               2015 Michael Marner.
*               2015 Markus Broecker.
*               2015 Benjamin Close.
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
    /// A tracker transform updates the translation and rotation properties of the attached game object from data collected from an outside source through VRPN.
    /// </summary>
    public class TrackerTransform : MonoBehaviour
    {
        // Runtime information for connecting to, and processing VRPN data.
        // --Modifiable by external scripts:
        [SerializeField]
        public string trackerId = "";
        [SerializeField]
        public TrackerBoss trackerBoss = null;
        [SerializeField]
        public bool ipFromBoss = false;
        [SerializeField]
        public string trackerIp = "";
        [SerializeField]
        public bool cstFromBoss = false;
        // --Must be set via SetCstMatrix and SetOffsetMatrix:
        [SerializeField]
        Vector3 offsetTranslate = Vector3.zero;
        [SerializeField]
        Quaternion offsetRotation = Quaternion.identity;
        [SerializeField]
        Matrix4x4 cstTransformMatrix = Matrix4x4.identity;
        [SerializeField]
        float cstScaleFactor = 1;

        // For automatic loading of data from predefined matricies.
        public enum RelativePathBase
        {
            Absolute,
            ProjectAssets,
            TrackerBoss
        };
        [SerializeField]
        bool cstFromFile = false;
        [SerializeField]
        RelativePathBase cstPathBase = RelativePathBase.Absolute;
        [SerializeField]
        string cstPath = "";
        [SerializeField]
        bool offsetFromFile = false;
        [SerializeField]
        RelativePathBase offsetPathBase = RelativePathBase.Absolute;
        [SerializeField]
        string offsetPath = "";

#if UNITY_EDITOR
        // Properties not valid at run time.
        [SerializeField]
        bool updateInEditor = false;
        [SerializeField]
        bool useBoss = false;
        [SerializeField]
        Matrix4x4 cstMatrix = Matrix4x4.identity;
        [SerializeField]
        Matrix4x4 offsetMatrix = Matrix4x4.identity;
#endif

        private TrackerBoss sceneBoss;

        [System.Serializable]
        public enum Axis { X, Y, Z };
        [SerializeField]
        private Axis axis = Axis.X;

        private void Start()
        {
            // Load
            var boss = GetTrackerBoss();
            if (cstFromFile)
            {
                var basePath = "";
                if (cstPathBase == RelativePathBase.ProjectAssets)
                {
                    basePath = Application.dataPath;
                }
                else if (cstPathBase == RelativePathBase.TrackerBoss)
                {
                    if(boss == null)
                    {
                        throw new System.Exception("No TrackerBoss in scene.");
                    }
                    basePath = boss.MatrixDirectory;
                }
                var path = System.IO.Path.Combine(basePath, cstPath);
                var cstMatrix = MatrixIO.ReadMatrixFromFile(path);
                SetCstMatrix(cstMatrix);
            }
            if (offsetFromFile)
            {
                var basePath = "";
                if (offsetPathBase == RelativePathBase.ProjectAssets)
                {
                    basePath = Application.dataPath;
                }
                else if (offsetPathBase == RelativePathBase.TrackerBoss)
                {
                    if (boss == null)
                    {
                        throw new System.Exception("No TrackerBoss in scene.");
                    }
                    basePath = boss.MatrixDirectory;
                }
                var path = System.IO.Path.Combine(basePath, offsetPath);
                var offsetMatrix = MatrixIO.ReadMatrixFromFile(path);
                SetOffsetMatrix(offsetMatrix);
            }
        }

        private void Update()
        {
            var ip = trackerIp;
            var cstMatrix = cstTransformMatrix;
            var cstScale = cstScaleFactor;
            if (ipFromBoss || cstFromBoss) // If boss is in use
            {
                var boss = GetTrackerBoss();
                if (boss == null)
                {
                    // No boss in scene so avoid checking every frame
                    enabled = false;
                    throw new System.Exception("TrackerTransform set to derive from TrackerBoss, but no TrackerBoss in scene. TrackerTransform will be disabled.");
                }
                if (ipFromBoss)
                {
                    ip = boss.trackerIp;
                }
                if (cstFromBoss)
                {
                    cstMatrix = boss.CstTransformMatrix;
                    cstScale = boss.CstScaleFactor;
                }
            }

            // Get tracker pose from VRPN.
            var vrpnRot = VRPN_Handler.vrpnTrackerQuat(trackerId + "@" + ip, 0);
            var vrpnPos = VRPN_Handler.vrpnTrackerPos(trackerId + "@" + ip, 0);
            // Convert from right to left handed coordinate base.
            Quaternion lhRot = LHQuatFromRHQuat(vrpnRot, axis);
            Vector3 lhPos = LHVectorFromRHVector(vrpnPos, axis);
            // Multiply by the CST matrix to transform from tracker worldspace to app worldspace.
            var cstRot = MatrixIO.QuaternionFromMatrix(cstMatrix) * lhRot;
            var cstPos = cstMatrix.MultiplyPoint3x4(lhPos);
            cstPos *= cstScale;
            // Apply offset matrix.
            var trackerTransform = Matrix4x4.TRS(cstPos, cstRot, Vector3.one);
            trackerTransform = trackerTransform * Matrix4x4.TRS(offsetTranslate, offsetRotation, Vector3.one);
            // Decompose and apply to GameObject transform.
            transform.position = trackerTransform.GetColumn(3);
            transform.rotation = MatrixIO.QuaternionFromMatrix(trackerTransform);
        }

        private Quaternion LHQuatFromRHQuat(Quaternion rh, Axis axis = Axis.X)
        {
            Vector3 angles = rh.eulerAngles;
            if (axis == Axis.X)
            {
                return Quaternion.Euler(new Vector3(angles.x, -angles.y, -angles.z));
            }
            if (axis == Axis.Y)
            {
                return Quaternion.Euler(new Vector3(-angles.x, angles.y, -angles.z));
            }
            else
            {
                return Quaternion.Euler(new Vector3(-angles.x, -angles.y, angles.z));
            }
        }

        private Vector3 LHVectorFromRHVector(Vector3 rh, Axis axis = Axis.X)
        {
            if (axis == Axis.X)
            {
                return new Vector3(-rh.x, rh.y, rh.z);
            }
            if (axis == Axis.Y)
            {
                return new Vector3(rh.x, -rh.y, rh.z);
            }
            else
            {
                return new Vector3(rh.x, rh.y, -rh.z);
            }
        }

        private TrackerBoss GetTrackerBoss()
        {
            // If boss is not set, try to find a Boss in the scene, if that doesn't work return null.
            if (trackerBoss == null)
            {
                if (sceneBoss == null)
                {
                    sceneBoss = FindObjectOfType<TrackerBoss>();
                }
                return sceneBoss;
            }
            return trackerBoss;
        }

        /// <summary>
        /// Set the associated Coorindate Space Transform matrix to be used with this tracker transform
        /// </summary>
        /// <param name="cstMat">The precalculated CoordinateSpaceTransform matrix</param>
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

        public void SetOffsetMatrix(Matrix4x4 offsetMatrix)
        {
            var temp = offsetMatrix;
            offsetTranslate = offsetMatrix.GetColumn(3);
            temp.SetColumn(3, Vector4.zero);
            offsetRotation = MatrixIO.QuaternionFromMatrix(temp);
        }
    }
}
