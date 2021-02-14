using System;
using System.Drawing;
using Grasshopper.Kernel;
using Karamba.GHopper.Materials;
using KarambaConnect.Properties;
using KarambaConnect.S2K;

namespace KarambaConnect.Component.IO
{
    public class MaterialMatcher : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public MaterialMatcher()
          : base("MaterialMatcher", "MMatcher", "Assigning STB material information to Karamba3D", "HoaryFox", "IO")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Stb Material Name", "sMat", "Box shape cross section family name", GH_ParamAccess.item, "HF-Box");
            pManager.AddParameter(new Param_FemMaterial(), "Karamba Material", "KMat", "Karamba3D material data to match", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FamilyName", "Family", "Each CrossSection Family Name", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var name = new string[8];
            for (var i = 0; i < name.Length; i++)
            {
                if (!DA.GetData(i, ref name[i])) { return; }
            }

            var familyName = new CroSecFamilyName
            {
                Box = name[0],
                H = name[1],
                Circle = name[2],
                Pipe = name[3],
                FB = name[4],
                L = name[5],
                T = name[6],
                Other = name[7]
            };

            DA.SetData(0, familyName);
        }

        protected override Bitmap Icon => Resource.SetFamilyName;
        public override Guid ComponentGuid => new Guid("6479593D-DC0A-4362-BE28-515E6AC0E342");
    }
}
