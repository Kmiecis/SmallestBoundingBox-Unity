using UnityEngine;

public class WireframeCube : MonoBehaviour
{
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    LineRenderer mainLineRenderer;
    public LineRenderer[] helperLineRenderers;


    void FromCorners(Vector3[] corners)
    {
        LoadIfNecessary();

        mainLineRenderer.positionCount = corners.Length;
        mainLineRenderer.SetPositions(corners);
        // 1-4, 2-7, 3-6, 5-8,

        int positions = 2;
        Vector3[] points = new Vector3[positions];

        LineRenderer a = helperLineRenderers[0];
        points[0] = corners[0];
        points[1] = corners[3];
        a.positionCount = positions;
        a.SetPositions(points);

        LineRenderer b = helperLineRenderers[1];
        points[0] = corners[1];
        points[1] = corners[6];
        b.positionCount = positions;
        b.SetPositions(points);

        LineRenderer c = helperLineRenderers[2];
        points[0] = corners[2];
        points[1] = corners[5];
        c.positionCount = positions;
        c.SetPositions(points);

        LineRenderer d = helperLineRenderers[3];
        points[0] = corners[4];
        points[1] = corners[7];
        d.positionCount = positions;
        d.SetPositions(points);
    }


    public void FromBox(Box box)
    {
        LoadIfNecessary();

        Vector3[] corners = new Vector3[8];
        corners[0] = box.Corner;
        corners[1] = box.Corner + box.X * box.Extents.x;
        corners[2] = box.Corner + box.X * box.Extents.x + box.Y * box.Extents.y;
        corners[3] = box.Corner + box.Y * box.Extents.y;
        corners[4] = box.Corner + box.Y * box.Extents.y + box.Z * box.Extents.z;
        corners[5] = box.Corner + box.X * box.Extents.x + box.Y * box.Extents.y + box.Z * box.Extents.z;
        corners[6] = box.Corner + box.X * box.Extents.x + box.Z * box.Extents.z;
        corners[7] = box.Corner + box.Z * box.Extents.z;

        FromCorners(corners);
    }


    bool loaded = false;
    void LoadIfNecessary()
    {
        if (!loaded)
        {
            loaded = true;

            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Particles/Additive"));
            }

            mainLineRenderer = GetComponent<LineRenderer>();

            mainLineRenderer.material = lineMaterial;
            mainLineRenderer.widthMultiplier = lineWidth;
            mainLineRenderer.loop = true;

            foreach (LineRenderer lineRenderer in helperLineRenderers)
            {
                lineRenderer.material = lineMaterial;
                lineRenderer.widthMultiplier = lineWidth;
            }
        }

        SetActive(true);
    }



    public void Clear()
    {
        if (loaded)
        {
            mainLineRenderer.positionCount = 0;

            foreach (LineRenderer lineRenderer in helperLineRenderers)
            {
                lineRenderer.positionCount = 0;
            }
        }

        SetActive(false);
    }


    void SetActive(bool value)
    {
        gameObject.SetActive(value);
        foreach (LineRenderer lineRenderer in helperLineRenderers)
        {
            lineRenderer.gameObject.SetActive(value);
        }
    }
}