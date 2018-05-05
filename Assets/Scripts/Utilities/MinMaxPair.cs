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
        if (value < Min) { Min = value; }
        if (value > Max) { Max = value; }
    }
}