/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
*               2017 Daniel Jackson.
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

namespace UnitySARCommon.Rendering
{
    [ExecuteInEditMode]
    public class OffAxisCamera : MonoBehaviour
    {
        public GameObject imagePlane;
        public bool estimateViewFrustum = true;
        public bool setNearClipPlane = false;
        public float nearClipPlaneOffset = -0.01f;

        private Vector3 imageLowerLeft;
        private Vector3 imageLowerRight;
        private Vector3 imageUpperLeft;

        private Camera eyeCamera;

        void Start()
        {
            eyeCamera = GetComponent<Camera>();
            OnValidate();
        }

        public void SetImagePlane(GameObject go)
        {
            imagePlane = go;
        }


        void OnValidate()
        {
            Vector3 min = imagePlane.GetComponent<MeshFilter>().sharedMesh.bounds.min;
            Vector3 max = imagePlane.GetComponent<MeshFilter>().sharedMesh.bounds.max;

            imageLowerLeft = min;
            imageLowerRight = new Vector3(max.x, min.y, min.z);
            imageUpperLeft = new Vector3(min.x, min.y, max.z);
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (imagePlane != null && eyeCamera != null)
            {
                //// Lower left corner of image plane (world space)
                //Vector3 pa = imagePlane.transform.TransformPoint(new Vector3(-5, 0, -5));
                ////// Lower right
                //Vector3 pb = imagePlane.transform.TransformPoint(new Vector3(5, 0, -5));
                ////// Upper left
                //Vector3 pc = imagePlane.transform.TransformPoint(new Vector3(-5, 0, 5));
                //Lower left corner of image plane (world space)
                Vector3 pa = imagePlane.transform.TransformPoint(imageLowerLeft);
                // Lower right
                Vector3 pb = imagePlane.transform.TransformPoint(imageLowerRight);
                // Upper left
                Vector3 pc = imagePlane.transform.TransformPoint(imageUpperLeft);

                // Eye position
                Vector3 pe = eyeCamera.transform.position;
                // Distance between eye and near clip plane
                float n = eyeCamera.nearClipPlane;
                // Distance between eye and far clip plane
                float f = eyeCamera.farClipPlane;

                Vector3 va = pa - pe; // pe to pa
                Vector3 vb = pb - pe; // pe to pb
                Vector3 vc = pc - pe; // pe to pc
                Vector3 vr = pb - pa; // Screen right axis
                Vector3 vu = pc - pa; // Screen up axis
                Vector3 vn;

                // If the eye is behind the screen reverse the z axis
                if (Vector3.Dot(-Vector3.Cross(va, vc), vb) < 0)
                {
                    vu *= -1;
                    pa = pc;
                    pb = pa + vr;
                    pc = pa + vu;
                    va = pa - pe;
                    vb = pb - pe;
                    vc = pc - pe;
                }

                vr.Normalize();
                vu.Normalize();
                vn = -Vector3.Cross(vr, vu); // Unity coord are left handed, so we negate result.
                vn.Normalize();

                float d = -Vector3.Dot(va, vn); // Distance from eye to screen.
                if (setNearClipPlane)
                {
                    n = d + nearClipPlaneOffset;
                    eyeCamera.nearClipPlane = n;
                }
                float l = Vector3.Dot(vr, va) * n / d; // Distance to left screen edge.
                float r = Vector3.Dot(vr, vb) * n / d; // Distance to right screen edge.
                float b = Vector3.Dot(vu, va) * n / d; // Distance to bottom screen edge.
                float t = Vector3.Dot(vu, vc) * n / d; // Distance to top screen edge.

                Matrix4x4 p = Matrix4x4.identity; // New projection matrix
                p[0, 0] = 2 * n / (r - l);
                p[0, 1] = 0;
                p[0, 2] = (r + l) / (r - l);
                p[0, 3] = 0;

                p[1, 0] = 0;
                p[1, 1] = 2 * n / (t - b);
                p[1, 2] = (t + b) / (t - b);
                p[1, 3] = 0;

                p[2, 0] = 0;
                p[2, 1] = 0;
                p[2, 2] = (f + n) / (n - f);
                p[2, 3] = 2 * f * n / (n - f);

                p[3, 0] = 0;
                p[3, 1] = 0;
                p[3, 2] = -1;
                p[3, 3] = 0;

                Matrix4x4 rm = Matrix4x4.identity; // rotation matrix;
                rm[0, 0] = vr.x;
                rm[0, 1] = vr.y;
                rm[0, 2] = vr.z;
                rm[0, 3] = 0;

                rm[1, 0] = vu.x;
                rm[1, 1] = vu.y;
                rm[1, 2] = vu.z;
                rm[1, 3] = 0;

                rm[2, 0] = vn.x;
                rm[2, 1] = vn.y;
                rm[2, 2] = vn.z;
                rm[2, 3] = 0;

                rm[3, 0] = 0;
                rm[3, 1] = 0;
                rm[3, 2] = 0;
                rm[3, 3] = 1;

                Matrix4x4 tm = Matrix4x4.identity; // translation matrix;
                tm[0, 0] = 1;
                tm[0, 1] = 0;
                tm[0, 2] = 0;
                tm[0, 3] = -pe.x;

                tm[1, 0] = 0;
                tm[1, 1] = 1;
                tm[1, 2] = 0;
                tm[1, 3] = -pe.y;

                tm[2, 0] = 0;
                tm[2, 1] = 0;
                tm[2, 2] = 1;
                tm[2, 3] = -pe.z;

                tm[3, 0] = 0;
                tm[3, 1] = 0;
                tm[3, 2] = 0;
                tm[3, 3] = 1;

                // Set matrices
                eyeCamera.projectionMatrix = p;
                eyeCamera.worldToCameraMatrix = rm * tm;

                if (estimateViewFrustum)
                {
                    // Rotate camera to screen for culling to work
                    Quaternion q = Quaternion.identity;
                    q.SetLookRotation((0.5f * (pb + pc) - pe), vu);
                    eyeCamera.transform.rotation = q;

                    // set fieldOfView 
                }

            }
        }
    }
}
