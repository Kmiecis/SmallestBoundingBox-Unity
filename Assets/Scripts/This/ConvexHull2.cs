using UnityEngine;
using System;
using System.Collections.Generic;

public class ConvexHull2
{
    List<Vector3> m_dataCloud;
    public List<Vector3> DataCloud
    {
        get { return m_dataCloud; }
        set { m_dataCloud = value; }
    }


    List<BasicEdge> m_basicEdges;
    public List<BasicEdge> BasicEdges
    {
        get { return m_basicEdges; }
    }


    List<Edge> m_edges;
    public List<Edge> Edges
    {
        get
        {
            if (Utilities.IsNullOrEmpty(m_edges))
            {
                m_edges = new List<Edge>();
                foreach (BasicEdge edge in m_basicEdges)
                {
                    m_edges.Add(new Edge(m_dataCloud[edge.v[0]], m_dataCloud[edge.v[1]]));
                }
            }
            return m_edges;
        }
    }


    List<int> m_vertices;
    public List<int> Vertices
    {
        get
        {
            if (Utilities.IsNullOrEmpty(m_vertices))
            {
                m_vertices = new List<int>();
                int index = 0;
                foreach (BasicEdge basicEdge in m_basicEdges)
                {
                    m_vertices.Add(index);
                    m_vertices.Add(index + 1);
                    m_vertices.Add(index + 2);
                    index += 3;
                }
            }

            return m_vertices;
        }
    }


    List<Vector3> m_triangles;
    public List<Vector3> Triangles
    {
        get
        {
            if (Utilities.IsNullOrEmpty(m_triangles))
            {
                m_triangles = new List<Vector3>();
                Vector3 centroid = ExtMathf.Centroid(m_dataCloud.ToArray());
                foreach (BasicEdge edge in m_basicEdges)
                {
                    m_triangles.Add(m_dataCloud[edge.v[0]]);
                    m_triangles.Add(m_dataCloud[edge.v[1]]);
                    m_triangles.Add(centroid);
                }
            }
            return m_triangles;
        }
    }


    public ConvexHull2(List<Vector3> dataCloud)
    {
        m_dataCloud = dataCloud;
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
            Edge line = new Edge(m_dataCloud[left], m_dataCloud[right]);
            foreach (int point in points)
            {
                float squaredDistance = line.CalculateSquaredDistance(m_dataCloud[point]);
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
            Edge onLeftEdge = new Edge(m_dataCloud[left], m_dataCloud[p]);
            List<int> onRight = new List<int>();
            Edge onRightEdge = new Edge(m_dataCloud[p], m_dataCloud[right]);
            foreach (int point in points)
            {
                Vector3 P = m_dataCloud[point];
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
        m_dataCloud.Sort((a, b) => a.x.CompareTo(b.x));

        int first = 0;
        int last = m_dataCloud.Count - 1;
        Edge edge = new Edge(m_dataCloud[first], m_dataCloud[last]);

        List<int> above = new List<int>();
        List<int> below = new List<int>();

        for (int p = 1; p < last; p++)
        {
            if (edge.CalculateRelative2DPosition(m_dataCloud[p]) == 1)
            {
                below.Add(p);
            }
            else
            {
                above.Add(p);
            }
        }

        m_basicEdges.AddRange(SeekEdges(first, last, above));
        m_basicEdges.AddRange(SeekEdges(last, first, below));
    }


    void ClearIfNecessary()
    {
        if (m_basicEdges == null)
        {
            m_basicEdges = new List<BasicEdge>();
        }
        else
        {
            m_basicEdges.Clear();
        }
        if (!Utilities.IsNullOrEmpty(m_vertices))
        {
            m_vertices.Clear();
        }
        if (!Utilities.IsNullOrEmpty(m_triangles))
        {
            m_triangles.Clear();
        }
        if (!Utilities.IsNullOrEmpty(m_edges))
        {
            m_edges.Clear();
        }
    }


    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = Triangles.ToArray();
        mesh.triangles = Vertices.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}