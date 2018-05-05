using UnityEngine;
using System;

public class Triangle : IEquatable<Triangle>
{
    /// <summary>
    /// Triangle vertex.
    /// </summary>
    public Vector3[] V;
    /// <summary>
    /// Triangle normal.
    /// </summary>
    public Vector3 N;
    /// <summary>
    /// Triangle offset.
    /// </summary>
    public float d;
    /// <summary>
    /// Triangle local 2D axis.
    /// </summary>
    public Vector3 X, Y;
    

    public Triangle(Vector3 V1, Vector3 V2, Vector3 V3)
    {
        V = new Vector3[] { V1, V2, V3 };

        N = Vector3.Cross(V2 - V1, V3 - V2).normalized;

        d = Vector3.Dot(V3, N);

        X = (V3 - V2).normalized;
        Y = Vector3.Cross(N, X).normalized;
    }

    /// <summary>
    /// Returns shortest distance from this Triangle to point 'V'.
    /// </summary>
    public float CalculateDistance(Vector3 P)
    {
        float distance = Vector3.Dot(P, N);
        
        return distance - d;
    }

    /// <summary>
    /// Returns relative position of point 'V' to this Triangle.
    /// +1 - point lies in front of the triangle.
    ///  0 - point lies on the triangle.
    /// -1 - point lies behind the triangle.
    /// </summary>
    public int CalculateRelativePosition(Vector3 P)
    {
        float distance = CalculateDistance(P);

        return ExtMathf.Greater(distance, 0f) ?
            1 : (ExtMathf.Less(distance, 0f) ?
                -1 : 0);
    }


    public void Reverse()
    {
        Utilities.Swap(ref V[0], ref V[2]);
        N *= -1;
    }

    /// <summary>
    /// Changes point from world 3D position to local 2D position on triangle plane.
    /// </summary>
    public Vector3 ToLocal(Vector3 P)
    {
        Vector3 dV = P - V[1];

        float x = Vector3.Dot(X, dV);
        float y = Vector3.Dot(Y, dV);

        return new Vector3(x, y);
    }

    /// <summary>
    /// Changes point from local 2D position on triangle plane to world 3D position.
    /// </summary>
    public Vector3 ToWorld(Vector3 P)
    {   // General equation: P = Origin + x*X + y*Y + s*N... but s is neglected, which means we consider only points directly on plane
        return V[1] + P.x * X + P.y * Y;
    }


    public bool Equals(Triangle other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return V[0].Equals(other.V[0]) && V[1].Equals(other.V[1]) && V[2].Equals(other.V[2]);
    }


    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (other.GetType() != this.GetType()) return false;

        return Equals((Triangle)other);
    }


    public override int GetHashCode()
    {
        return Hash.GetHashCode(V[0], V[1], V[2]);
    }


    public override string ToString()
    {
        return string.Format("T[{0}, {1}, {2}]", V[0], V[1], V[2]);
    }
}