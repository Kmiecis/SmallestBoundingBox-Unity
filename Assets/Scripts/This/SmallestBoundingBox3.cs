using UnityEngine;
using System.Collections.Generic;

public class SmallestBoundingBox3
{
    ConvexHull3 m_convexHull3;
    public ConvexHull3 ConvexHull3Data
    {
        set { m_convexHull3 = value; }
    }

    Vector3 m_extents;
    public Vector3 Extents
    {
        get { return m_extents; }
    }

    float m_volume;
    public float Volume
    {
        get { return m_volume; }
    }

    Vector3[] m_corners;
    public Vector3[] Corners
    {
        get { return m_corners; }
    }


    public SmallestBoundingBox3(ConvexHull3 convexHull3Data)
    {
        m_convexHull3 = convexHull3Data;
        m_corners = new Vector3[8];
    }


    public void AlgorithmHullFaces()
    {
        var triangles = m_convexHull3.Triangles;
        var dataCloud = m_convexHull3.DataCloud;
        var ch3points = m_convexHull3.Vertices;

        if (triangles.Count < 4)
        {
            Debugger.Get.Log("Insufficient points to form a tetrahedron.", DebugOption.SBB3);
            return;
        }

        m_volume = float.MaxValue;

        foreach (Triangle triangle in triangles)
        {
            List<Vector3> localPoints = new List<Vector3>(triangles.Count * 3);

            float farthestDistance = -float.MaxValue;
            foreach (int point in ch3points)
            {
                Vector3 dataCloudPoint = dataCloud[point];

                float distance = Mathf.Abs(triangle.CalculateDistance(dataCloudPoint));
                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                }

                localPoints.Add(triangle.ToLocal(dataCloudPoint));
            }

            ConvexHull2 ch2 = new ConvexHull2(localPoints);
            ch2.AlgorithmQuickhull();

            SmallestBoundingBox2 sbb2 = new SmallestBoundingBox2(ch2);
            sbb2.AlgorithmRotatingCalipers();

            float newVolume = sbb2.Area * farthestDistance;
            if (newVolume < m_volume)
            {
                m_volume = newVolume;
                m_extents = new Vector3(sbb2.Extents.x, sbb2.Extents.y, farthestDistance);

                m_corners[0] = triangle.ToWorld(sbb2.Corners[0]);
                m_corners[1] = triangle.ToWorld(sbb2.Corners[3]);
                m_corners[2] = triangle.ToWorld(sbb2.Corners[3]) - triangle.N * farthestDistance;
                m_corners[3] = triangle.ToWorld(sbb2.Corners[0]) - triangle.N * farthestDistance;
                m_corners[4] = triangle.ToWorld(sbb2.Corners[1]) - triangle.N * farthestDistance;
                m_corners[5] = triangle.ToWorld(sbb2.Corners[2]) - triangle.N * farthestDistance;
                m_corners[6] = triangle.ToWorld(sbb2.Corners[2]);
                m_corners[7] = triangle.ToWorld(sbb2.Corners[1]);
            }
        }
    }


    public void AlgorithmAABB()
    {
        var dataCloud = m_convexHull3.DataCloud;

        m_volume = float.MaxValue;

        MinMaxPair X = new MinMaxPair();
        MinMaxPair Y = new MinMaxPair();
        MinMaxPair Z = new MinMaxPair();

        foreach (Vector3 point in dataCloud)
        {
            X.CheckAgainst(point.x);
            Y.CheckAgainst(point.y);
            Z.CheckAgainst(point.z);
        }

        float newVolume = X.Delta * Y.Delta * Z.Delta;
        if (newVolume < m_volume)
        {
            m_volume = newVolume;
            m_extents = new Vector3(X.Delta, Y.Delta, Z.Delta);

            m_corners[0] = new Vector3(X.Min, Y.Min, Z.Min);
            m_corners[1] = new Vector3(X.Max, Y.Min, Z.Min);
            m_corners[2] = new Vector3(X.Max, Y.Max, Z.Min);
            m_corners[3] = new Vector3(X.Min, Y.Max, Z.Min);
            m_corners[4] = new Vector3(X.Min, Y.Max, Z.Max);
            m_corners[5] = new Vector3(X.Max, Y.Max, Z.Max);
            m_corners[6] = new Vector3(X.Max, Y.Min, Z.Max);
            m_corners[7] = new Vector3(X.Min, Y.Min, Z.Max);
        }
    }
}