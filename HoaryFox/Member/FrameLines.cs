using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using STBDotNet.Elements;
using STBDotNet.Elements.StbModel;
using STBDotNet.Elements.StbModel.StbMember;

namespace HoaryFox.Member
{
    public class FrameLines
    {
        private readonly StbElements _stbElements;
        private readonly List<Node> _nodes;
        
        public FrameLines(StbElements stbElements)
        {
            _stbElements = stbElements;
            _nodes = stbElements.Model.Nodes;
        }

        public List<Line> Columns()
        {
            return CreateFrameLines(_stbElements.Model.Members.Columns);
        }

        public List<Line> Girders()
        {
            return CreateFrameLines(_stbElements.Model.Members.Girders);
        }

        public List<Line> Posts()
        {
            return CreateFrameLines(_stbElements.Model.Members.Posts);
        }

        public List<Line> Beams()
        {
            return CreateFrameLines(_stbElements.Model.Members.Beams);
        }

        public List<Line> Braces()
        {
            return CreateFrameLines(_stbElements.Model.Members.Braces);
        }

        public List<Point3d> Nodes()
        {
            return _nodes.ToRhino();
        }

        private List<Line> CreateFrameLines(IEnumerable<IFrame> frames)
        {
            var lines = new List<Line>();
            foreach (IFrame frame in frames)
            {
                Point3d ptStart = _nodes[frame.IdNodeStart].Position.ToRhino();
                Point3d ptEnd = _nodes[frame.IdNodeEnd].Position.ToRhino();
                lines.Add(new Line(ptStart, ptEnd));
            }
            return lines;
        }
    }
}