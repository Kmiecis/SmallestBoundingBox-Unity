using System;

public class BasicEdge : IEquatable<BasicEdge>
{
    public int[] v;

    public BasicEdge(int v1, int v2)
    {
        v = new int[] { v1, v2 };
    }


    public bool ContainsVertex(int v0)
    {
        foreach (int vertex in v)
        {
            if (v0 == vertex)
            {
                return true;
            }
        }
        return false;
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

        bool found = false;
        foreach (int vertex in v)
        {
            if (other.ContainsVertex(vertex))
            {
                if (found)
                {
                    return true;
                }

                found = true;
            }
        }
        return false;
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

    public override string ToString()
    {
        return string.Format("[{0},{1}]", v[0], v[1]);
    }
}