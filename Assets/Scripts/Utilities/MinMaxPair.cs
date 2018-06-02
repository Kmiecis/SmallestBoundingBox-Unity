using UnityEngine;

public class MinMaxPair
{
    public float Min { get; set; }
    public float Max { get; set; }

    public float Delta { get { return Max - Min; } }


    public MinMaxPair()
    {
        Min = float.MaxValue;
        Max = float.MinValue;
    }


    public void CheckAgainst(float value)
    {
        Min = Mathf.Min(Min, value);
        Max = Mathf.Max(Max, value);
    }
}