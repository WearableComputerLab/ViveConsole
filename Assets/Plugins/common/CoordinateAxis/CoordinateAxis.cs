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

namespace UnitySARCommon.Display
{
    /// <summary>
    /// Represent the three axis of an object. Red (X), Green (Y), Blue (Z)
    /// </summary>
    public class CoordinateAxis : MonoBehaviour
    {
        private GameObject xAxis;
        private GameObject yAxis;
        private GameObject zAxis;

        private LineRenderer xAxisRenderer;
        private LineRenderer yAxisRenderer;
        private LineRenderer zAxisRenderer;

        public enum Measurement
        {
            Millimetres,
            Centimetres,
            Metres
        };

        public Measurement measurement;

        private float length = 0.1F;

        public void CreateCoordinateAxis(GameObject parent)
        {
            xAxis = new GameObject();
            yAxis = new GameObject();
            zAxis = new GameObject();

            Material xMat = Resources.Load("Materials/xAxis", typeof(Material)) as Material;
            Material yMat = Resources.Load("Materials/yAxis", typeof(Material)) as Material;
            Material zMat = Resources.Load("Materials/zAxis", typeof(Material)) as Material;

            xAxis.name = "X-Axis";
            yAxis.name = "Y-Axis";
            zAxis.name = "Z-Axis";

            xAxis.transform.parent = this.transform;
            yAxis.transform.parent = this.transform;
            zAxis.transform.parent = this.transform;

            xAxisRenderer = new LineRenderer();
            yAxisRenderer = new LineRenderer();
            zAxisRenderer = new LineRenderer();

            xAxisRenderer = xAxis.AddComponent<LineRenderer>();
            yAxisRenderer = yAxis.AddComponent<LineRenderer>();
            zAxisRenderer = zAxis.AddComponent<LineRenderer>();

            CreateAxisObjectsLineRenderer(xAxisRenderer, xMat);
            CreateAxisObjectsLineRenderer(yAxisRenderer, yMat);
            CreateAxisObjectsLineRenderer(zAxisRenderer, zMat);
        }

        void UpdateAxisSize(LineRenderer lr, Measurement m)
        {
            if (m == Measurement.Millimetres)
            {
                lr.widthMultiplier = 0.002F;
                length = 0.1F;
            }
            else if (m == Measurement.Centimetres)
            {
                lr.widthMultiplier = 0.02F;
                length = 1.0F;
            }
            else if (m == Measurement.Metres)
            {
                lr.widthMultiplier = 0.2F;
                length = 10F;
            }
        }

        void OnValidate()
        {
            if (measurement == Measurement.Millimetres)
            {
                UpdateAxisSize(xAxisRenderer, Measurement.Millimetres);
                UpdateAxisSize(yAxisRenderer, Measurement.Millimetres);
                UpdateAxisSize(zAxisRenderer, Measurement.Millimetres);
            }
            else if (measurement == Measurement.Centimetres)
            {
                UpdateAxisSize(xAxisRenderer, Measurement.Centimetres);
                UpdateAxisSize(yAxisRenderer, Measurement.Centimetres);
                UpdateAxisSize(zAxisRenderer, Measurement.Centimetres);
            }
            else if (measurement == Measurement.Metres)
            {
                UpdateAxisSize(xAxisRenderer, Measurement.Metres);
                UpdateAxisSize(yAxisRenderer, Measurement.Metres);
                UpdateAxisSize(zAxisRenderer, Measurement.Metres);
            }

            DrawCoordinateAxis();
        }

        private void CreateAxisObjectsLineRenderer(LineRenderer lr, Material mat)
        {
            lr.widthMultiplier = 0.002F;
            lr.positionCount = 2;
            lr.material = mat;
            lr.useWorldSpace = false;
        }

        public void DrawCoordinateAxis()
        {
            xAxisRenderer.SetPosition(0, new Vector3(xAxis.transform.position.x, xAxis.transform.position.y, xAxis.transform.position.z));
            xAxisRenderer.SetPosition(1, new Vector3(xAxis.transform.position.x + length, xAxis.transform.position.y, xAxis.transform.position.z));

            yAxisRenderer.SetPosition(0, new Vector3(yAxis.transform.position.x, yAxis.transform.position.y, yAxis.transform.position.z));
            yAxisRenderer.SetPosition(1, new Vector3(yAxis.transform.position.x, yAxis.transform.position.y + length, yAxis.transform.position.z));

            zAxisRenderer.SetPosition(0, new Vector3(zAxis.transform.position.x, zAxis.transform.position.y, zAxis.transform.position.z));
            zAxisRenderer.SetPosition(1, new Vector3(zAxis.transform.position.x, zAxis.transform.position.y, zAxis.transform.position.z + length));
        }

        void Start()
        {
            this.transform.parent = transform.parent;
        }

    }
}
