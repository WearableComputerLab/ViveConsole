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
    /// The Famous WCL Box!
    /// </summary>
    [ExecuteInEditMode]
    public class WCLBox : MonoBehaviour
    {
        public enum Measurement
        {
            Millimetres,
            Centimetres,
            Metres
        };

        private const float BoxWidth = 350;
        private const float BoxHeight = 254;
        private const float BoxDepth = 206;

        private float width = 350;
        private float height = 254;
        private float depth = 206;

        public Measurement measurement;

        void OnValidate()
        {
            if (measurement == Measurement.Millimetres)
            {
                width = BoxWidth / 1000;
                depth = BoxDepth / 1000;
                height = BoxHeight / 1000;
            }
            else if (measurement == Measurement.Centimetres)
            {
                width = BoxWidth / 100;
                depth = BoxDepth / 100;
                height = BoxHeight / 100;
            }
            else if (measurement == Measurement.Metres)
            {
                width = BoxWidth;
                depth = BoxDepth;
                height = BoxHeight;
            }

            transform.localScale = GetSize();
            transform.position = Vector3.zero + GetOffset();
        }

        public void UpdateSizeOnMeasurement()
        {
            OnValidate();
        }


        // Update is called once per frame
        void Update()
        {

        }

        public Vector3 GetSize()
        {
            return new Vector3(width, height, depth);
        }

        public Vector3 GetOffset()
        {
            return new Vector3(0, height / 2, 0);
        }
    }
}
