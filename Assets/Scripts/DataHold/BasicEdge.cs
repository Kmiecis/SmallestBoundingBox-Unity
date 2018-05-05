using System;

public class BasicEdge : IEquatable<BasicEdge>
{
    public int[] v;

    public BasicEdge(int v1, int v2)
    {
        v = new int[] { v1, v2 };
    }


    public void Unorder()
    {
        if (v[0] > v[1])
        {
            Utilities.Swap(ref v[0], ref v[1]);
        }
    }


    public BasicEdge Unordered()
    {
        BasicEdge result = new BasicEdge(v[0], v[1]);
        result.Unorder();
        return result;
    }


    public bool Equals(BasicEdge other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return v[0].Equals(other.v[0]) && v[1].Equals(other.v[1]);
    }


    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (GetType() != other.GetType()) return false;

        return Equals((BasicEdge)other);
    }


    public override int GetHashCode()
    {
        return Hash.GetHashCode(v[0], v[1]);
    }
}