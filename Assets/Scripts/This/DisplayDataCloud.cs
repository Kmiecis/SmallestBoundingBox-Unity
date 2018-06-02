using UnityEngine;
using System.Collections.Generic;

public class DisplayDataCloud : MonoBehaviour
{
    [Range(40, 200)]
    public int drawDivider = 40;
    public bool drawDataCloud = false;
    public bool drawEdges = false;
    public bool drawTriangles = false;
    [HideInInspector]
    public List<Vector3> dataCloud;


    private void OnDrawGizmos()
    {
        if (drawDataCloud && !dataCloud.IsNullOrEmpty())
        {
            Gizmos.color = Color.green;
            float radius = Mathf.Log10(dataCloud.Count) / drawDivider;
            foreach (Vector3 point in dataCloud)
            {
                Gizmos.DrawSphere(point, radius);
            }
        }
    }
}
