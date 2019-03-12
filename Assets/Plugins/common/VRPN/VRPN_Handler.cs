/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Andrew Irlitti.
*               2017 Daniel Jackson.
*               2016 James Walsh.
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
using System.Runtime.InteropServices;

namespace UnitySARCommon.Tracking
{
    /// <summary>
    /// Collect VRPN packets from the network from a provided host.
    /// Requries the associated VRPN.dll to function.
    /// Only works with 64 bit Unity builds.
    /// </summary>
    public static class VRPN_Handler
    {
        [DllImport("unityVrpn")]
        private static extern double vrpnAnalogExtern(string address, int channel, int frameCount);

        [DllImport("unityVrpn")]
        private static extern bool vrpnButtonExtern(string address, int channel, int frameCount);

        [DllImport("unityVrpn")]
        private static extern double vrpnTrackerExtern(string address, int channel, int component, int frameCount);

        public static double vrpnAnalog(string address, int channel)
        {
            return vrpnAnalogExtern(address, channel, Time.frameCount);
        }

        public static bool vrpnButton(string address, int channel)
        {
            return vrpnButtonExtern(address, channel, Time.frameCount);
        }

        public static Vector3 vrpnTrackerPos(string address, int channel)
        {
            return new Vector3(
                (float)vrpnTrackerExtern(address, channel, 0, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 1, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 2, Time.frameCount));
        }

        public static Quaternion vrpnTrackerQuat(string address, int channel)
        {
            return new Quaternion(
                (float)vrpnTrackerExtern(address, channel, 3, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 4, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 5, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 6, Time.frameCount));
        }
    }
}