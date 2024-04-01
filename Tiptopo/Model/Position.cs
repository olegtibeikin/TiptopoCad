using Autodesk.AutoCAD.Geometry;

namespace Tiptopo.Model
{
    public class Position
    {
        public double x { get; set; }

        public double y { get; set; }
        public double z { get; set; }

        public Point3d toPoint3d() {
            return new Point3d(x, y, z);
        }

        public Point3d toPoint3dZeroHeight()
        {
            return new Point3d(x, y, 0.0);
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
