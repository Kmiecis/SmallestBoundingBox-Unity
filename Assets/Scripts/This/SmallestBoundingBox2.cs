using UnityEngine;

public class SmallestBoundingBox2
{
    ConvexHull2 m_convexHull2;
    public ConvexHull2 ConvexHull2Data
    {
        set { m_convexHull2 = value; }
    }

    Vector2 m_extents;
    public Vector2 Extents
    {
        get { return m_extents; }
    }

    float m_area;
    public float Area
    {
        get { return m_area; }
    }

    Vector3[] m_corners;
    public Vector3[] Corners
    {
        get { return m_corners; }
    }


    public SmallestBoundingBox2(ConvexHull2 convexHull2Data)
    {
        m_convexHull2 = convexHull2Data;
        m_corners = new Vector3[4];
    }


    public void AlgorithmRotatingCalipers()
    {
        var edges = m_convexHull2.Edges;
        var dataCloud = m_convexHull2.DataCloud;

        if (edges.Count < 3)
        {
            Debugger.Get.Log("Insufficient points to form a triangle.", DebugOption.SBB2);
            return;
        }

        m_area = float.MaxValue;

        foreach (Edge edge in edges)
        {
            Vector3 edgeDirection = edge.V[1] - edge.V[0];
            float edgeAngle = Mathf.Atan2(edgeDirection.y, edgeDirection.x);

            // x = min, y = max
            MinMaxPair X = new MinMaxPair();
            MinMaxPair Y = new MinMaxPair();

            foreach (Vector3 point in dataCloud)
            {
                Vector3 rotatedPoint = ExtMathf.Rotation2D(point, -edgeAngle);

                X.CheckAgainst(rotatedPoint.x);
                Y.CheckAgainst(rotatedPoint.y);
            }

            m_extents.x = X.Delta;
            m_extents.y = Y.Delta;

            float newArea = m_extents.x * m_extents.y;
            if (newArea < m_area)
            {
                m_area = newArea;

                m_corners[0] = ExtMathf.Rotation2D(new Vector3(X.Min, Y.Min), edgeAngle);
                m_corners[1] = ExtMathf.Rotation2D(new Vector3(X.Min, Y.Max), edgeAngle);
                m_corners[2] = ExtMathf.Rotation2D(new Vector3(X.Max, Y.Max), edgeAngle);
                m_corners[3] = ExtMathf.Rotation2D(new Vector3(X.Max, Y.Min), edgeAngle);
            }
        }
    }
}