using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CstLoader : MonoBehaviour {
    //public string cst;
    private string cst;
    public Matrix4x4 coordinateSpaceTransform = Matrix4x4.identity;
    public float scale = 1;
    
    private void Start()
    {
		//cst = Application.dataPath + "/CstLoader.dat";
		cst = Application.streamingAssetsPath + "/CstLoader.dat";
		//print (Application.dataPath);
        var cstmat = UnitySARCommon.IO.MatrixIO.ReadMatrixFromFile(cst);
        setCoordinateSpaceTransforMatrix(cstmat);

        transform.position = coordinateSpaceTransform.MultiplyPoint3x4(Vector3.zero);
        transform.rotation = UnitySARCommon.IO.MatrixIO.QuaternionFromMatrix(coordinateSpaceTransform);
        transform.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// Set the associated Coorindate Space Transform matrix to be used with this tracker transform
    /// </summary>
    /// <param name="cstMat">The precalculated CoordinateSpaceTransform matrix</param>
    public void setCoordinateSpaceTransforMatrix(Matrix4x4 cstMat)
    {
        coordinateSpaceTransform = cstMat;
        Vector3 x = new Vector3(coordinateSpaceTransform.m00, coordinateSpaceTransform.m10, coordinateSpaceTransform.m20);
        float scalar = x.magnitude;

        Vector3 translateToZero = new Vector3(coordinateSpaceTransform[0, 3], coordinateSpaceTransform[1, 3], coordinateSpaceTransform[2, 3]);
        coordinateSpaceTransform = MultMatrixScalar(coordinateSpaceTransform, scalar);

        coordinateSpaceTransform.m03 = translateToZero.x;
        coordinateSpaceTransform.m13 = translateToZero.y;
        coordinateSpaceTransform.m23 = translateToZero.z;

        scale = scalar;
    }

    static Matrix4x4 MultMatrixScalar(Matrix4x4 m, float s)
    {
        Matrix4x4 r = Matrix4x4.identity;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                r[i, j] = m[i, j] / s;
            }
        }
        return r;
    }
}
