using System.Collections.Generic;
using Rhino.Geometry;
using STBDotNet.Elements.StbModel;
using STBDotNet.Geometry;

namespace HoaryFox
{
    public static class Convert
    {
        public static Point3d ToRhino(this Point3 pt)
        {
            return new Point3d(pt.X, pt.Y, pt.Z);
        }

        public static List<Point3d> ToRhino(this List<Node> nodes)
        {
            var result = new List<Point3d>(nodes.Count);
            foreach (Node node in nodes)
            {
                result.Add(node.Position.ToRhino());
            }

            return result;
        }

        public static Line ToRhino(this Line3 ln)
        {
            return new Line(ln.Start.ToRhino(), ln.End.ToRhino());
        }
    }
}