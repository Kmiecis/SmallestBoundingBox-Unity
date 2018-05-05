using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataCloud3
{
    public static List<Vector3> Cube( int Count, Vector3 Extents, Vector3 Origin )
    {
        List<Vector3> result = new List<Vector3>( Count );

        for ( int i = 0; i < Count; i++ )
        {
            result.Add( new Vector3 (
                Random.Range( Origin.x - Extents.x, Origin.x + Extents.x ),
                Random.Range( Origin.y - Extents.y, Origin.y + Extents.y ),
                Random.Range( Origin.z - Extents.z, Origin.z + Extents.z )
            ));
        }

        return result;
    }


    public static List<Vector3> Ellipsoid( int Count, Vector3 Extents, Vector3 Origin )
    {
        List<Vector3> result = new List<Vector3>( Count );

        Vector3 reverseExtents = new Vector3( 1f / Extents.x, 1f / Extents.y, 1f / Extents.z );

        int innerCount = 0;
        while ( innerCount < Count )
        {
            float x = Random.Range( - Extents.x, Extents.x );
            float y = Random.Range( - Extents.y, Extents.y );
            float z = Random.Range( - Extents.z, Extents.z );

            float dx = x * reverseExtents.x;
            float dy = y * reverseExtents.y;
            float dz = z * reverseExtents.z;

            if ( dx * dx + dy * dy + dz * dz <= 1 )
            {
                result.Add( new Vector3( x, y, z ) + Origin );
                innerCount++;
            }
        }

        return result;
    }
}
