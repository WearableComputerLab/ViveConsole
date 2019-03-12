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

using System.IO;
using UnityEngine;

namespace UnitySARCommon.IO
{
    /// <summary>
    /// Matrix Input/Output functions.
    /// </summary>
    public static class MatrixIO
    {
        /// <summary>
        /// Write a matrix out to File
        /// The default extension is used
        /// </summary>
        /// <param name="mat_">The matrix to save</param>
        /// <param name="filename_">The file</param>
        public static void WriteMatrixToFile(Matrix4x4 mat_, string filename_)
        {
            WriteMatrixToFile(mat_, filename_, "");
        }

        /// <summary>
        /// Write a matrix out to File with a user defined extension
        /// </summary>
        /// <param name="mat_">The matrix to save</param>
        /// <param name="filename_">The file</param>
        /// <param name="extension_">The file extension to use</param>
        public static void WriteMatrixToFile(Matrix4x4 mat_, string filename_, string extension_)
        {
            string[] lines = new string[4];
            for (int i = 0; i < 4; i++)
            {
                lines[i] = mat_[i, 0] + " " + mat_[i, 1] + " " + mat_[i, 2] + " " + mat_[i, 3];
            }
            File.WriteAllLines(filename_ + extension_, lines);
        }

        /// <summary>
        /// Read a matrix from file
        /// </summary>
        /// <param name="file_">The file to read</param>
        /// <returns></returns>
        public static Matrix4x4 ReadMatrixFromFile(string file_)
        { 
            Matrix4x4 mat = new Matrix4x4();
            string[] lines = File.ReadAllLines(file_);
            for (int row = 0; row < 4; row++)
            {
                string[] rowVals = lines[row].Split(' ');
                for (int col = 0; col < 4; col++)
                {
                    float val = float.Parse(rowVals[col]);
                    mat[row, col] = val;
                }
            }
            return mat;
        }

        /// <summary>
        /// Calculate a quaternion from a given matrix
        /// </summary>
        /// <param name="m">The matrix to calculate the quaternion from</param>
        /// <returns>The calculated quaternion</returns>
        public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
        {
            // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            Quaternion q = new Quaternion();

            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
            q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
            q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
            q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
            Normalize(ref q);
            return q;
        }

        /// <summary>
        /// Calculate a matrix from a given quaternion
        /// </summary>
        /// <param name="q">The quaternion to calculate the matrix from</param>
        /// <returns>The calculated matrix</returns>
        public static Matrix4x4 MatrixFromQuaternion(Quaternion q)
        {
            // Adapted from: http://wcl.ml.unisa.edu.au/gitlab/code/libwcl/blob/master/src/wcl/maths/Quaternion.cpp
            float s, xs, ys, zs, wx, wy, wz, xx, xy, xz, yy, yz, zz;
            s = 2.0f / (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);

            xs = s * q.x; ys = s * q.y; zs = s * q.z;
            wx = q.w * xs; wy = q.w * ys; wz = q.w * zs;
            xx = q.x * xs; xy = q.x * ys; xz = q.x * zs;
            yy = q.y * ys; yz = q.y * zs; zz = q.z * zs;

            Matrix4x4 m = Matrix4x4.identity;

            m.m00 = 1.0f - (yy + zz);
            m.m01 = xy - wz;
            m.m02 = xz + wy;

            m.m10 = xy + wz;
            m.m11 = 1.0f - (xx + zz);
            m.m12 = yz - wx;

            m.m20 = xz - wy;
            m.m21 = yz + wx;
            m.m22 = 1.0f - (xx + yy);

            m.m33 = 1.0f;

            return m;
        }

        private static void Normalize(ref Quaternion q)
        {
            float sum = 0;
            for (int i = 0; i < 4; ++i)
                sum += q[i] * q[i];
            float magnitudeInverse = 1 / Mathf.Sqrt(sum);
            for (int i = 0; i < 4; ++i)
                q[i] *= magnitudeInverse;
        }

    }
}
