using UnityEngine;
using System;
using System.Collections.Generic;

public class ConvexHull2
{
    List<Vector3> mDataCloud;
    public List<Vector3> DataCloud
    {
        get { return mDataCloud; }
        set { mDataCloud = value; }
    }

    List<BasicEdge> mBasicEdges;
    public List<BasicEdge> BasicEdges
    {
        get { return mBasicEdges; }
    }

    HashSet<int> mVertices;
    public HashSet<int> Vertices
    {
        get
        {
            if (mVertices.IsNullOrEmpty())
            {
                mVertices = new HashSet<int>();
                foreach (BasicEdge basicEdge in mBasicEdges)
                {
                    mVertices.Add(basicEdge.v[0]);
                }
            }
            return mVertices;
        }
    }

    List<Edge> mEdges;
    public List<Edge> Edges
    {
        get
        {
            if (mEdges.IsNullOrEmpty())
            {
                mEdges = new List<Edge>();
                foreach (BasicEdge basicEdge in mBasicEdges)
                {
                    mEdges.Add(new Edge(mDataCloud[basicEdge.v[0]], mDataCloud[basicEdge.v[1]]));
                }
            }
            return mEdges;
        }
    }

    Vector3 mCentroid;
    public Vector3 Centroid
    {
        get
        {
            if (mCentroid == Vector3.zero)
            {
                mCentroid = ExtMathf.Centroid(mDataCloud.ToArray());
            }
            return mCentroid;
        }
    }


    public ConvexHull2(List<Vector3> dataCloud)
    {
        mDataCloud = dataCloud;
    }


    public void AlgorithmQuickhull()
    {
        ClearIfNecessary();

        Func<int, int, List<int>, List<BasicEdge>> SeekEdges = null;
        SeekEdges = (int left, int right, List<int> points) =>
        {
            Debugger.Get.Log(
                string.Format("Seeking edges from {0} to {1} in list of {2} items.", left, right, points.Count), DebugOption.CH2);
            List<BasicEdge> result = new List<BasicEdge>();

            if (points.Count == 0)
            {   // If there are no points left outside, return passed points.
                result.Add(new BasicEdge(left, right));
                return result;
            }

            if (points.Count == 1)
            {   // If there is one point left outside, create edges with it and return.
                int point = points[0];
                result.Add(new BasicEdge(left, point));
                result.Add(new BasicEdge(point, right));
                return result;
            }

            // For remaining points, find farthest away.
            KeyValuePair<int, float> pointDistance = new KeyValuePair<int, float>(-int.MaxValue, -float.MaxValue);
            Edge line = new Edge(mDataCloud[left], mDataCloud[right]);
            foreach (int point in points)
            {
                float squaredDistance = line.CalculateSquaredDistance(mDataCloud[point]);
                if (squaredDistance > pointDistance.Value)
                {
                    pointDistance = new KeyValuePair<int, float>(point, squaredDistance);
                }
            }

            // Copy found point and erase it.
            int p = pointDistance.Key;
            points.Remove(p);

            // Create two lists of points outside new edges.
            List<int> onLeft = new List<int>();
            Edge onLeftEdge = new Edge(mDataCloud[left], mDataCloud[p]);
            List<int> onRight = new List<int>();
            Edge onRightEdge = new Edge(mDataCloud[p], mDataCloud[right]);
            foreach (int point in points)
            {
                Vector3 P = mDataCloud[point];
                if (onLeftEdge.CalculateRelative2DPosition(P) == -1)
                {
                    onLeft.Add(point);
                }
                else if (onRightEdge.CalculateRelative2DPosition(P) == -1)
                {
                    onRight.Add(point);
                }
            }

            result.AddRange(SeekEdges(left, p, onLeft));
            result.AddRange(SeekEdges(p, right, onRight));

            return result;
        };

        // Sort points to ease picking leftmost and rightmost.
        mDataCloud.Sort((a, b) => a.x.CompareTo(b.x));

        int first = 0;
        int last = mDataCloud.Count - 1;
        Edge edge = new Edge(mDataCloud[first], mDataCloud[last]);

        List<int> above = new List<int>();
        List<int> below = new List<int>();

        for (int p = 1; p < last; p++)
        {
            if (edge.CalculateRelative2DPosition(mDataCloud[p]) == 1)
            {
                below.Add(p);
            }
            else
            {
                above.Add(p);
            }
        }

        mBasicEdges.AddRange(SeekEdges(first, last, above));
        mBasicEdges.AddRange(SeekEdges(last, first, below));
    }


    void ClearIfNecessary()
    {
        if (mBasicEdges == null)
        {
            mBasicEdges = new List<BasicEdge>();
        }
        else
        {
            mBasicEdges.Clear();
        }
        if (!mVertices.IsNullOrEmpty())
        {
            mVertices.Clear();
        }
        if (!mEdges.IsNullOrEmpty())
        {
            mEdges.Clear();
        }

        mCentroid = Vector3.zero;
    }


    public void TrimDataCloudIfNecessary()
    {
        if (Vertices.Count == DataCloud.Count)
        {   // Data already trimmed.
            return;
        }

        Dictionary<int, int> exchangedVertices = new Dictionary<int, int>();
        List<Vector3> newDataCloud = new List<Vector3>();

        foreach (int p in Vertices)
        {
            exchangedVertices.Add(p, newDataCloud.Count);
            newDataCloud.Add(mDataCloud[p]);
        }

        mVertices.Clear();
        mVertices = new HashSet<int>(exchangedVertices.Values);

        foreach (BasicEdge basicEdge in mBasicEdges)
        {
            for (int i = 0; i < 2; i++)
            {
                Debug.Assert(exchangedVertices.TryGetValue(basicEdge.v[i], out basicEdge.v[i]));
            }
        }

        mDataCloud = newDataCloud;
    }


    public Mesh GetMesh()
    {
        MeshData meshData = new MeshData();

        Vector3 centre = Centroid;
        foreach (BasicEdge basicEdge in mBasicEdges)
        {
            meshData.AddTriangles(mDataCloud[basicEdge.v[0]], mDataCloud[basicEdge.v[1]], centre);
        }

        return meshData.GetMesh();
    }
}