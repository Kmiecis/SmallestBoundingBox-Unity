using UnityEngine;
using System.Collections.Generic;

public static class ArrayExtensions
{
    public static T[] Populate<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }

        return array;
    }
}


public static class ListExtensions
{
    public static List<T> Populate<T>(this List<T> list, T value, int count)
    {
        if (!list.IsNullOrEmpty())
        {
            list.Clear();
        }
        list = new List<T>(count);

        for (int i = 0; i < count; i++)
        {
            list.Add(value);
        }

        return list;
    }


    public static void RemoveFirst<T>(this List<T> list)
    {
        list.RemoveAt(0);
    }


    public static T Second<T>(this List<T> list)
    {
        return list[1];
    }


    public static T Last<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }


    public static void RemoveLast<T>(this List<T> list)
    {
        list.RemoveAt(list.Count - 1);
    }


    public static bool Empty<T>(this List<T> list)
    {
        return list.Count == 0;
    }


    public static bool IsNullOrEmpty<T>(this List<T> list)
    {
        return (list == null) || list.Empty();
    }
}


public static class HashSetExtensions
{
    public static bool IsNullOrEmpty<T>(this HashSet<T> hashSet)
    {
        return (hashSet == null) || (hashSet.Count == 0);
    }
}


public static class DictionaryExtensions
{
    public static bool IsNullOrEmpty<T, U>(this Dictionary<T, U> dictionary)
    {
        return (dictionary == null) || (dictionary.Count == 0);
    }
}


public static class Vector3Extensions
{
    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }


    public static void PerpendicularBasis(this Vector3 vector, out Vector3 outA, out Vector3 outB)
    {
        Vector3 a = vector.Abs();
        Vector3 q;

        if (a.x <= a.y)
        {
            if (a.x <= a.z)
                q = Vector3.right;
            else
                q = Vector3.forward;
        }
        else if (a.y <= a.z)
        {
            q = Vector3.up;
        }
        else
        {
            q = Vector3.forward;
        }

        outA = Vector3.Cross(vector, q).normalized;
        outB = Vector3.Cross(vector, outA).normalized;
    }


    public static Vector3 Perpendicular(this Vector3 vector, Vector3 hint, Vector3 hint2)
    {
        Vector3 perpendicular = Vector3.Cross(vector, hint).normalized;
        float length = perpendicular.magnitude;

        if (length < 1e-6f)
            return hint2;
        return perpendicular;
    }


    public static Vector3 Perpendicular(this Vector3 vector)
    {
        return vector.Perpendicular(Vector3.up, Vector3.forward);
    }
}


public static class Matrix4x4Extensions
{
    public static bool SolveAxb(this Matrix4x4 matrix, Vector3 b, ref Vector3 x)
    {
        float v00 = matrix.m00;
        float v10 = matrix.m10;
        float v20 = matrix.m20;

        float v01 = matrix.m01;
        float v11 = matrix.m11;
        float v21 = matrix.m21;

        float v02 = matrix.m02;
        float v12 = matrix.m12;
        float v22 = matrix.m22;

        float av00 = Mathf.Abs(v00);
        float av10 = Mathf.Abs(v10);
        float av20 = Mathf.Abs(v20);

        // Find which item in first column has largest absolute value.
        if (av10 >= av00 && av10 >= av20)
        {
            Utilities.Swap(ref v00, ref v10);
            Utilities.Swap(ref v01, ref v11);
            Utilities.Swap(ref v02, ref v12);
            Utilities.Swap(ref b.x, ref b.y);
        }
        else if (av20 >= av00)
        {
            Utilities.Swap(ref v00, ref v20);
            Utilities.Swap(ref v01, ref v21);
            Utilities.Swap(ref v02, ref v22);
            Utilities.Swap(ref b.x, ref b.z);
        }

        /* a b c | x
        d e f | y
        g h i | z , where |a| >= |d| && |a| >= |g| */

        if (ExtMathf.Equal(av00, 0f))
            return false;

        // Scale row so that leading element is one.
        float denom = 1f / v00;
        v01 *= denom;
        v02 *= denom;
        b.x *= denom;

        /* 1 b c | x
           d e f | y
           g h i | z */

        // Zero first column of second and third rows.
        v11 -= v10 * v01;
        v12 -= v10 * v02;
        b.y -= v10 * b.x;

        v21 -= v20 * v01;
        v22 -= v20 * v02;
        b.z -= v20 * b.x;

        /* 1 b c | x
           0 e f | y
           0 h i | z */

        // Pivotize again.
        if (Mathf.Abs(v21) > Mathf.Abs(v11))
        {
            Utilities.Swap(ref v11, ref v21);
            Utilities.Swap(ref v12, ref v22);
            Utilities.Swap(ref b.y, ref b.z);
        }

        if (ExtMathf.Equal(Mathf.Abs(v11), 0f))
            return false;

        /* 1 b c | x
           0 e f | y
           0 h i | z, where |e| >= |h| */

        denom = 1f / v11;
        v12 *= denom;
        b.y *= denom;

        /* 1 b c | x
           0 1 f | y
           0 h i | z */

        v22 -= v21 * v12;
        b.z -= v21 * b.y;

        /* 1 b c | x
           0 1 f | y
           0 0 i | z */

        if (ExtMathf.Equal(Mathf.Abs(v22), 0f))
            return false;

        x.z = b.z / v22;
        x.y = b.y - x.z * v12;
        x.x = b.x - x.z * v02 - x.y * v01;

        return true;
    }
}