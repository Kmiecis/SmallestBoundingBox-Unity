public class Pair<T, U>
{
    public T First;
    public U Second;

    public Pair(T first, U second)
    {
        First = first;
        Second = second;
    }

    public override string ToString()
    {
        return string.Format("Pair: {0}, {1}.", First.ToString(), Second.ToString());
    }
}