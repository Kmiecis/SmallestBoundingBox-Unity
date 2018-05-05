using System;

namespace Enums
{
    public enum DataCloud2Type
    {
        Rectangle,
        Ellipse
    }


    public enum DataCloud3Type
    {
        Cuboid,
        Ellipsoid
    }


    public enum DataDimension
    {
        _2D,
        _3D
    }


    public class EnvironmentSpecs
    {
        public DataDimension dataDimension;
        public DataCloud3Type dataCloud3Type;
        public DataCloud2Type dataCloud2Type;


        public EnvironmentSpecs()
        {
            dataDimension = DataDimension._3D;
            dataCloud3Type = DataCloud3Type.Cuboid;
            dataCloud2Type = DataCloud2Type.Rectangle;
        }
    }
}