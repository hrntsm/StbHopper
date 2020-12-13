using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Grasshopper.Kernel;
using HoaryFox.Member;
using STBDotNet;
using STBDotNet.Elements;
using STBDotNet.Elements.StbModel.StbMember;

namespace HoaryFox.Component.Geometry
{
    public class Stb2Brep:GH_Component
    {
        private StbElements _stbElements;

        private List<Brep> _slabBreps = new List<Brep>();
        private List<Brep> _wallBreps = new List<Brep>();
        private readonly List<List<Brep>> _frameBreps = new List<List<Brep>>();

        public Stb2Brep()
          : base("Stb to Brep", "S2B", "Read ST-Bridge file and display", "HoaryFox", "Geometry")
        {
        }

        public override void ClearData()
        {
            base.ClearData();
            _slabBreps.Clear();
            _wallBreps.Clear();
            _frameBreps.Clear();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "input ST-Bridge Data", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Columns", "Col", "output StbColumns to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("Girders", "Gird", "output StbGirders to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("Posts", "Pst", "output StbPosts to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("Beams", "Bm", "output StbBeams to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("Braces", "Brc", "output StbBraces to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("Slabs", "Slb", "output StbSlabs to Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("Walls", "Wl", "output StbWalls to Brep", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData("Data", ref _stbElements)) { return; }
            MakeBrep();

            for (var i = 0; i < 5; i++)
            {
                DA.SetDataList(i, _frameBreps[i]);
            }
            DA.SetDataList(5, _slabBreps);
            DA.SetDataList(6, _wallBreps);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.Brep;
        public override Guid ComponentGuid => new Guid("7d2f0c4e-4888-4607-8548-592104f6f06f");

        private void MakeBrep()
        {
            var stbFrames = new List<FrameBase>
            {
                _stbElements.Model.Members.Columns, _stbData.Girders, _stbData.Posts, _stbData.Beams, _stbData.Braces
            };
            
            var breps = new FrameBreps(_stbData);

            _slabBreps = breps.Slab(_stbData.Slabs);
            _wallBreps = breps.Wall(_stbData.Walls);

            foreach (StbFrame frame in stbFrames)
            {
                _frameBreps.Add(breps.Frame(frame));
            }
        }
    }
}
