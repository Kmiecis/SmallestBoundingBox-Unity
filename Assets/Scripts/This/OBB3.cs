using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class OBB3
{
    ConvexHull3 mConvexHull3;
    public ConvexHull3 ConvexHull3Data
    {
        get { return mConvexHull3; }
        set { mConvexHull3 = value; }
    }

    Box mBox;
    public Box Box
    {
        get { return mBox; }
    }

    float mVolume;
    public float Volume
    {
        get { return mVolume; }
    }


    public OBB3(ConvexHull3 convexHull3Data)
    {
        mConvexHull3 = convexHull3Data;
        mBox = new Box();
    }


    public void AlgorithmOptimized()
    {
        /* Outline of the algorithm:
          0. Compute the convex hull of the point set (given as input to this function) O(VlogV)
          1. Compute vertex adjacency data, i.e. given a vertex, return a list of its neighboring vertices. O(V)
          2. Precompute face normal direction vectors, since these are needed often. (does not affect big-O complexity, just a micro-opt) O(F)
          3. Compute edge adjacency data, i.e. given an edge, return the two indices of its neighboring faces. O(V)
          4. Precompute antipodal vertices for each edge. O(A*ElogV), where A is the size of antipodal vertices per edge. A ~ O(1) on average.
          5. Precompute all sidepodal edges for each edge. O(E*S), where S is the size of sidepodal edges per edge. S ~ O(sqrtE) on average.
             - Sort the sidepodal edges to a linear order so that it's possible to do fast set intersection computations on them. O(E*S*logS), or O(E*sqrtE*logE).
          6. Test all configurations where all three edges are on adjacent faces. O(E*S^2) = O(E^2) or if smart with graph search, O(ES) = O(E*sqrtE)?
          7. Test all configurations where two edges are on opposing faces, and the third one is on a face adjacent to the two. O(E*sqrtE*logV)?
          8. Test all configurations where two edges are on the same face (OBB aligns with a face of the convex hull). O(F*sqrtE*logV).
          9. Return the best found OBB.
        */
        
        mVolume = float.MaxValue;

        var dataCloud = mConvexHull3.DataCloud;
        var basicTriangles = mConvexHull3.BasicTriangles;
        var edges = mConvexHull3.BasicEdges;
        var vertices = mConvexHull3.Vertices;
        int verticesCount = vertices.Count;

        if (verticesCount < 4)
            return;

        // Precomputation: For each vertex in the convex hull, compute their neighboring vertices.
        var adjacencyData = mConvexHull3.GetVertexAdjacencyData(); // O(V)

        // Precomputation: for each triangle create and store its normal, for later use.
        List<Vector3> faceNormals = new List<Vector3>(basicTriangles.Count);
        foreach (BasicTriangle basicTriangle in basicTriangles) // O(F)
        {
            Triangle triangle = Triangle.FromBasicTriangle(basicTriangle, dataCloud);
            faceNormals.Add(triangle.N);
        }

        // Precomputation: For each edge in convex hull, compute both connected faces data.
        Dictionary<int, List<int>> facesForEdge = mConvexHull3.GetEdgeFacesData(); // edgeFaces

        HashSet<int> internalEdges = new HashSet<int>();
        Func<int, bool> IsInternalEdge = (int edgeIndex) => { return internalEdges.Contains(edgeIndex); };

        for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
        {
            float dot = Vector3.Dot(
                faceNormals[facesForEdge[edgeIndex].First()],
                faceNormals[facesForEdge[edgeIndex].Second()]
            );

            bool isInternal = ExtMathf.Equal(dot, 1f, 1e-4f);

            if (isInternal)
                internalEdges.Add(edgeIndex);
        }

        // For each vertex pair (v, u) through which there is an edge, specifies the index i of the edge that passes through them.
        // This map contains duplicates, so both (v, u) and (u, v) map to the same edge index.
        List<int> vertexPairToEdges = mConvexHull3.GetUniversalEdgesList();

        // Throughout the whole algorithm, this array stores an auxiliary structure for performing graph searches
        // on the vertices of the convex hull. Conceptually each index of the array stores a boolean whether we
        // have visited that vertex or not during the current search. However storing such booleans is slow, since
        // we would have to perform a linear-time scan through this array before next search to reset each boolean
        // to unvisited false state. Instead, store a number, called a "color" for each vertex to specify whether
        // that vertex has been visited, and manage a global color counter floodFillVisitColor that represents the
        // visited vertices. At any given time, the vertices that have already been visited have the value
        // floodFillVisited[i] == floodFillVisitColor in them. This gives a win that we can perform constant-time
        // clears of the floodFillVisited array, by simply incrementing the "color" counter to clear the array.

        List<int> floodFillVisited = new List<int>().Populate(-1, verticesCount);
        int floodFillVisitColor = 1;

        Action ClearGraphSearch = () => { floodFillVisitColor++; };
        Action<int> MarkVertexVisited = (int v) => { floodFillVisited[v] = floodFillVisitColor; };
        Func<int, bool> HaveVisitedVertex = (int v) => { return floodFillVisited[v] == floodFillVisitColor; };

        List<List<int>> antipodalPointsForEdge = new List<List<int>>().Populate(new List<int>(), edges.Count); // edgeAntipodalVertices

        // The currently best variant for establishing a spatially coherent traversal order.
        HashSet<int> spatialFaceOrder = new HashSet<int>();
        HashSet<int> spatialEdgeOrder = new HashSet<int>();

        { // Explicit scope for variables that are not needed after this.
            List<Pair<int, int>> traverseStackEdge = new List<Pair<int, int>>();
            List<bool> visitedEdges = new List<bool>().Populate(false, edges.Count);
            List<bool> visitedFaces = new List<bool>().Populate(false, basicTriangles.Count);

            traverseStackEdge.Add(new Pair<int, int>(0, adjacencyData[0].First()));
            while (traverseStackEdge.Count != 0)
            {
                Pair<int, int> e = traverseStackEdge.Last();
                traverseStackEdge.RemoveLast();
                
                int thisEdge = vertexPairToEdges[e.First * verticesCount + e.Second];

                if (visitedEdges[thisEdge])
                    continue;
                visitedEdges[thisEdge] = true;

                if (!visitedFaces[facesForEdge[thisEdge].First()])
                {
                    visitedFaces[facesForEdge[thisEdge].First()] = true;
                    spatialFaceOrder.Add(facesForEdge[thisEdge].First());
                }

                if (!visitedFaces[facesForEdge[thisEdge].Second()])
                {
                    visitedFaces[facesForEdge[thisEdge].Second()] = true;
                    spatialFaceOrder.Add(facesForEdge[thisEdge].Second());
                }

                if (!IsInternalEdge(thisEdge))
                    spatialEdgeOrder.Add(thisEdge);

                int v0 = e.Second;
                int sizeBefore = traverseStackEdge.Count;

                foreach (int v1 in adjacencyData[v0])
                {
                    int e1 = vertexPairToEdges[v0 * verticesCount + v1];

                    if (visitedEdges[e1])
                        continue;

                    traverseStackEdge.Add(new Pair<int, int>(v0, v1));
                }

                //Take a random adjacent edge
                int nNewEdges = (traverseStackEdge.Count - sizeBefore);
                if (nNewEdges > 0)
                {
                    int r = UnityEngine.Random.Range(0, nNewEdges - 1);

                    var swapTemp = traverseStackEdge[traverseStackEdge.Count - 1];
                    traverseStackEdge[traverseStackEdge.Count - 1] = traverseStackEdge[sizeBefore + r];
                    traverseStackEdge[sizeBefore + r] = swapTemp;
                }
            }
        }

        // Stores a memory of yet unvisited vertices for current graph search.
        List<int> traverseStack = new List<int>();

        // Since we do several extreme vertex searches, and the search directions have a lot of spatial locality,
        // always start the search for the next extreme vertex from the extreme vertex that was found during the
        // previous iteration for the previous edge. This has been profiled to improve overall performance by as
        // much as 15-25%.

        int debugCount = 0;
        var startPoint = TimeMeasure.Start();

        // Precomputation: for each edge, we need to compute the list of potential antipodal points (points on
        // the opposing face of an enclosing OBB of the face that is flush with the given edge of the polyhedron).
        // This is O(E*log(V)) ?
        foreach (int i in spatialEdgeOrder)
        {
            Vector3 f1a = faceNormals[facesForEdge[i].First()];
            Vector3 f1b = faceNormals[facesForEdge[i].Second()];

            MinMaxPair breadthData;
            int startingVertex = mConvexHull3.GetAlongAxisData(f1a, out breadthData);
            ClearGraphSearch(); // Search through the graph for all adjacent antipodal vertices.

            traverseStack.Add(startingVertex);
            MarkVertexVisited(startingVertex);
            while (!traverseStack.Empty())
            {   // In amortized analysis, only a constant number of vertices are antipodal points for any edge?
                int v = traverseStack.Last();
                traverseStack.RemoveLast();

                var neighbours = adjacencyData[v];
                if (IsVertexAntipodalToEdge(v, neighbours, f1a, f1b))
                {
                    if (edges[i].ContainsVertex(v))
                    {
                        Debug.LogFormat("Edge {0} is antipodal to vertex {1} which is part of the same edge!" +
                            "This should be possible only if the input is degenerate planar!",
                            edges[i], v);
                        
                        return;
                    }

                    antipodalPointsForEdge[i].Add(v);
                    foreach (int neighboursVertex in neighbours)
                    {
                        if (!HaveVisitedVertex(neighboursVertex))
                        {
                            traverseStack.Add(neighboursVertex);
                            MarkVertexVisited(neighboursVertex);
                        }
                        debugCount++;
                    }
                }
                debugCount++;
            }

            // Robustness: If the above search did not find any antipodal points, add the first found extreme
            // point at least, since it is always an antipodal point. This is known to occur very rarely due
            // to numerical imprecision in the above loop over adjacent edges.
            if (antipodalPointsForEdge[i].Empty())
            {
                Debug.LogFormat("Got to place which is most likely a bug...");
                // Fall back to linear scan, which is very slow.
                for (int j = 0; j < verticesCount; j++)
                    if (IsVertexAntipodalToEdge(j, adjacencyData[j], f1a, f1b))
                        antipodalPointsForEdge[i].Add(j);
            }

            debugCount++;
        }

        Debug.Log("@ First loop count: " + debugCount + " in time: " + TimeMeasure.Stop(startPoint));

        // Stores for each edge i the list of all sidepodal edge indices j that it can form an OBB with.
        List<HashSet<int>> compatibleEdges = new List<HashSet<int>>().Populate(new HashSet<int>(), edges.Count);

        // Use a O(E*V) data structure for sidepodal vertices.
        bool[] sidepodalVertices = new bool[edges.Count * verticesCount].Populate(false);

        // Compute all sidepodal edges for each edge by performing a graph search. The set of sidepodal edges is
        // connected in the graph, which lets us avoid having to iterate over each edge pair of the convex hull.
        // Total running time is O(E*sqrtE).
        debugCount = 0;
        startPoint = TimeMeasure.Start();
        foreach (int i in spatialEdgeOrder)
        {
            Vector3 f1a = faceNormals[facesForEdge[i].First()];
            Vector3 f1b = faceNormals[facesForEdge[i].Second()];

            Vector3 deadDirection = (f1a + f1b) * .5f;
            Vector3 basis1, basis2;
            deadDirection.PerpendicularBasis(out basis1, out basis2);

            Vector3 dir = f1a.Perpendicular();

            MinMaxPair breadthData;
            int startingVertex = mConvexHull3.GetAlongAxisData(-dir, out breadthData);
            ClearGraphSearch();

            traverseStack.Add(startingVertex);
            while (!traverseStack.Empty()) // O(sqrt(E))
            {
                int v = traverseStack.Last();
                traverseStack.RemoveLast();

                if (HaveVisitedVertex(v))
                    continue;
                MarkVertexVisited(v);

                foreach (int vAdj in adjacencyData[v])
                {
                    if (HaveVisitedVertex(vAdj))
                        continue;

                    int edge = vertexPairToEdges[v * verticesCount + vAdj];
                    if (AreEdgesCompatibleForOBB(f1a, f1b, faceNormals[facesForEdge[edge].First()], faceNormals[facesForEdge[edge].Second()]))
                    {
                        if (i <= edge)
                        {
                            if (!IsInternalEdge(edge))
                                compatibleEdges[i].Add(edge);

                            sidepodalVertices[i * verticesCount + edges[edge].v[0]] = true;
                            sidepodalVertices[i * verticesCount + edges[edge].v[1]] = true;

                            if (i != edge)
                            {
                                if (!IsInternalEdge(edge))
                                    compatibleEdges[edge].Add(i);

                                sidepodalVertices[edge * verticesCount + edges[i].v[0]] = true;
                                sidepodalVertices[edge * verticesCount + edges[i].v[1]] = true;
                            }
                        }

                        traverseStack.Add(vAdj);
                    }

                    debugCount++;
                }

                debugCount++;
            }

            debugCount++;
        }

        Debug.Log("@ Second loop count: " + debugCount + " in time: " + TimeMeasure.Stop(startPoint));

        // Take advantage of spatial locality: start the search for the extreme vertex from the extreme vertex
        // that was found during the previous iteration for the previous edge. This speeds up the search since
        // edge directions have some amount of spatial locality and the next extreme vertex is often close
        // to the previous one. Track two hint variables since we are performing extreme vertex searches to
        // two opposing directions at the same time.

        // Stores a memory of yet unvisited vertices that are common sidepodal vertices to both currently chosen edges for current graph search.
        List<int> traverseStackCommonSidepodals = new List<int>();
        Debug.Log("@ Edges count: " + edges.Count);
        Debug.Log("@ Vertices count: " + verticesCount);
        debugCount = 0;
        startPoint = TimeMeasure.Start();
        Debug.Log("@ Spatial edges: " + spatialEdgeOrder.Count);
        foreach (int i in spatialEdgeOrder)
        {
            Vector3 f1a = faceNormals[facesForEdge[i].First()];
            Vector3 f1b = faceNormals[facesForEdge[i].Second()];

            Vector3 deadDirection = (f1a + f1b) * .5f;

            Debug.Log("@ Compatible edges: " + compatibleEdges[i].Count);
            foreach (int edgeJ in compatibleEdges[i])
            {
                if (edgeJ <= i) // Remove symmetry.
                    continue;

                Vector3 f2a = faceNormals[facesForEdge[edgeJ].First()];
                Vector3 f2b = faceNormals[facesForEdge[edgeJ].Second()];

                Vector3 deadDirection2 = (f2a + f2b) * .5f;

                Vector3 searchDir = Vector3.Cross(deadDirection, deadDirection2).normalized;
                float length = searchDir.magnitude;

                if (ExtMathf.Equal(length, 0f))
                {
                    searchDir = Vector3.Cross(f1a, f2a).normalized;
                    length = searchDir.magnitude;

                    if (ExtMathf.Equal(length, 0f))
                        searchDir = f1a.Perpendicular();
                }

                ClearGraphSearch();
                MinMaxPair breadthData;
                int extremeVertexSearchHint1 = mConvexHull3.GetAlongAxisData(-searchDir, out breadthData);

                ClearGraphSearch();
                MinMaxPair breadthData2;
                int extremeVertexSearchHint2 = mConvexHull3.GetAlongAxisData(searchDir, out breadthData2);

                ClearGraphSearch();

                int secondSearch = -1;

                if (sidepodalVertices[edgeJ * verticesCount + extremeVertexSearchHint1])
                    traverseStackCommonSidepodals.Add(extremeVertexSearchHint1);
                else
                    traverseStack.Add(extremeVertexSearchHint1);

                if (sidepodalVertices[edgeJ * verticesCount + extremeVertexSearchHint2])
                    traverseStackCommonSidepodals.Add(extremeVertexSearchHint2);
                else
                    secondSearch = extremeVertexSearchHint2;

                // Bootstrap to a good vertex that is sidepodal to both edges.
                ClearGraphSearch();
                while (!traverseStack.Empty())
                {
                    int v = traverseStack.First();
                    traverseStack.RemoveFirst();

                    if (HaveVisitedVertex(v))
                        continue;
                    MarkVertexVisited(v);

                    foreach (int vAdj in adjacencyData[v])
                    {
                        if (!HaveVisitedVertex(vAdj) && sidepodalVertices[i * verticesCount + vAdj])
                        {
                            if (sidepodalVertices[edgeJ * verticesCount + vAdj])
                            {
                                traverseStack.Clear();
                                if (secondSearch != -1)
                                {
                                    traverseStack.Add(secondSearch);
                                    secondSearch = -1;
                                    MarkVertexVisited(vAdj);
                                }

                                traverseStackCommonSidepodals.Add(vAdj);
                                break;
                            }
                            else
                            {
                                traverseStack.Add(vAdj);
                            }
                        }

                        debugCount++;
                    }

                    debugCount++;
                }

                ClearGraphSearch();
                while (!traverseStackCommonSidepodals.Empty())
                {
                    int v = traverseStackCommonSidepodals.Last();
                    traverseStackCommonSidepodals.RemoveLast();

                    if (HaveVisitedVertex(v))
                        continue;
                    MarkVertexVisited(v);

                    foreach (int vAdj in adjacencyData[v])
                    {
                        int edgeK = vertexPairToEdges[v * verticesCount + vAdj];

                        if (IsInternalEdge(edgeK))  // Edges inside faces with 180 degrees dihedral angles can be ignored.
                            continue;

                        if (sidepodalVertices[i * verticesCount + vAdj] && sidepodalVertices[edgeJ * verticesCount + vAdj])
                        {
                            if (!HaveVisitedVertex(vAdj))
                                traverseStackCommonSidepodals.Add(vAdj);

                            if (edgeJ < edgeK)
                            {
                                Vector3 f3a = faceNormals[facesForEdge[edgeK].First()];
                                Vector3 f3b = faceNormals[facesForEdge[edgeK].Second()];

                                Vector3[] n1 = new Vector3[2];
                                Vector3[] n2 = new Vector3[2];
                                Vector3[] n3 = new Vector3[2];

                                int nSolutions = ComputeBasis(f1a, f1b, f2a, f2b, f3a, f3b, ref n1, ref n2, ref n3);
                                for (int s = 0; s < nSolutions; s++)
                                {
                                    BoxFromNormalAxes(n1[s], n2[s], n3[s]);
                                }
                            }
                        }

                        debugCount++;
                    }

                    debugCount++;
                }

                debugCount++;
            }

            debugCount++;
        }

        Debug.Log("@ Third loop count: " + debugCount + " in time: " + TimeMeasure.Stop(startPoint));

        HashSet<int> antipodalEdges = new HashSet<int>();
        List<Vector3> antipodalEdgeNormals = new List<Vector3>();

        // Main algorithm body for finding all search directions where the OBB is flush with the edges of the
        // convex hull from two opposing faces. This is O(E*sqrtE*logV)?
        debugCount = 0;
        startPoint = TimeMeasure.Start();
        foreach (int i in spatialEdgeOrder)
        {
            Vector3 f1a = faceNormals[facesForEdge[i].First()];
            Vector3 f1b = faceNormals[facesForEdge[i].Second()];

            antipodalEdges.Clear();
            antipodalEdgeNormals.Clear();

            foreach (int antipodalVertex in antipodalPointsForEdge[i])
            {
                foreach (int vAdj in adjacencyData[antipodalVertex])
                {
                    if (vAdj < antipodalVertex) // We search unordered edges, so no need to process edge (v1, v2) and (v2, v1)
                        continue;               // twice - take the canonical order to be antipodalVertex < vAdj

                    int edge = vertexPairToEdges[antipodalVertex * verticesCount + vAdj];
                    if (i > edge) // We search pairs of edges, so no need to process twice - take the canonical order to be i < edge.
                        continue;

                    if (IsInternalEdge(edge))
                        continue; // Edges inside faces with 180 degrees dihedral angles can be ignored.

                    Vector3 f2a = faceNormals[facesForEdge[edge].First()];
                    Vector3 f2b = faceNormals[facesForEdge[edge].Second()];

                    Vector3 normal;
                    bool success = AreCompatibleOpposingEdges(f1a, f1b, f2a, f2b, out normal);
                    if (success)
                    {
                        antipodalEdges.Add(edge);
                        antipodalEdgeNormals.Add(normal.normalized);
                    }

                    debugCount++;
                }

                debugCount++;
            }

            var compatibleEdgesI = compatibleEdges[i];
            foreach (int edgeJ in compatibleEdgesI)
            {
                int k = 0;
                foreach (int edgeK in antipodalEdges)
                {
                    Vector3 n1 = antipodalEdgeNormals[k++];

                    MinMaxPair N1 = new MinMaxPair();
                    N1.Min = Vector3.Dot(n1, dataCloud[edges[edgeK].v[0]]);
                    N1.Max = Vector3.Dot(n1, dataCloud[edges[i].v[0]]);

                    Debug.Assert(N1.Min < N1.Max);

                    // Test all mutual compatible edges.
                    Vector3 f3a = faceNormals[facesForEdge[edgeJ].First()];
                    Vector3 f3b = faceNormals[facesForEdge[edgeJ].Second()];

                    float num = Vector3.Dot(n1, f3b);
                    float denom = Vector3.Dot(n1, f3b - f3a);

                    RefAction<float, float> MoveSign = (ref float dst, ref float src) =>
                    {
                        if (src < 0f)
                        {
                            dst = -dst;
                            src = -src;
                        }
                    };
                    MoveSign(ref num, ref denom);

                    float epsilon = 1e-4f;
                    if (denom < epsilon)
                    {
                        num = (Mathf.Abs(num) == 0f) ? 0f : -1f;
                        denom = 1f;
                    }

                    if (num >= denom * -epsilon && num <= denom * (1f + epsilon))
                    {
                        float v = num / denom;

                        Vector3 n3 = (f3b + (f3a - f3b) * v).normalized;
                        Vector3 n2 = Vector3.Cross(n3, n1).normalized;

                        BoxFromNormalAxes(n1, n2, n3);
                    }

                    debugCount++;
                }

                debugCount++;
            }

            debugCount++;
        }

        Debug.Log("@ Fourth loop count: " + debugCount + " in time: " + TimeMeasure.Stop(startPoint));

        // Main algorithm body for computing all search directions where the OBB touches two edges on the same face.
        // This is O(F*sqrtE*logV)?
        debugCount = 0;
        startPoint = TimeMeasure.Start();
        foreach (int i in spatialFaceOrder)
        {
            Vector3 n1 = faceNormals[i];

            // Find two edges on the face. Since we have flexibility to choose from multiple edges of the same face,
            // choose two that are possibly most opposing to each other, in the hope that their sets of sidepodal
            // edges are most mutually exclusive as possible, speeding up the search below.
            int e1 = -1;
            int v0 = basicTriangles[i].v[2];

            for (int j = 0; j < basicTriangles[i].v.Length; j++)
            {
                int v1 = basicTriangles[i].v[j];
                int e = vertexPairToEdges[v0 * verticesCount + v1];
                if (!IsInternalEdge(e))
                {
                    e1 = e;
                    break;
                }
                v0 = v1;
            }

            if (e1 == -1)
                continue; // All edges of this face were degenerate internal edges! Just skip processing the whole face.

            var compatibleEdgesI = compatibleEdges[e1];
            foreach (int edge3 in compatibleEdgesI)
            {
                Vector3 f3a = faceNormals[facesForEdge[edge3].First()];
                Vector3 f3b = faceNormals[facesForEdge[edge3].Second()];

                float num = Vector3.Dot(n1, f3b);
                float denom = Vector3.Dot(n1, f3b - f3a);
                float v;
                if (!ExtMathf.Equal(Mathf.Abs(denom), 0f))
                    v = num / denom;
                else
                    v = ExtMathf.Equal(Mathf.Abs(num), 0f) ? 0f : -1f;

                const float epsilon = 1e-4f;
                if (v >= 0f - epsilon && v <= 1f + epsilon)
                {
                    Vector3 n3 = (f3b + (f3a - f3b) * v).normalized;
                    Vector3 n2 = Vector3.Cross(n3, n1).normalized;

                    BoxFromNormalAxes(n1, n2, n3);
                }

                debugCount++;
            }

            debugCount++;
        }

        Debug.Log("@ Fifth loop count: " + debugCount + " in time: " + TimeMeasure.Stop(startPoint));
    }


    public void AlgorithmHullFaces()
    {
        var triangles = mConvexHull3.Triangles;
        var dataCloud = mConvexHull3.DataCloud;
        var points = mConvexHull3.Vertices;

        if (triangles.Count < 4)
        {
            Debugger.Get.Log("Insufficient points to form a tetrahedron.", DebugOption.SBB3);
            return;
        }

        mVolume = float.MaxValue;
        foreach (Triangle triangle in triangles)
        {
            List<Vector3> localPoints = new List<Vector3>(triangles.Count * 3);

            float farthestDistance = -float.MaxValue;
            foreach (int point in points)
            {
                Vector3 dataCloudPoint = dataCloud[point];

                float distance = Mathf.Abs(triangle.CalculateDistance(dataCloudPoint));
                farthestDistance = Mathf.Max(farthestDistance, distance);

                localPoints.Add(triangle.ToLocal(dataCloudPoint));
            }

            ConvexHull2 ch2 = new ConvexHull2(localPoints);
            ch2.AlgorithmQuickhull();

            OBB2 obb2 = new OBB2(ch2);
            obb2.AlgorithmRotatingCalipers();

            float newVolume = obb2.Area * farthestDistance;
            if (newVolume < mVolume)
            {
                mVolume = newVolume;

                var worldRect = new {
                    BottomLeftCorner = triangle.ToWorld(obb2.Rectangle.Corner),
                    BottomRightCorner = triangle.ToWorld(obb2.Rectangle.Corner + obb2.Rectangle.X * obb2.Rectangle.Extents.x),
                    TopLeftCorner = triangle.ToWorld(obb2.Rectangle.Corner + obb2.Rectangle.Y * obb2.Rectangle.Extents.y)
                };

                mBox.Extents = new Vector3(obb2.Rectangle.Extents.x, obb2.Rectangle.Extents.y, farthestDistance);
                mBox.Corner = worldRect.BottomLeftCorner;
                mBox.X = (worldRect.BottomRightCorner - mBox.Corner).normalized;
                mBox.Y = (worldRect.TopLeftCorner - mBox.Corner).normalized;
                mBox.Z = -triangle.N;   // Triangle normal always points outside convex hull.
            }
        }
    }


    public void AlgorithmBruteDirection()
    {
        var dataCloud = mConvexHull3.DataCloud;
        var points = mConvexHull3.Vertices;

        const int X = 128;
        const int Y = 128;

        mVolume = float.MaxValue;
        for (int y = 0; y < Y; y++)
        {
            for (int x = 0; x < X; x++)
            {
                float fx = (float)x / (X - 1) * 2f - 1f;
                float fy = (float)y / (Y - 1) * 2f - 1f;

                float sqLength = fx * fx + fy * fy;
                if (sqLength > 1f)
                    continue;

                float fz = Mathf.Sqrt(1f - sqLength);

                Vector3 zAxis = new Vector3(fx, fy, fz);
                Vector3 xAxis;
                Vector3 yAxis;
                ExtMathf.AxesFromAxis(zAxis, out xAxis, out yAxis);

                BoxFromNormalAxes(xAxis, yAxis, zAxis);
            }
        }
    }


    public void AlgorithmBruteQuaternion()
    {
        var dataCloud = mConvexHull3.DataCloud;
        var points = mConvexHull3.Vertices;

        mVolume = float.MaxValue;

        const int S = 32;
        float inc = 360f / S;

        float Z = 0f;
        for (int z = 0; z < S; z++)
        {
            float Y = 0f;
            for (int y = 0; y < S; y++)
            {
                float X = 0f;
                for (int x = 0; x < S; x++)
                {
                    Quaternion Q = Quaternion.Euler(X, Y, Z);

                    Matrix4x4 M = Matrix4x4.Rotate(Q);

                    Vector3 xAxis = M.GetColumn(0);
                    xAxis.Normalize();
                    Vector3 yAxis = M.GetColumn(1);
                    yAxis.Normalize();
                    Vector3 zAxis = M.GetColumn(2);
                    zAxis.Normalize();

                    BoxFromNormalAxes(xAxis, yAxis, zAxis);

                    X += inc;
                }

                Y += inc;
            }

            Z += inc;
        }
    }


    public void AlgorithmAABB()
    {
        var dataCloud = mConvexHull3.DataCloud;
        var points = mConvexHull3.Vertices;

        MinMaxPair X = new MinMaxPair();
        MinMaxPair Y = new MinMaxPair();
        MinMaxPair Z = new MinMaxPair();

        foreach (int point in points)
        {
            Vector3 dataCloudPoint = dataCloud[point];

            X.CheckAgainst(dataCloudPoint.x);
            Y.CheckAgainst(dataCloudPoint.y);
            Z.CheckAgainst(dataCloudPoint.z);
        }

        mVolume = X.Delta * Y.Delta * Z.Delta;

        mBox.Extents = new Vector3(X.Delta, Y.Delta, Z.Delta);
        mBox.Corner = new Vector3(X.Min, Y.Min, Z.Min);
        mBox.X = Vector3.right;
        mBox.Y = Vector3.up;
        mBox.Z = Vector3.forward;
    }


    bool IsVertexAntipodalToEdge(int vertexIndex, HashSet<int> neighbours, Vector3 normalA, Vector3 normalB)
    {
        float tMin = 0f;
        float tMax = 1f;

        var dataCloud = mConvexHull3.DataCloud;

        Vector3 vertex = dataCloud[vertexIndex];
        Vector3 normalDirection = normalB - normalA;

        foreach (int neighbour in neighbours)
        {
            Vector3 edgeDirection = dataCloud[neighbour] - vertex;

            float s = Vector3.Dot(normalDirection, edgeDirection);
            float n = Vector3.Dot(normalB, edgeDirection);

            const float epsilon = 1e-4f;
            if (s > epsilon)
                tMax = Mathf.Min(tMax, n / s);
            else if (s < -epsilon)
                tMin = Mathf.Max(tMin, n / s);
            else if (n < -epsilon)
                return false;

            // The interval of possible solutions for t is now degenerate?
            if (tMax - tMin < -5e-2f) // -1e-3f has been seen to be too strict here.
                return false;
        }

        return true;
    }


    bool AreEdgesCompatibleForOBB(Vector3 f1a, Vector3 f1b, Vector3 f2a, Vector3 f2b)
    {
        Vector3 f1Dir = f1a - f1b;
        Vector3 f2Dir = f2a - f2b;

        float a = Vector3.Dot(f1b, f2b);
        float b = Vector3.Dot(f1Dir, f2b);
        float c = Vector3.Dot(f2Dir, f1b);
        float d = Vector3.Dot(f1Dir, f2Dir);

        float ab = a + b;
        float ac = a + c;
        float abcd = ab + c + d;
        float minVal = Mathf.Min(a, ab, ac, abcd);
        float maxVal = Mathf.Max(a, ab, ac, abcd);

        return minVal <= 0f && maxVal >= 0f;
    }


    bool AreCompatibleOpposingEdges(Vector3 f1a, Vector3 f1b, Vector3 f2a, Vector3 f2b, out Vector3 n)
    {
        const float tooCloseToFaceEpsilon = 1e-4f;

        Matrix4x4 A = new Matrix4x4();
        A.SetColumn(0, f2b); // c
        A.SetColumn(1, (f1a - f1b)); // t
        A.SetColumn(2, (f2a - f2b)); // r = c*u

        Vector3 x = Vector3.zero;
        bool success = A.SolveAxb(-f1b, ref x);
        float c = x.x;
        float t = x.y;
        float cu = x.z;

        n = Vector3.zero;

        if (!success || c <= 0f || t < 0f || t > 1f)
            return false;

        float u = cu / c;
        if (t < tooCloseToFaceEpsilon || t > 1f - tooCloseToFaceEpsilon
            || u < tooCloseToFaceEpsilon || u > 1f - tooCloseToFaceEpsilon)
            return false;

        if (cu < 0f || cu > c)
            return false;

        n = f1b + (f1a - f1b) * t;
        return true;
    }


    int ComputeBasis(Vector3 f1a, Vector3 f1b, Vector3 f2a, Vector3 f2b, Vector3 f3a, Vector3 f3b,
        ref Vector3[] n1, ref Vector3[] n2, ref Vector3[] n3)
    {
        const float eps = 1e-4f;
        const float angleEps = 1e-3f;

        Vector3 a = f1b;
        Vector3 b = f1a - f1b;
        Vector3 c = f2b;
        Vector3 d = f2a - f2b;
        Vector3 e = f3b;
        Vector3 f = f3a - f3b;

        float g = Vector3.Dot(a, c) * Vector3.Dot(d, e) - Vector3.Dot(a, d) * Vector3.Dot(c, e);
        float h = Vector3.Dot(a, c) * Vector3.Dot(d, f) - Vector3.Dot(a, d) * Vector3.Dot(c, f);
        float i = Vector3.Dot(b, c) * Vector3.Dot(d, e) - Vector3.Dot(b, d) * Vector3.Dot(c, e);
        float j = Vector3.Dot(b, c) * Vector3.Dot(d, f) - Vector3.Dot(b, d) * Vector3.Dot(c, f);

        float k = g * Vector3.Dot(b, e) - Vector3.Dot(a, e) * i;
        float l = h * Vector3.Dot(b, e) + g * Vector3.Dot(b, f) - Vector3.Dot(a, f) * i - Vector3.Dot(a, e) * j;
        float m = h * Vector3.Dot(b, f) - Vector3.Dot(a, f) * j;

        float s = l * l - 4 * m * k;

        if (Mathf.Abs(m) < 1e-5f || Mathf.Abs(s) < 1e-5f)
        {   // The equation is linear.
            float v = -k / l;
            float t = -(g + h * v) / (i + j * v);
            float u = -(Vector3.Dot(c, e) + Vector3.Dot(c, f) * v) / (Vector3.Dot(d, e) + Vector3.Dot(d, f) * v);

            int solutions = 0;
            // If we happened to divide by zero above, the following checks handle them.
            if (v >= -eps && t >= -eps && u >= -eps && v <= 1f + eps && t <= 1f + eps && u <= 1f + eps)
            {
                n1[0] = (a + b * t).normalized;
                n2[0] = (c + d * u).normalized;
                n3[0] = (e + f * v).normalized;

                if (
                    Mathf.Abs(Vector3.Dot(n1[0], n2[0])) < angleEps &&
                    Mathf.Abs(Vector3.Dot(n1[0], n3[0])) < angleEps &&
                    Mathf.Abs(Vector3.Dot(n2[0], n3[0])) < angleEps
                    )
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            return solutions;
        }

        if (s < 0f) // Discriminant negative, no solutions for v.
            return 0;

        float sgnL = l < 0 ? -1f : 1f;
        float V1 = -(l + sgnL * Mathf.Sqrt(s)) / (2f * m);
        float V2 = k / (m * V1);

        float T1 = -(g + h * V1) / (i + j * V1);
        float T2 = -(g + h * V2) / (i + j * V2);

        float U1 = -(Vector3.Dot(c, e) + Vector3.Dot(c, f) * V1) / (Vector3.Dot(d, e) + Vector3.Dot(d, f) * V1);
        float U2 = -(Vector3.Dot(c, e) + Vector3.Dot(c, f) * V2) / (Vector3.Dot(d, e) + Vector3.Dot(d, f) * V2);

        int nSolutions = 0;
        if (V1 >= -eps && T1 >= -eps && U1 >= -eps && V1 <= 1f + eps && T1 <= 1f + eps && U1 <= 1f + eps)
        {
            n1[nSolutions] = (a + b * T1).normalized;
            n2[nSolutions] = (c + d * U1).normalized;
            n3[nSolutions] = (e + f * V1).normalized;

            if (Mathf.Abs(Vector3.Dot(n1[nSolutions], n2[nSolutions])) < angleEps
                && Mathf.Abs(Vector3.Dot(n1[nSolutions], n3[nSolutions])) < angleEps
                && Mathf.Abs(Vector3.Dot(n2[nSolutions], n3[nSolutions])) < angleEps
            )
            {
                ++nSolutions;
            }
        }
        if (V2 >= -eps && T2 >= -eps && U2 >= -eps && V2 <= 1f + eps && T2 <= 1f + eps && U2 <= 1f + eps)
        {
            n1[nSolutions] = (a + b * T2).normalized;
            n2[nSolutions] = (c + d * U2).normalized;
            n3[nSolutions] = (e + f * V2).normalized;
            if (Mathf.Abs(Vector3.Dot(n1[nSolutions], n2[nSolutions])) < angleEps
                && Mathf.Abs(Vector3.Dot(n1[nSolutions], n3[nSolutions])) < angleEps
                && Mathf.Abs(Vector3.Dot(n2[nSolutions], n3[nSolutions])) < angleEps
            )
            {
                ++nSolutions;
            }
        }
        if (s < eps && nSolutions == 2)
            nSolutions = 1;

        return nSolutions;
    }


    bool BoxFromNormalAxes (Vector3 xAxis, Vector3 yAxis, Vector3 zAxis) // TestThreeAdjacentFaces
    {
        Pair<int, MinMaxPair> xAxisData;
        Pair<int, MinMaxPair> yAxisData;
        Pair<int, MinMaxPair> zAxisData;
        mConvexHull3.GetAlongAxesData(xAxis, yAxis, zAxis, out xAxisData, out yAxisData, out zAxisData);

        float newVolume = xAxisData.Second.Delta * yAxisData.Second.Delta * zAxisData.Second.Delta;
        if (newVolume < mVolume)
        {
            mVolume = newVolume;

            mBox.Corner = mConvexHull3.Centroid + (xAxis * xAxisData.Second.Min + yAxis * yAxisData.Second.Min + zAxis * zAxisData.Second.Min);
            mBox.Extents = new Vector3(xAxisData.Second.Delta, yAxisData.Second.Delta, zAxisData.Second.Delta);
            mBox.X = xAxis;
            mBox.Y = yAxis;
            mBox.Z = zAxis;

            return true;
        }

        return false;
    }

    delegate void RefAction<T1, T2>(ref T1 arg1, ref T2 arg2);
}