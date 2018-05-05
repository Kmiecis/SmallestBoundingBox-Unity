using System;

public class BasicTriangle : IEquatable<BasicTriangle>
{
    public int[] v;

    public BasicTriangle(int V1, int V2, int V3)
    {
        v = new int[] { V1, V2, V3 };
    }


    public void Reverse()
    {
        Utilities.Swap(ref v[0], ref v[2]);
    }


    public bool Equals(BasicTriangle other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return v[0].Equals(other.v[0]) && v[1].Equals(other.v[1]) && v[2].Equals(other.v[2]);
    }


    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (GetType() != other.GetType()) return false;

        return Equals((BasicTriangle)other);
    }


    public override int GetHashCode()
    {
        return Hash.GetHashCode(v[0], v[1], v[2]);
    }


    public override string ToString()
    {
        return string.Format("BT[{0}, {1}, {2}]", v[0], v[1], v[2]);
    }
}