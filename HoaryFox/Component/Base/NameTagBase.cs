using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using STBDotNet.Elements;
using STBDotNet.Elements.StbModel;
using STBDotNet.Elements.StbModel.StbMember;

namespace HoaryFox.Component.Base
{
    public class NameTagBase : GH_Component
    {
        private int _size;
        private StbElements _stbElements;
        private readonly MemberBase _member;
        private readonly List<string> _frameName = new List<string>();
        private readonly List<Point3d> _framePos = new List<Point3d>();

        public override bool IsPreviewCapable => true;

        protected NameTagBase(string name, string nickname, string description, MemberBase member)
            :base(name, nickname, description, "HoaryFox", "Name")
        {
            _member = member;
        }

        public override void ClearData()
        {
            base.ClearData();
            _frameName.Clear();
            _framePos.Clear();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "input ST-Bridge file data", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Size", "S", "Tag size", GH_ParamAccess.item, 12);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("NameTag", "NTag", "output name tag", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData("Data", ref _stbElements)) { return; }
            if (!DA.GetData("Size", ref _size)) { return; }

            List<Node> nodes = _stbElements.Model.Nodes;
            IEnumerable<IFrame> frames = Util.GetFrames(_stbElements, _member);

            foreach (IFrame frame in frames)
            {
                int idStart = frame.IdNodeStart;
                int idEnd = frame.IdNodeEnd;
                _frameName.Add(frame.Name);
                _framePos.Add(((nodes[idStart].Position + nodes[idEnd].Position) / 2d).ToRhino());
            }

            DA.SetDataList(0, _frameName);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            for (var i = 0; i < _frameName.Count; i++)
            {
                args.Display.Draw2dText(_frameName[i], Color.Black, _framePos[i], true, _size);
            }
        }

        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("758DE991-F652-4EDC-BC63-2A454BA43FB0");
    }
}