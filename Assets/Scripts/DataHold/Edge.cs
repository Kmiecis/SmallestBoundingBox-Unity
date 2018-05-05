using UnityEngine;
using System;

public class Edge : IEquatable<Edge>
{
    public Vector3[] V;

    
    public Edge(Vector3 V1, Vector3 V2)
    {
        V = new Vector3[] { V1, V2 };
    }

    /// <summary>
    /// Returns shortest squared distance from this Edge to point 'V'. A little bit faster than actual distance.
    /// </summary>
    public float CalculateSquaredDistance(Vector3 P)
    {
        Vector3 dV = (V[0] - V[1]).normalized;
        Vector3 dP = P - V[1];
        float t = Vector3.Dot(dP, dV);
        Vector3 C = V[1] + t * dV;
        Vector3 dC = C - P;

        return dC.sqrMagnitude;
    }

    /// <summary>
    /// Returns relative position of point 'V' to this Edge, assuming space to be 2D.
    /// +1 - point lies above the line.
    ///  0 - point lies on the line.
    /// -1 - point lies below the line.
    /// </summary>
    public int CalculateRelative2DPosition(Vector3 P)
    {
        float d = (P.x - V[0].x) * (V[1].y - V[0].y) - (P.y - V[0].y) * (V[1].x - V[0].x);

        return ExtMathf.Greater(d, 0f) ? 
            1 : (ExtMathf.Less(d, 0f) ? 
                -1 : 0);
    }


    /// <summary>
    /// Checks whether point 'V' lies on a line that passes through this Edge.
    /// </summary>
    public bool CalculateIfIsOnLine(Vector3 P)
    {
        float dV = (V[0] - V[1]).sqrMagnitude;
        float dP1 = (V[0] - P).sqrMagnitude;
        float dP2 = (P - V[1]).sqrMagnitude;

        float dP = dP1 + dP2;

        return ExtMathf.Equal(dV, dP);
    }


    public bool Equals(Edge other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return V[0].Equals(other.V[0]) && V[1].Equals(other.V[1]);
    }


    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (this.GetType() != other.GetType()) return false;

        return Equals((Edge)other);
    }


    public override int GetHashCode()
    {
        return Hash.GetHashCode(V[0], V[1]);
    }
}