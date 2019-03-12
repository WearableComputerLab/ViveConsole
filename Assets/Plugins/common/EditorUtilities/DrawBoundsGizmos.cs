/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnitySARCommon.Editor
{
    /// <summary>
    /// Renders a boundingbox for Colliders or Renderers for assisting in debugging
    /// </summary>
    public class DrawBoundsGizmos : MonoBehaviour
    {
#if UNITY_EDITOR
        public enum ColliderOrRenderer
        {
            Collider,
            Renderer
        }

        public enum DrawGizmo
        {
            Always,
            Selected,
            ParentSelected
        }

        public ColliderOrRenderer type;
        public DrawGizmo drawGizmo;

        private static readonly Vector3 gizmosSize = Vector3.one * .1f;

        public void OnDrawGizmosSelected()
        {
            if (drawGizmo == DrawGizmo.ParentSelected)
            {
                DrawGizmos();
            }
            else if (drawGizmo == DrawGizmo.Selected && Selection.activeTransform == transform)
            {
                DrawGizmos();
            }
        }

        private void OnDrawGizmos()
        {
            if (drawGizmo == DrawGizmo.Always)
            {
                DrawGizmos();
            }
        }

        private void DrawGizmos()
        {
            try
            {
                Bounds B = type == ColliderOrRenderer.Renderer ? GetComponent<Renderer>().bounds : GetComponent<Collider>().bounds;
                DrawGizmosFor(B);
            }
            catch
            {
                // nothing to draw the bounds of!
            }
        }

        public static void DrawGizmosFor(Bounds B)
        {
            float[] xVals = new float[] { B.max.x, B.min.x };
            float[] yVals = new float[] { B.max.y, B.min.y };
            float[] zVals = new float[] { B.max.z, B.min.z };

            for (int i = 0; i < xVals.Length; i++)
            {
                float x = xVals[i];
                for (int j = 0; j < yVals.Length; j++)
                {
                    float y = yVals[j];
                    for (int k = 0; k < zVals.Length; k++)
                    {
                        float z = zVals[k];

                        Vector3 point = new Vector3(x, y, z);
                        Gizmos.DrawCube(point, gizmosSize);

                        if (i == 0)
                        {
                            Gizmos.DrawLine(point, new Vector3(xVals[1], y, z));
                        }
                        if (j == 0)
                        {
                            Gizmos.DrawLine(point, new Vector3(x, yVals[1], z));
                        }
                        if (k == 0)
                        {
                            Gizmos.DrawLine(point, new Vector3(x, y, zVals[1]));
                        }
                    }
                }
            }
        }
#endif
    }
}