using UnityEngine;
using System.Collections.Generic;

public struct ExtMathf
{
    private const float m_tolerance = 1e-5f;

    public static bool Equal(float left, float right, float tolerance = m_tolerance)
    {
        return !Greater(left, right, tolerance) && !Less(left, right, tolerance);
    }


    public static bool Greater(float left, float right, float tolerance = m_tolerance)
    {
        return left - tolerance > right;
    }

    
    public static bool Less(float left, float right, float tolerance = m_tolerance)
    {
        return left < right - tolerance;
    }


    public static bool Equal(Vector3 left, Vector3 right, float tolerance = m_tolerance)
    {
        return Equal(left.x, right.x, tolerance) && Equal(left.y, right.y, tolerance) && Equal(left.z, right.z, tolerance);
    }


    public static Vector3 Centroid(params Vector3[] points)
    {
        Vector3 result = Vector3.zero;

        if (points.Length == 0)
        {
            Debugger.Get.Warning(string.Format("Passed empty list of points to Centroid function."), DebugOption.ExtMath);
            return result;
        }

        foreach (Vector3 point in points)
        {
            result += point;
        }

        return result * (1f / points.Length);
    }


    public static Vector3 Rotation2D(Vector3 vector, float angle)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        float dx = vector.x;
        float dy = vector.y;

        vector.x = (cos * dx) - (sin * dy);
        vector.y = (sin * dx) + (cos * dy);

        return vector;
    }
}