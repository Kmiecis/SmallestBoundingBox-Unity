using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConvexHull3
{
    List<Vector3> mDataCloud;
    public List<Vector3> DataCloud
    {
        get { return mDataCloud; }
        set { mDataCloud = value; }
    }

    List<BasicTriangle> mBasicTriangles;
    public List<BasicTriangle> BasicTriangles
    {
        get { return mBasicTriangles; }
    }

    List<BasicEdge> mBasicEdges;
    public List<BasicEdge> BasicEdges
    {
        get
        {
            if (mBasicEdges.IsNullOrEmpty())
            {
                mBasicEdges = new List<BasicEdge>();

                HashSet<BasicEdge> unorderedBasicEdges = new HashSet<BasicEdge>();
                foreach (BasicTriangle basicTriangle in mBasicTriangles)
                {
                    for (int a = 2, b = 0; b < 3; a = b++)
                    {
                        BasicEdge basicEdge = new BasicEdge(basicTriangle.v[a], basicTriangle.v[b]);
                        BasicEdge unorderedBasicEdge = basicEdge.Unordered();

                        if (!unorderedBasicEdges.Contains(unorderedBasicEdge))
                        {
                            mBasicEdges.Add(basicEdge);
                            unorderedBasicEdges.Add(unorderedBasicEdge);
                        }
                    }
                }
            }

            return mBasicEdges;
        }
    }

    HashSet<int> mVertices;
    public HashSet<int> Vertices
    {
        get
        {
            if (mVertices.IsNullOrEmpty())
            {
                mVertices = new HashSet<int>();
                foreach (BasicTriangle basicTriangle in mBasicTriangles)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        mVertices.Add(basicTriangle.v[i]);
                    }
                }
            }

            return mVertices;
        }
    }

    List<Triangle> mTriangles;
    public List<Triangle> Triangles
    {
        get
        {
            if (mTriangles.IsNullOrEmpty())
            {
                mTriangles = new List<Triangle>(mBasicTriangles.Count);
                foreach (BasicTriangle triangle in mBasicTriangles)
                {
                    mTriangles.Add(Triangle.FromBasicTriangle(triangle, mDataCloud));
                }
            }

            return mTriangles;
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

    /// <summary>
    /// Function for each axis returns it's min vertex index and breadth along it.
    /// </summary>
    public void GetAlongAxesData(
        Vector3 xAxis, Vector3 yAxis, Vector3 zAxis,
        out Pair<int, MinMaxPair> xAxisData,
        out Pair<int, MinMaxPair> yAxisData,
        out Pair<int, MinMaxPair> zAxisData
        )
    {
        Pair<int, float> minVertexX = new Pair<int, float>(0, float.MaxValue);
        Pair<int, float> minVertexY = new Pair<int, float>(0, float.MaxValue);
        Pair<int, float> minVertexZ = new Pair<int, float>(0, float.MaxValue);
        MinMaxPair breadthX = new MinMaxPair();
        MinMaxPair breadthY = new MinMaxPair();
        MinMaxPair breadthZ = new MinMaxPair();

        Vector3 centroid = Centroid;
        for (int i = 0; i < mDataCloud.Count; i++)
        {
            Vector3 point = mDataCloud[i];

            Vector3 direction = point - centroid;
            float xDistance = Vector3.Dot(direction, xAxis);
            float yDistance = Vector3.Dot(direction, yAxis);
            float zDistance = Vector3.Dot(direction, zAxis);

            breadthX.CheckAgainst(xDistance);
            breadthY.CheckAgainst(yDistance);
            breadthZ.CheckAgainst(zDistance);

            if (ExtMathf.Less(xDistance, minVertexX.Second))
            {
                minVertexX.First = i;
                minVertexX.Second = xDistance;
            }

            if (ExtMathf.Less(yDistance, minVertexY.Second))
            {
                minVertexY.First = i;
                minVertexY.Second = yDistance;
            }

            if (ExtMathf.Less(zDistance, minVertexZ.Second))
            {
                minVertexZ.First = i;
                minVertexZ.Second = zDistance;
            }
        }

        xAxisData = new Pair<int, MinMaxPair>(minVertexX.First, breadthX);
        yAxisData = new Pair<int, MinMaxPair>(minVertexY.First, breadthY);
        zAxisData = new Pair<int, MinMaxPair>(minVertexZ.First, breadthZ);
    }

    /// <summary>
    /// Function returns for given axis min vertex index and breadth along it.
    /// </summary>
    public int GetAlongAxisData(Vector3 axis, out MinMaxPair breadthData)
    {
        Pair<int, float> minVertex = new Pair<int, float>(-1, float.MaxValue);
        breadthData = new MinMaxPair();

        Vector3 centroid = Centroid;
        for (int i = 0; i < mDataCloud.Count; i++)
        {
            Vector3 point = mDataCloud[i];

            Vector3 direction = point - centroid;
            float distance = Vector3.Dot(direction, axis);

            breadthData.CheckAgainst(distance);

            if (ExtMathf.Less(distance, minVertex.Second))
            {
                minVertex.First = i;
                minVertex.Second = distance;
            }
        }

        return minVertex.First;
    }

    /// <summary>
    /// Function returns vertices connected to each vertice.
    /// </summary>
    public Dictionary<int, HashSet<int>> GetVertexAdjacencyData()
    {
        Dictionary<int, HashSet<int>> result = new Dictionary<int, HashSet<int>>();

        foreach (BasicTriangle basicTriangle in mBasicTriangles)
        {
            for (int a = 2, b = 0; b < 3; a = b++)
            {
                int v = basicTriangle.v[a];
                int u = basicTriangle.v[b];
                
                if (!result.ContainsKey(v))
                    result.Add(v, new HashSet<int>());

                result[v].Add(u);
            }
        }

        return result;
    }

    /// <summary>
    /// Function returns faces connected to each edge. Result holds indexes to BasicEdges and BasicTriangles, respectively.
    /// </summary>
    public Dictionary<int, List<int>> GetEdgeFacesData()
    {
        Dictionary<int, List<int>> edgeFaces = new Dictionary<int, List<int>>();

        var basicTriangles = BasicTriangles;
        var basicEdges = BasicEdges;

        for (int t = 0; t < basicTriangles.Count; t++)
        {
            BasicTriangle basicTriangle = basicTriangles[t];
            for (int e = 0; e < basicEdges.Count; e++)
            {
                BasicEdge basicEdge = basicEdges[e];
                if (basicTriangle.ContainsEdge(basicEdge))
                {
                    if (!edgeFaces.ContainsKey(e))
                    {
                        edgeFaces.Add(e, new List<int>());
                    }
                    edgeFaces[e].Add(t);
                }
            }
        }

        return edgeFaces;
    }


    /// <summary>
    /// Function returns list, which provides us with same edge index for both combinations of edge indexes passed into.
    /// Search index form should be: v0 * Vertices.Count + v1.
    /// </summary>
    public List<int> GetUniversalEdgesList()
    {
        int verticesCount = Vertices.Count;
        int emptyEdge = int.MaxValue;
        int basicEdgesCount = 0;    // Should increment in a way that BasicEdges are created, so it will actually point to correct BasicEdge.

        List<int> result = new List<int>().Populate(emptyEdge, verticesCount * verticesCount);

        for (int t = 0; t < BasicTriangles.Count; t++)
        {
            BasicTriangle basicTriangle = BasicTriangles[t];
            for (int a = 2, b = 0; b < 3; a = b++)
            {
                int v = basicTriangle.v[a];
                int u = basicTriangle.v[b];

                if (result[v * verticesCount + u] == emptyEdge)
                {
                    result[v * verticesCount + u] = basicEdgesCount;
                    result[u * verticesCount + v] = basicEdgesCount;

                    basicEdgesCount++;
                }
            }
        }

        return result;
    }


    public ConvexHull3(List<Vector3> dataCloud)
    {
        mDataCloud = dataCloud;
    }


    public void AlgorithmRandomIncremental()
    {
        ClearIfNecessary();

        if (mDataCloud.Count < 4)
        {
            Debugger.Get.Log("Insufficient points to generate a tetrahedron.", DebugOption.CH3);
            return;
        }

        int index = 0;
        // Pick first point.
        KeyValuePair<int, Vector3> P1 = new KeyValuePair<int, Vector3>(index, mDataCloud[index]);

        KeyValuePair<int, Vector3> P2 = new KeyValuePair<int, Vector3>(-1, Vector3.zero);
        while (P2.Key == -1 && ++index < mDataCloud.Count)
        {
            // Two points form a line if they are not equal.
            Vector3 P = mDataCloud[index];
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
        while (P3.Key == -1 && ++index < mDataCloud.Count)
        {
            // Three points form a triangle if they are not on the same line.
            Vector3 P = mDataCloud[index];
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
        while (P4.Key == -1 && ++index < mDataCloud.Count)
        {
            // Four points form a tetrahedron if they are not on the same plane.
            Vector3 P = mDataCloud[index];
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
                mDataCloud[basicTriangle.v[0]],
                mDataCloud[basicTriangle.v[1]],
                mDataCloud[basicTriangle.v[2]]
            );

            if (triangle.CalculateRelativePosition(centroid) != -1)
            {   // Centroid of ConvexHull should always be inside.
                basicTriangle.Reverse();
            }
            mBasicTriangles.Add(basicTriangle);
        }

        Dictionary<int, HashSet<BasicTriangle>> pointFacets = new Dictionary<int, HashSet<BasicTriangle>>();
        Dictionary<BasicTriangle, HashSet<int>> facetPoints = new Dictionary<BasicTriangle, HashSet<int>>();

        foreach (BasicTriangle basicTriangle in mBasicTriangles)
        {
            Triangle triangle = new Triangle(
                mDataCloud[basicTriangle.v[0]],
                mDataCloud[basicTriangle.v[1]],
                mDataCloud[basicTriangle.v[2]]
            );

            for (int p = index; p < mDataCloud.Count; p++)
            {
                if (triangle.CalculateRelativePosition(mDataCloud[p]) == 1)
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
                if (triangle.CalculateRelativePosition(mDataCloud[p]) == 1)
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
                    mBasicTriangles.Remove(facet);
                }
                pointFacets.Remove(pointKey);

                foreach (KeyValuePair<BasicEdge, BasicEdge> edges in horizon)
                {
                    BasicTriangle newBasicTriangle = new BasicTriangle(edges.Value.v[0], edges.Value.v[1], pointKey);

                    mBasicTriangles.Add(newBasicTriangle);

                    if (pointFacets.Count > 0)
                    {
                        Triangle newTriangle = new Triangle(
                            mDataCloud[newBasicTriangle.v[0]],
                            mDataCloud[newBasicTriangle.v[1]],
                            mDataCloud[newBasicTriangle.v[2]]
                        );

                        foreach (var pointFacetsKey in pointFacets.Keys)
                        {
                            if (newTriangle.CalculateRelativePosition(mDataCloud[pointFacetsKey]) == 1)
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
        if (mBasicTriangles == null)
        {
            mBasicTriangles = new List<BasicTriangle>();
        }
        else
        {
            mBasicTriangles.Clear();
        }
        if (!mVertices.IsNullOrEmpty())
        {
            mVertices.Clear();
        }
        if (!mTriangles.IsNullOrEmpty())
        {
            mTriangles.Clear();
        }
        if (!mBasicEdges.IsNullOrEmpty())
        {
            mBasicEdges.Clear();
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

        foreach (BasicTriangle basicTriangle in mBasicTriangles)
        {
            for (int i = 0; i < 3; i++)
            {
                Debug.Assert(exchangedVertices.TryGetValue(basicTriangle.v[i], out basicTriangle.v[i]));
            }
        }

        mDataCloud = newDataCloud;
    }


    public Mesh GetMesh()
    {
        MeshData meshData = new MeshData(true);

        foreach (BasicTriangle basicTriangle in mBasicTriangles)
        {
            meshData.AddTriangles(mDataCloud[basicTriangle.v[0]], mDataCloud[basicTriangle.v[1]], mDataCloud[basicTriangle.v[2]]);
        }

        return meshData.GetMesh();
    }
}