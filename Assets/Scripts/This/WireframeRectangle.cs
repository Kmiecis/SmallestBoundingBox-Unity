using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WireframeRectangle : MonoBehaviour
{
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    LineRenderer lineRenderer;


    public void FromSBB2(SmallestBoundingBox2 sbb2)
    {
        LoadIfNecessary();

        Vector3[] corners = sbb2.Corners;

        lineRenderer.positionCount = corners.Length;
        lineRenderer.SetPositions(corners);
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