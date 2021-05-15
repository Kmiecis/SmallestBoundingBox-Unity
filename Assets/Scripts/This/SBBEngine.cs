//#define RANDOM

using UnityEngine;
using System.Collections.Generic;
using Enums;

[RequireComponent(typeof(MeshFilter))]
public class SBBEngine : MonoBehaviour
{
    MeshFilter meshFilter;

    public DisplayDataCloud displayDataCloud;

    public ConvexHull2 ch2;
    public ConvexHull3 ch3;
    public OBB2 obb2;
    public WireframeRectangle wireframeRectangle;
    public OBB3 obb3;
    public WireframeCube wireframeCube;

    private List<Vector3> mDataCloud;


    public void Refresh(int count, EnvironmentSpecs environment)
    {
        LoadIfNecessary();

#if RANDOM
        NewDataCloud(count, environment);
#else
        mDataCloud = new List<Vector3>();
        mDataCloud.Add(new Vector3(-1f, -1f, -1f));
        mDataCloud.Add(new Vector3(1f, -1f, 1f));
        mDataCloud.Add(new Vector3(-1f, 1f, 1f));
        mDataCloud.Add(new Vector3(1f, 1f, -1f));
#endif

        switch (environment.dataDimension)
        {
            default:
            case DataDimension._3D:
                {
                    On3D();
                    break;
                }
            case DataDimension._2D:
                {
                    On2D();
                    break;
                }
        }
    }


    void On2D()
    {
        wireframeCube.Clear();
        ch2.DataCloud = mDataCloud;

        ch2.AlgorithmQuickhull();
        obb2.AlgorithmRotatingCalipers();

        displayDataCloud.dataCloud = mDataCloud;
        meshFilter.mesh = ch2.GetMesh();
        wireframeRectangle.FromRectangle(obb2.Rectangle);
    }


    void On3D()
    {
        wireframeRectangle.Clear();
        ch3.DataCloud = mDataCloud;

        ch3.AlgorithmRandomIncremental();
        ch3.TrimDataCloudIfNecessary();

        //obb3.AlgorithmOptimized();
        //Debug.Log("@ OBB3 Optimized calculation took: " + TimeMeasure.InSeconds(obb3.AlgorithmAABB).ToString(Const.Decimal));
        //Debug.Log("@ Volume calculated: " + obb3.Volume);
        //Debug.Log("@ OBB3 Optimized calculation took: " + TimeMeasure.InSeconds(obb3.AlgorithmBruteQuaternion).ToString(Const.Decimal));
        //Debug.Log("@ Volume calculated: " + obb3.Volume);
        Debug.Log("@ OBB3 Optimized calculation took: " + TimeMeasure.InSeconds(obb3.AlgorithmBruteDirection).ToString(Const.Decimal));
        Debug.Log("@ Volume calculated: " + obb3.Volume);
        //Debug.Log("@ OBB3 Optimized calculation took: " + TimeMeasure.InSeconds(obb3.AlgorithmHullFaces).ToString(Const.Decimal));
        //Debug.Log("@ Volume calculated: " + obb3.Volume);
        //Debug.Log("@ OBB3 Optimized calculation took: " + TimeMeasure.InSeconds(obb3.AlgorithmOptimized).ToString(Const.Decimal));
        //Debug.Log("@ Volume calculated: " + obb3.Volume);

        displayDataCloud.dataCloud = mDataCloud;
        meshFilter.mesh = ch3.GetMesh();
        wireframeCube.FromBox(obb3.Box);
    }


    void NewDataCloud(int count, EnvironmentSpecs environment)
    {
        switch (environment.dataDimension)
        {
            default:    // Use 3D
            case DataDimension._3D:
                {
                    switch (environment.dataCloud3Type)
                    {
                        default:    // Use Cube
                        case DataCloud3Type.Cuboid:
                            {
                                mDataCloud = DataCloud3.Cube(count, transform.localScale, transform.position);
                                break;
                            }
                        case DataCloud3Type.Ellipsoid:
                            {
                                mDataCloud = DataCloud3.Ellipsoid(count, transform.localScale, transform.position);
                                break;
                            }
                    }

                    break;
                }
            case DataDimension._2D:
                {
                    switch (environment.dataCloud2Type)
                    {
                        default:    // Use Rectangle
                        case DataCloud2Type.Rectangle:
                            {
                                mDataCloud = DataCloud2.Rectangle(count, transform.localScale, transform.position);
                                break;
                            }
                        case DataCloud2Type.Ellipse:
                            {
                                mDataCloud = DataCloud2.Ellipse(count, transform.localScale, transform.position);
                                break;
                            }
                    }

                    break;
                }
        }
    }


    void LoadIfNecessary()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
        if (ch2 == null)
        {
            ch2 = new ConvexHull2(new List<Vector3>());
        }
        if (obb2 == null)
        {
            obb2 = new OBB2(ch2);
        }
        if (ch3 == null)
        {
            ch3 = new ConvexHull3(new List<Vector3>());
        }
        if (obb3 == null)
        {
            obb3 = new OBB3(ch3);
        }
    }
}