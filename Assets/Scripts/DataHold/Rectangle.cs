using UnityEngine;

public class Rectangle
{
    /// <summary>
    /// Reference corner of the rectangle. It is bottom left corner of the rectangle.
    /// </summary>
    public Vector3 Corner;
    /// <summary>
    /// Position of the rectangle in its centre.
    /// </summary>
    public Vector3 Centre { get { return Corner + (X * Extents.x + Y * Extents.y) * .5f; } }
    /// <summary>
    /// Axes defining the rectangle rotation.
    /// </summary>
    public Vector3 X, Y;
    /// <summary>
    /// Extents of the rectangle along proper axes. Should always be positive.
    /// </summary>
    public Vector2 Extents;

    /// <summary>
    /// Creates undefined box. Requires all variables to be explicitly set before use.
    /// </summary>
    public Rectangle()
    {
    }

    public Rectangle(Vector3 corner, Vector3 xAxis, Vector3 yAxis, Vector2 extents)
    {
        Corner = corner;
        X = xAxis;
        Y = yAxis;
        Extents = extents;
    }
}