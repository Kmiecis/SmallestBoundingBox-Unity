using UnityEngine;
using System.Collections;

public static class Tester
{
    public static void TestExtMath()
    {
        float a = 1f;
        float b = 1f + 1e-6f;
        float c = 1f + 1e-4f;
        Debug.Assert(ExtMathf.Equal(a, b));
        Debug.Assert(!ExtMathf.Equal(a, c));
        Debug.Assert(ExtMathf.Greater(c, a));
        Debug.Assert(!ExtMathf.Greater(b, a));
        Debug.Assert(ExtMathf.Less(a, c));
        Debug.Assert(!ExtMathf.Less(a, b));

        Vector3 A = new Vector3(-1, 0, -1);
        Vector3 B = new Vector3(1, 0, 1);
        Vector3 C = new Vector3(0, 1, 0);
        Vector3 D = new Vector3(0, (1f / 3f), 0);
        Debug.Assert(ExtMathf.Centroid(A, B, C) == D);

        Vector3 xAxis = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        Vector3 yAxis;
        Vector3 zAxis;
        ExtMathf.AxesFromAxis(xAxis, out yAxis, out zAxis);
        Debug.Assert(ExtMathf.Equal(Vector3.Cross(yAxis, zAxis).normalized, xAxis));
        Debug.Assert(ExtMathf.Equal(Vector3.Cross(zAxis, xAxis).normalized, yAxis));
        Debug.Assert(ExtMathf.Equal(Vector3.Cross(xAxis, yAxis).normalized, zAxis));
    }
}