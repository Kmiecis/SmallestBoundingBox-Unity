using UnityEngine;

public class Box
{
    /// <summary>
    /// Reference corner of the box. It is bottom left corner in bottom rectangle of the box.
    /// </summary>
    public Vector3 Corner;
    /// <summary>
    /// Position of the box in its centre.
    /// </summary>
    public Vector3 Centre { get { return Corner + (X * Extents.x + Y * Extents.y + Z * Extents.z) * .5f; } }
    /// <summary>
    /// Axes defining the box rotation.
    /// </summary>
    public Vector3 X, Y, Z;
    /// <summary>
    /// Extents of the box along proper axes. Should always be positive.
    /// </summary>
    public Vector3 Extents;

    /// <summary>
    /// Creates undefined box. Requires all variables to be explicitly set before use.
    /// </summary>
    public Box()
    {
    }


    public Box(Vector3 corner, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, Vector3 extents)
    {
        Corner = corner;
        X = xAxis;
        Y = yAxis;
        Z = zAxis;
        Extents = extents;
    }
}