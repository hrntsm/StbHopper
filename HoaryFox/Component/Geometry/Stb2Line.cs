using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using HoaryFox.Member;
using Rhino.Geometry;
using STBDotNet.Elements;


namespace HoaryFox.Component.Geometry
{
    public class Stb2Line:GH_Component
    {
        private StbElements _stbElements;
        private List<Point3d> _nodes = new List<Point3d>();
        private List<Line> _columns = new List<Line>();
        private List<Line> _girders = new List<Line>();
        private List<Line> _posts = new List<Line>();
        private List<Line> _beams = new List<Line>();
        private List<Line> _braces = new List<Line>();

        public Stb2Line()
          : base(name: "Stb to Line", nickname: "S2L", description: "Read ST-Bridge file and display", category: "HoaryFox", subCategory: "Geometry")
        {
        }

        public override void ClearData()
        {
            base.ClearData();
            _nodes.Clear();
            _columns.Clear();
            _girders.Clear();
            _posts.Clear();
            _beams.Clear();
            _braces.Clear();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "input ST-Bridge Data", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes", "pt", "output StbNodes to point3d", GH_ParamAccess.list);
            pManager.AddLineParameter("Columns", "Col", "output StbColumns to Line", GH_ParamAccess.list);
            pManager.AddLineParameter("Girders", "Gird", "output StbGirders to Line", GH_ParamAccess.list);
            pManager.AddLineParameter("Posts", "Pst", "output StbPosts to Line", GH_ParamAccess.list);
            pManager.AddLineParameter("Beams", "Beam", "output StbBeams to Line", GH_ParamAccess.list);
            pManager.AddLineParameter("Braces", "Brc", "output StbBraces to Line", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData("Data", ref _stbElements)) { return; }

            MakeLine();

            DA.SetDataList(0, _nodes);
            DA.SetDataList(1, _columns);
            DA.SetDataList(2, _girders);
            DA.SetDataList(3, _posts);
            DA.SetDataList(4, _beams);
            DA.SetDataList(5, _braces);
        }

        private void MakeLine()
        {
            var createLines = new FrameLines(_stbElements);
            _nodes = createLines.Nodes();
            _columns = createLines.Columns();
            _girders = createLines.Girders();
            _posts = createLines.Posts();
            _beams = createLines.Beams();
            _braces = createLines.Braces();
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.Line;
        public override Guid ComponentGuid => new Guid("7d2f0c4e-4888-4607-8548-592104f6f06d");
    }
}