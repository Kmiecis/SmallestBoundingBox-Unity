using UnityEngine;

public class OBB2
{
    ConvexHull2 mConvexHull2;
    public ConvexHull2 ConvexHull2Data
    {
        set { mConvexHull2 = value; }
    }

    Rectangle mRectangle;
    public Rectangle Rectangle
    {
        get { return mRectangle; }
    }

    float mArea;
    public float Area
    {
        get { return mArea; }
    }


    public OBB2(ConvexHull2 convexHull2Data)
    {
        mConvexHull2 = convexHull2Data;
        mRectangle = new Rectangle();
    }


    public void AlgorithmRotatingCalipers()
    {
        var edges = mConvexHull2.Edges;
        var dataCloud = mConvexHull2.DataCloud;
        var vertices = mConvexHull2.Vertices;

        if (edges.Count < 3)
        {
            Debugger.Get.Log("Insufficient points to form a triangle.", DebugOption.SBB2);
            return;
        }

        mArea = float.MaxValue;

        foreach (Edge edge in edges)
        {
            Vector3 edgeDirection = edge.V[1] - edge.V[0];
            float edgeAngle = Mathf.Atan2(edgeDirection.y, edgeDirection.x);

            // x = min, y = max
            MinMaxPair X = new MinMaxPair();
            MinMaxPair Y = new MinMaxPair();

            foreach (int p in vertices)
            {
                Vector3 rotatedPoint = ExtMathf.Rotation2D(dataCloud[p], -edgeAngle);

                X.CheckAgainst(rotatedPoint.x);
                Y.CheckAgainst(rotatedPoint.y);
            }

            float newArea = X.Delta * Y.Delta;
            if (newArea < mArea)
            {
                mArea = newArea;

                mRectangle.Extents = new Vector2(X.Delta, Y.Delta);
                mRectangle.Corner = ExtMathf.Rotation2D(new Vector3(X.Min, Y.Min), edgeAngle);
                mRectangle.X = ExtMathf.Rotation2D(Vector3.right, edgeAngle).normalized;
                mRectangle.Y = ExtMathf.Rotation2D(Vector3.up, edgeAngle).normalized;
            }
        }
    }
}