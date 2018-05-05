using UnityEngine;
using System.Collections.Generic;

public static class DataCloudReductor
{
    public static List<Vector3> Reduce(List<Vector3> dataCloud)
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < dataCloud.Count; i++)
        {
            float sqrDistance = float.MaxValue;

            for (int j = i + 1; j < dataCloud.Count; j++)
            {
                float tempSqrDistance = (dataCloud[i] - dataCloud[j]).sqrMagnitude;
                sqrDistance = Mathf.Min(tempSqrDistance, sqrDistance);
            }

            if (sqrDistance > .01f)
            {
                result.Add(dataCloud[i]);
            }
        }

        return result;
    }
}