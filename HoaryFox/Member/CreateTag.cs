using System;
using System.Collections.Generic;
using System.Globalization;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using STBDotNet.Elements.StbModel;
using STBDotNet.Elements.StbModel.StbMember;
using STBDotNet.Elements.StbModel.StbSection;

namespace HoaryFox.Member
{
    public class CreateTag
    {
        private readonly List<Node> _nodes;
        private readonly List<ColumnRc> _colRc;
        private readonly List<ColumnS> _colS;
        private readonly List<BeamRc> _beamRc;
        private readonly List<BeamS> _beamS;
        private readonly List<BraceS> _braceS;
        private readonly SecSteel _secSteel;
        public List<Point3d> Position { get; } = new List<Point3d>();

        public CreateTag(List<Node> nodes, List<Section> sections)
        {
            _nodes = nodes;
            _secSteel = secSteel;
            _braceS = braceS;
            _beamS = beamS;
            _beamRc = beamRc;
            _colS = colS;
            _colRc = colRc;
        }

        public GH_Structure<GH_String> Frame(IEnumerable<IFrame> frames)
        {
            var eNum = 0;
            var ghSecStrings = new GH_Structure<GH_String>();
            
            foreach (IFrame frame in frames)
            {
                TagInfo tagInfo;
                int idSection = frame.IdSection;
                var ghPath = new GH_Path(new[] { eNum });
                KindStructure kind = frame.KindStructure;
                SetTagPosition(frame, eNum);

                switch (kind)
                {
                    case KindStructure.Rc:
                        tagInfo = TagRc(frame, idSection);
                        break;
                    case KindStructure.S:
                        tagInfo = TagSteel(frame, idSection);
                        break;
                    case KindStructure.Src:
                    case KindStructure.Cft:
                        throw new ArgumentException("Wrong kind structure");
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ghSecStrings.Append(new GH_String(tagInfo.Name), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.ShapeTypes.ToString()), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.P1.ToString(CultureInfo.InvariantCulture)), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.P2.ToString(CultureInfo.InvariantCulture)), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.P3.ToString(CultureInfo.InvariantCulture)), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.P4.ToString(CultureInfo.InvariantCulture)), ghPath);
            }
        
            return ghSecStrings;
        }

        private void SetTagPosition(IFrame frame, int eNum)
        {
            // 始点と終点の座標取得
            int startIndex = frame.IdNodeStart;
            int endIndex = frame.IdNodeEnd;
            var nodeStart = new Point3d(_nodes[startIndex].Position.ToRhino());
            var nodeEnd = new Point3d(_nodes[endIndex].X, _nodes[endIndex].Y, _nodes[endIndex].Z);
            Position.Add(new Point3d((nodeStart + nodeEnd) / 2d));
        }

        private TagInfo TagSteel(StbFrame frame, int idSection)
        {
            int idShape;
            string shapeName;
            switch (frame.FrameType)
            {
                case FrameType.Column:
                case FrameType.Post:
                    idShape = _colS.Id.IndexOf(idSection);
                    shapeName = _colS.Shape[idShape];
                    break;
                case FrameType.Girder:
                case FrameType.Beam:
                    idShape = _beamS.Id.IndexOf(idSection);
                    shapeName = _beamS.Shape[idShape];
                    break;
                case FrameType.Brace:
                    idShape = _braceS.Id.IndexOf(idSection);
                    shapeName = _braceS.Shape[idShape];
                    break;
                case FrameType.Slab:
                case FrameType.Wall:
                case FrameType.Any:
                    throw new ArgumentException("Wrong frame type");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int secIndex = _secSteel.Name.IndexOf(shapeName);
            var tagInfo = new TagInfo
            {
                Name = _secSteel.Name[secIndex],
                ShapeTypes = _secSteel.ShapeType[secIndex],
                P1 = _secSteel.P1[secIndex],
                P2 = _secSteel.P2[secIndex],
                P3 = _secSteel.P3[secIndex],
                P4 = _secSteel.P4[secIndex]
            };
            return tagInfo;
        }

        private TagInfo TagRc(StbFrame frame, int idSection)
        {
            int secIndex;
            TagInfo tagInfo;
            switch (frame.FrameType)
            {
                case FrameType.Column:
                case FrameType.Post:
                    secIndex = _colRc.Id.IndexOf(idSection);
                    tagInfo = new TagInfo(_colRc.Name[secIndex], _colRc.Height[secIndex], _colRc.Width[secIndex], 0d, 0d);
                    break;
                case FrameType.Girder:
                case FrameType.Beam:
                    secIndex = _beamRc.Id.IndexOf(idSection);
                    tagInfo = new TagInfo(_beamRc.Name[secIndex], _beamRc.Depth[secIndex], _beamRc.Width[secIndex], 0d, 0d);
                    break;
                case FrameType.Brace:
                case FrameType.Slab:
                case FrameType.Wall:
                case FrameType.Any:
                    throw new ArgumentException("Wrong frame type");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            tagInfo.ShapeTypes = tagInfo.P1 <= 0 ? ShapeTypes.Pipe : ShapeTypes.BOX;
            return tagInfo;
        }
    }

    public class TagInfo
    {
        public string Name { get; set; }
        public ShapeTypes ShapeTypes { get;  set; }
        public double P1 { get; set; }
        public double P2 { get; set; }
        public double P3 { get; set; }
        public double P4 { get; set; }

        public TagInfo()
        {
        }

        public TagInfo(string name, double p1, double p2, double p3, double p4)
        {
            Name = name;
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
        }
    }
}
