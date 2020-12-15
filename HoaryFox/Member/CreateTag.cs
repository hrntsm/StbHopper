using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly List<ColumnRc> _colRc = new List<ColumnRc>();
        private readonly List<ColumnS> _colS = new List<ColumnS>();
        private readonly List<BeamRc> _beamRc = new List<BeamRc>();
        private readonly List<BeamS> _beamS = new List<BeamS>();
        private readonly List<BraceS> _braceS = new List<BraceS>();
        private readonly Steel _steel;
        public List<Point3d> Position { get; } = new List<Point3d>();

        public CreateTag(List<Node> nodes, IEnumerable<Section> sections)
        {
            _nodes = nodes;
            foreach (Section section in sections)
            {
                switch (section)
                {
                    case ColumnRc colRc:
                        _colRc.Add(colRc);
                        break;
                    case ColumnS colS:
                        _colS.Add(colS);
                        break;
                    case BeamRc beamRc:
                        _beamRc.Add(beamRc);
                        break;
                    case BeamS beamS:
                        _beamS.Add(beamS);
                        break;
                    case BraceS braceS:
                        _braceS.Add(braceS);
                        break;
                    case Steel steel:
                        _steel = steel;
                        break;
                }
            }
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
                SetTagPosition(frame);

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
                ghSecStrings.Append(new GH_String(tagInfo.FigureType.ToString()), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.P1.ToString()), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.P2.ToString()), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.P3.ToString()), ghPath);
                ghSecStrings.Append(new GH_String(tagInfo.P4.ToString()), ghPath);
            }
        
            return ghSecStrings;
        }

        private void SetTagPosition(IFrame frame)
        {
            // 始点と終点の座標取得
            int startIndex = frame.IdNodeStart;
            int endIndex = frame.IdNodeEnd;
            var nodeStart = new Point3d(_nodes[startIndex].Position.ToRhino());
            var nodeEnd = new Point3d(_nodes[endIndex].Position.ToRhino());
            Position.Add(new Point3d((nodeStart + nodeEnd) / 2d));
        }

        private TagInfo TagSteel(IFrame frame, int idSection)
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

        private TagInfo TagRc(IFrame frame, int idSection)
        {
            var tagInfo = new TagInfo();

            switch (frame)
            {
                case Column _:
                    foreach (ColumnRc rc in _colRc.Where(rc => rc.Id == idSection))
                    {
                        tagInfo.Name = rc.Name;
                        switch (rc.FigureType)
                        {
                            case RcColumnFigureType.Rectangle:
                                tagInfo.P1 = rc.Figure.SecRect.DX;
                                tagInfo.P2 = rc.Figure.SecRect.DY;
                                break;
                            case RcColumnFigureType.Circle:
                                tagInfo.P1 = rc.Figure.SecCircle.D;
                                tagInfo.P2 = -1;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        tagInfo.FigureType = rc.FigureType.ToString();
                    }
                    break;
                case Girder _:
                    foreach (BeamRc rc in _beamRc.Where(rc => rc.Id == idSection))
                    {
                        tagInfo.Name = rc.Name;
                        switch (rc.FigureType)
                        {
                            case RcBeamFigureType.Straight:
                                tagInfo.P1 = rc.Figure.SecStraight.Depth;
                                tagInfo.P2 = rc.Figure.SecStraight.Width;
                                break;
                            case RcBeamFigureType.Haunch:
                                tagInfo.P1 = rc.Figure.SecHaunch.DepthCenter;
                                tagInfo.P1 = rc.Figure.SecHaunch.WidthCenter;
                                break;
                            case RcBeamFigureType.Taper:
                                tagInfo.P1 = rc.Figure.SecTaper.DepthEnd;
                                tagInfo.P1 = rc.Figure.SecTaper.WidthEnd;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        tagInfo.FigureType = rc.FigureType.ToString();
                    }
                    break;
                case Brace _:
                    throw new ArgumentException("Wrong frame type");
                default:
                    throw new ArgumentOutOfRangeException();

            }

            return tagInfo;
        }
    }

    public class TagInfo
    {
        public string Name { get; set; }
        public string FigureType { get;  set; }
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
