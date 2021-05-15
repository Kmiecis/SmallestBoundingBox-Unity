using UnityEngine;

public static class Equation3
{
    public static float SolveX(Vector3 parameters, float y, float z)
    {
        return (parameters.y * y + parameters.z * z) / (-parameters.y);
    }

    public static float SolveY(Vector3 parameters, float x, float z)
    {
        return (parameters.x * x + parameters.z * z) / (-parameters.y);
    }

    public static float SolveZ(Vector3 parameters, float x, float y)
    {
        return (parameters.x * x + parameters.y * y) / (-parameters.z);
    }
}