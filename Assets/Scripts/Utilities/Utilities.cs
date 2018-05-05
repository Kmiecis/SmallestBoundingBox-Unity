using System.Collections.Generic;

public static class Utilities
{
    public static void Swap<T>(ref T left, ref T right)
    {
        T temp = left;
        left = right;
        right = temp;
    }


    public static bool IsNullOrEmpty<T>(List<T> list)
    {
        return (list == null) || (list.Count == 0);
    }


    public static bool IsNullOrEmpty<T>(HashSet<T> hashSet)
    {
        return (hashSet == null) || (hashSet.Count == 0);
    }
}