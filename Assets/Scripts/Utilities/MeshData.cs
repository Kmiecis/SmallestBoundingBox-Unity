using UnityEngine;
using System.Collections.Generic;

public class MeshData
{
    Dictionary<Vector3, int> mMeshData;
    List<Vector3> mVertices;
    List<int> mTriangles;

    bool mFlatShaded;
    public bool FlatShading
    {
        get { return mFlatShaded; }
        set { mFlatShaded = value; }
    }


    public MeshData(bool flatShading = false)
    {
        mFlatShaded = flatShading;
        LoadIfNecessary();
    }


    public void AddTriangles(params Vector3[] vertices)
    {
        if (vertices.Length % 3 != 0)
        {
            Debug.LogError(string.Format("Passed undivisible by 3 number of vertices {0}.", vertices.Length));
            return;
        }

        int index = 0;
        while (index < vertices.Length)
        {
            if (mFlatShaded)
            {
                for (int i = index; i < index + 3; i++)
                {
                    mVertices.Add(vertices[i]);
                    mTriangles.Add(mTriangles.Count);
                }
            }
            else
            {
                for (int i = index; i < index + 3; i++)
                {
                    Vector3 vertex = vertices[i];
                    int tri;
                    if (!mMeshData.TryGetValue(vertex, out tri))
                    {
                        tri = mVertices.Count;

                        mMeshData.Add(vertex, tri);
                        mVertices.Add(vertex);
                    }
                    mTriangles.Add(tri);
                }

            }
            index += 3;
        }
    }


    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = mVertices.ToArray();
        mesh.triangles = mTriangles.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }


    void LoadIfNecessary()
    {
        if (mMeshData.IsNullOrEmpty())
        {
            mMeshData = new Dictionary<Vector3, int>();
        }
        if (mVertices.IsNullOrEmpty())
        {
            mVertices = new List<Vector3>();
        }
        if (mTriangles.IsNullOrEmpty())
        {
            mTriangles = new List<int>();
        }
    }
}