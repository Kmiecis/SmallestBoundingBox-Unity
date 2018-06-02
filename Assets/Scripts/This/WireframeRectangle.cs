using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WireframeRectangle : MonoBehaviour
{
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    LineRenderer lineRenderer;


    public void FromCorners(Vector3[] corners)
    {
        LoadIfNecessary();

        lineRenderer.positionCount = corners.Length;
        lineRenderer.SetPositions(corners);
    }


    public void FromRectangle(Rectangle rectangle)
    {
        LoadIfNecessary();

        Vector3[] corners = new Vector3[4];
        corners[0] = rectangle.Corner;
        corners[1] = rectangle.Corner + rectangle.Y * rectangle.Extents.y;
        corners[2] = rectangle.Corner + rectangle.X * rectangle.Extents.x + rectangle.Y * rectangle.Extents.y;
        corners[3] = rectangle.Corner + rectangle.X * rectangle.Extents.x;

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

            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            lineRenderer.widthMultiplier = lineWidth;
            lineRenderer.loop = true;
        }

        SetActive(true);
    }



    public void Clear()
    {
        if (loaded)
        {
            lineRenderer.positionCount = 0;
        }

        SetActive(false);
    }


    void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}