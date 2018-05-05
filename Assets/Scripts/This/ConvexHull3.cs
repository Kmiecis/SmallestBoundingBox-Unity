using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConvexHull3
{
    List<Vector3> m_dataCloud;
    public List<Vector3> DataCloud
    {
        get { return m_dataCloud; }
        set { m_dataCloud = value; }
    }

    List<BasicTriangle> m_basicTriangles;
    public List<BasicTriangle> BasicEdges
    {
        get { return m_basicTriangles; }
    }

    List<int> m_vertices;
    public List<int> Vertices
    {
        get
        {
            if (Utilities.IsNullOrEmpty(m_vertices))
            {
                m_vertices = new List<int>(m_basicTriangles.Count * 3);
                foreach (BasicTriangle basicTriangle in m_basicTriangles)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        m_vertices.Add(basicTriangle.v[i]);
                    }
                }
            }

            return m_vertices;
        }
    }

    List<Triangle> m_triangles;
    public List<Triangle> Triangles
    {
        get
        {
            if (Utilities.IsNullOrEmpty(m_triangles))
            {
                m_triangles = new List<Triangle>(m_basicTriangles.Count);
                foreach (BasicTriangle triangle in m_basicTriangles)
                {
                    m_triangles.Add(new Triangle(m_dataCloud[triangle.v[0]], m_dataCloud[triangle.v[1]], m_dataCloud[triangle.v[2]]));
                }
            }

            return m_triangles;
        }
    }


    public ConvexHull3(List<Vector3> dataCloud)
    {
        m_dataCloud = dataCloud;
    }


    public void AlgorithmRandomIncremental()
    {
        ClearIfNecessary();

        if (m_dataCloud.Count < 4)
        {
            Debugger.Get.Log("Insufficient points to generate a tetrahedron.", DebugOption.CH3);
            return;
        }

        int index = 0;
        // Pick first point.
        KeyValuePair<int, Vector3> P1 = new KeyValuePair<int, Vector3>(index, m_dataCloud[index]);

        KeyValuePair<int, Vector3> P2 = new KeyValuePair<int, Vector3>(-1, Vector3.zero);
        while (P2.Key == -1 && ++index < m_dataCloud.Count)
        {
            // Two points form a line if they are not equal.
            Vector3 P = m_dataCloud[index];
            if (!ExtMathf.Equal(P2.Value, P))
            {
                P2 = new KeyValuePair<int, Vector3>(index, P);
            }
        }
        if (P2.Key == -1)
        {
            Debugger.Get.Log("Couldn't find two not equal points.", DebugOption.CH3);
            return;
        }

        // Points on line and on the same plane, still should be considered later on. Happens.
        List<int> skippedPoints = new List<int>();

        Edge line = new Edge(P1.Value, P2.Value);
        KeyValuePair<int, Vector3> P3 = new KeyValuePair<int, Vector3>(-1, Vector3.zero);
        while (P3.Key == -1 && ++index < m_dataCloud.Count)
        {
            // Three points form a triangle if they are not on the same line.
            Vector3 P = m_dataCloud[index];
            if (!line.CalculateIfIsOnLine(P))
            {
                P3 = new KeyValuePair<int, Vector3>(index, P);
            }
            else
            {
                skippedPoints.Add(index);
            }
        }
        if (P3.Key == -1)
        {
            Debugger.Get.Log("Couldn't find three not linear points.", DebugOption.CH3);
            return;
        }

        Triangle planeTriangle = new Triangle(P1.Value, P2.Value, P3.Value);
        KeyValuePair<int, Vector3> P4 = new KeyValuePair<int, Vector3>(-1, Vector3.zero);
        while (P4.Key == -1 && ++index < m_dataCloud.Count)
        {
            // Four points form a tetrahedron if they are not on the same plane.
            Vector3 P = m_dataCloud[index];
            if (planeTriangle.CalculateRelativePosition(P) != 0)
            {
                P4 = new KeyValuePair<int, Vector3>(index, P);
            }
            else
            {
                skippedPoints.Add(index);
            }
        }
        if (P4.Key == -1)
        {
            Debugger.Get.Log("Couldn't find four not planar points.", DebugOption.CH3);
            return;
        }

        // Calculate reference centroid of ConvexHull.
        Vector3 centroid = ExtMathf.Centroid(P1.Value, P2.Value, P3.Value, P4.Value);

        List<BasicTriangle> tetrahedronTriangles = new List<BasicTriangle>();
        tetrahedronTriangles.Add(new BasicTriangle(P3.Key, P2.Key, P1.Key));
        tetrahedronTriangles.Add(new BasicTriangle(P1.Key, P2.Key, P4.Key));
        tetrahedronTriangles.Add(new BasicTriangle(P2.Key, P3.Key, P4.Key));
        tetrahedronTriangles.Add(new BasicTriangle(P3.Key, P1.Key, P4.Key));

        foreach (BasicTriangle basicTriangle in tetrahedronTriangles)
        {
            Triangle triangle = new Triangle(
                m_dataCloud[basicTriangle.v[0]],
                m_dataCloud[basicTriangle.v[1]],
                m_dataCloud[basicTriangle.v[2]]
            );

            if (triangle.CalculateRelativePosition(centroid) != -1)
            {   // Centroid of ConvexHull should always be inside.
                basicTriangle.Reverse();
            }
            m_basicTriangles.Add(basicTriangle);
        }

        Dictionary<int, HashSet<BasicTriangle>> pointFacets = new Dictionary<int, HashSet<BasicTriangle>>();
        Dictionary<BasicTriangle, HashSet<int>> facetPoints = new Dictionary<BasicTriangle, HashSet<int>>();

        foreach (BasicTriangle basicTriangle in m_basicTriangles)
        {
            Triangle triangle = new Triangle(
                m_dataCloud[basicTriangle.v[0]],
                m_dataCloud[basicTriangle.v[1]],
                m_dataCloud[basicTriangle.v[2]]
            );

            for (int p = index; p < m_dataCloud.Count; p++)
            {
                if (triangle.CalculateRelativePosition(m_dataCloud[p]) == 1)
                {
                    if (!pointFacets.ContainsKey(p))
                    {
                        pointFacets.Add(p, new HashSet<BasicTriangle>());
                    }
                    pointFacets[p].Add(basicTriangle);

                    if (!facetPoints.ContainsKey(basicTriangle))
                    {
                        facetPoints.Add(basicTriangle, new HashSet<int>());
                    }
                    facetPoints[basicTriangle].Add(p);
                }
            }

            foreach (int p in skippedPoints)
            {
                if (triangle.CalculateRelativePosition(m_dataCloud[p]) == 1)
                {
                    if (!pointFacets.ContainsKey(p))
                    {
                        pointFacets.Add(p, new HashSet<BasicTriangle>());
                    }
                    pointFacets[p].Add(basicTriangle);
                    facetPoints[basicTriangle].Add(p);
                }
            }
        }

        while (pointFacets.Count > 0)
        {
            var firstPointFacet = pointFacets.First();
            if (firstPointFacet.Value.Count > 0)
            {
                Dictionary<BasicEdge, BasicEdge> horizon = new Dictionary<BasicEdge, BasicEdge>();

                foreach (BasicTriangle basicTriangle in firstPointFacet.Value)
                {
                    for (int a = 2, b = 0; b < 3; a = b++)
                    {
                        BasicEdge edge = new BasicEdge(basicTriangle.v[a], basicTriangle.v[b]);
                        BasicEdge edgeUnordered = edge.Unordered();

                        if (horizon.ContainsKey(edgeUnordered))
                        {
                            horizon.Remove(edgeUnordered);
                        }
                        else
                        {
                            horizon.Add(edgeUnordered, edge);
                        }
                    }
                }

                int pointKey = firstPointFacet.Key;
                foreach (BasicTriangle facet in firstPointFacet.Value)
                {
                    foreach (int facetPointKey in facetPoints[facet])
                    {
                        if (facetPointKey != pointKey)
                        {
                            pointFacets[facetPointKey].Remove(facet);
                        }
                    }

                    facetPoints.Remove(facet);
                    m_basicTriangles.Remove(facet);
                }
                pointFacets.Remove(pointKey);

                foreach (KeyValuePair<BasicEdge, BasicEdge> edges in horizon)
                {
                    BasicTriangle newBasicTriangle = new BasicTriangle(edges.Value.v[0], edges.Value.v[1], pointKey);

                    m_basicTriangles.Add(newBasicTriangle);

                    if (pointFacets.Count > 0)
                    {
                        Triangle newTriangle = new Triangle(
                            m_dataCloud[newBasicTriangle.v[0]],
                            m_dataCloud[newBasicTriangle.v[1]],
                            m_dataCloud[newBasicTriangle.v[2]]
                        );

                        foreach (var pointFacetsKey in pointFacets.Keys)
                        {
                            if (newTriangle.CalculateRelativePosition(m_dataCloud[pointFacetsKey]) == 1)
                            {
                                pointFacets[pointFacetsKey].Add(newBasicTriangle);

                                if (!facetPoints.ContainsKey(newBasicTriangle))
                                {
                                    facetPoints.Add(newBasicTriangle, new HashSet<int>());
                                }
                                facetPoints[newBasicTriangle].Add(pointFacetsKey);
                            }
                        }
                    }
                }

            }
            else
            {
                pointFacets.Remove(firstPointFacet.Key);
            }
        }
    }


    void ClearIfNecessary()
    {
        if (m_basicTriangles == null)
        {
            m_basicTriangles = new List<BasicTriangle>();
        }
        else
        {
            m_basicTriangles.Clear();
        }
        if (!Utilities.IsNullOrEmpty(m_vertices))
        {
            m_vertices.Clear();
        }
        if (!Utilities.IsNullOrEmpty(m_triangles))
        {
            m_triangles.Clear();
        }
    }


    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[m_basicTriangles.Count * 3];
        int[] triangles = new int[m_basicTriangles.Count * 3];

        int index = 0;
        foreach (BasicTriangle triangle in m_basicTriangles)
        {
            triangles[index] = index;
            triangles[index + 1] = index + 1;
            triangles[index + 2] = index + 2;

            vertices[index] = m_dataCloud[triangle.v[0]];
            vertices[index + 1] = m_dataCloud[triangle.v[1]];
            vertices[index + 2] = m_dataCloud[triangle.v[2]];

            index += 3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}