using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using HoaryFox.Member;
using Rhino.Geometry;
using STBDotNet.Elements;
using STBDotNet.Elements.StbModel;
using STBDotNet.Elements.StbModel.StbMember;

namespace HoaryFox.Component.Base
{
    public class SecTagBase : GH_Component
    {
        private StbElements _stbElements;
        private int _size;
        private readonly MemberBase _member;

        private GH_Structure<GH_String> _frameTags = new GH_Structure<GH_String>();
        private List<Point3d> _tagPos = new List<Point3d>();

        protected SecTagBase(string name, string nickname, string description, MemberBase member)
            :base(name, nickname, description, "HoaryFox", "Section")
        {
            _member = member;
        }

        public override void ClearData()
        {
            base.ClearData();
            _frameTags.Clear();
            _tagPos.Clear();
        }
        
        public override bool IsPreviewCapable => true;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "input ST-Bridge file data", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Size", "S", "Tag size", GH_ParamAccess.item, 12);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("SecTag", "STag", "output section tag", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData("Data", ref _stbElements)) { return; }
            if (!DA.GetData("Size", ref _size)) { return; }

            IEnumerable<IFrame> frames = Util.GetFrames(_stbElements, _member);
            GetTag(frames);

            DA.SetDataTree(0, _frameTags);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_frameTags.DataCount == 0)
            {
                return;
            }

            for (var i = 0; i < _frameTags.PathCount; i++)
            {
                List<GH_String> tags = _frameTags.Branches[i];
                string tag = tags[0].ToString() + "\n" + tags[1].ToString() + "\n" + tags[2].ToString() + "\n" + 
                             tags[3].ToString() + "\n" + tags[4].ToString() + "\n" + tags[5].ToString();
                args.Display.Draw2dText(tag, Color.Black, _tagPos[i], false, _size);
            }
        }

        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("6300E95D-38AF-47A6-B792-E4680FE37F49");

        private void GetTag(List<IFrame> frames)
        {
            var tags = new CreateTag(_stbElements.Model.Nodes, _stbElements.Model.Sections);
            _frameTags = tags.Frame(frames);
            _tagPos = tags.Position;
        }
    }
}
