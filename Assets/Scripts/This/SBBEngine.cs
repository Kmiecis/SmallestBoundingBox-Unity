using UnityEngine;
using System.Collections.Generic;
using Enums;

[RequireComponent(typeof(MeshFilter))]
public class SBBEngine : MonoBehaviour
{
    MeshFilter meshFilter;
    List<Vector3> dataCloud;

    public DisplayDataCloud displayDataCloud;

    public ConvexHull2 ch2;
    public ConvexHull3 ch3;
    public SmallestBoundingBox2 sbb2;
    public WireframeRectangle wireframeRectangle;
    public SmallestBoundingBox3 sbb3;
    public WireframeCube wireframeCube;


    public void Refresh(int count, EnvironmentSpecs environment)
    {
        LoadIfNecessary();

        NewDataCloud(count, environment);

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

        Debug.Log(string.Format("ConvexHull2 calculation took {0}.", TimeMeasure.InSeconds(ch2.AlgorithmQuickhull).ToString("0.0000")));
        Debug.Log(string.Format("SmallestBoundingBox2 calculation took {0}.", TimeMeasure.InSeconds(sbb2.AlgorithmRotatingCalipers).ToString("0.0000")));

        meshFilter.mesh = ch2.GetMesh();
        wireframeRectangle.FromSBB2(sbb2);
    }


    void On3D()
    {
        wireframeRectangle.Clear();

        displayDataCloud.dataCloud = dataCloud;
        //Debug.Log(string.Format("ConvexHull3 calculation took {0}.", TimeMeasure.InSeconds(ch3.AlgorithmRandomIncremental).ToString("0.0000")));
        dataCloud = DataCloudReductor.Reduce(dataCloud);

        ch3.DataCloud = dataCloud;
        Debug.Log("New count: " + dataCloud.Count);
        Debug.Log(string.Format("ConvexHull3 2 calculation took {0}.", TimeMeasure.InSeconds(ch3.AlgorithmRandomIncremental).ToString("0.0000")));
        //Debug.Log(string.Format("SmallestBoundingBox3 calculation took {0}.", TimeMeasure.InSeconds(sbb3.AlgorithmHullFaces).ToString("0.0000")));



        meshFilter.mesh = ch3.GetMesh();
        wireframeCube.FromSBB3(sbb3);
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
                                dataCloud = DataCloud3.Cube(count, transform.localScale, transform.position);
                                break;
                            }
                        case DataCloud3Type.Ellipsoid:
                            {
                                dataCloud = DataCloud3.Ellipsoid(count, transform.localScale, transform.position);
                                break;
                            }
                    }
                    ch3.DataCloud = dataCloud;

                    break;
                }
            case DataDimension._2D:
                {
                    switch (environment.dataCloud2Type)
                    {
                        default:    // Use Rectangle
                        case DataCloud2Type.Rectangle:
                            {
                                dataCloud = DataCloud2.Rectangle(count, transform.localScale, transform.position);
                                break;
                            }
                        case DataCloud2Type.Ellipse:
                            {
                                dataCloud = DataCloud2.Ellipse(count, transform.localScale, transform.position);
                                break;
                            }
                    }
                    ch2.DataCloud = dataCloud;

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
        if (dataCloud == null)
        {
            dataCloud = new List<Vector3>();
        }
        if (ch2 == null)
        {
            ch2 = new ConvexHull2(dataCloud);
        }
        if (sbb2 == null)
        {
            sbb2 = new SmallestBoundingBox2(ch2);
        }
        if (ch3 == null)
        {
            ch3 = new ConvexHull3(dataCloud);
        }
        if (sbb3 == null)
        {
            sbb3 = new SmallestBoundingBox3(ch3);
        }
    }
}