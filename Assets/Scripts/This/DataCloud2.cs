using UnityEngine;
using System.Collections.Generic;

public static class DataCloud2
{
    public static List<Vector3> Rectangle(int Count, Vector3 Extents, Vector3 Origin)
    {
        List<Vector3> result = new List<Vector3>(Count);

        for (int i = 0; i < Count; i++)
        {
            result.Add(new Vector3(
                Random.Range(Origin.x - Extents.x, Origin.x + Extents.x),
                Random.Range(Origin.y - Extents.y, Origin.y + Extents.y),
                Origin.z
            ));
        }

        return result;
    }


    public static List<Vector3> Ellipse(int Count, Vector3 Extents, Vector3 Origin)
    {
        List<Vector3> result = new List<Vector3>(Count);

        Vector2 reverseExtents = new Vector3(1f / Extents.x, 1f / Extents.y);

        int innerCount = 0;
        while (innerCount < Count)
        {
            float x = Random.Range(-Extents.x, Extents.x);
            float y = Random.Range(-Extents.y, Extents.y);

            float dx = x * reverseExtents.x;
            float dy = y * reverseExtents.y;

            if (dx * dx + dy * dy <= 1)
            {
                result.Add(new Vector3(x, y, 0f) + Origin);
                innerCount++;
            }
        }

        return result;
    }
}