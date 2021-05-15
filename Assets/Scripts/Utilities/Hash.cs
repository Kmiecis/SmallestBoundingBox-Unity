public static class Hash
{
    public static int GetHashCode<T1, T2>(T1 a, T2 b)
    {
        unchecked
        {
            return 31 * a.GetHashCode() + b.GetHashCode();
        }
    }

    public static int GetHashCode<T1, T2, T3>(T1 a, T2 b, T3 c)
    {
        unchecked
        {
            int hash = a.GetHashCode();
            hash = 31 * hash + b.GetHashCode();
            return 31 * hash + c.GetHashCode();
        }
    }
}