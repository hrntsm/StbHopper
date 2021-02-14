using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Karamba.GHopper.Materials;
using Karamba.Materials;
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
            pManager.AddTextParameter("Stb Material Name", "sMat", "Material name as defined in the ST-Bridge", GH_ParamAccess.list);
            pManager.AddParameter(new Param_FemMaterial(), "Karamba Material", "KMat", "Karamba3D material data to match", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MaterialPair", "MatPair", "Matched material pair", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var materialIn = new object();
            var materialName = new List<string>();
            if (!DA.GetDataList(0, materialName)) { return; }
            if (!DA.GetData(1, ref materialIn)) { return; }

            if (!(materialIn is GH_FemMaterial ghFemMaterial))
            {
                throw new ArgumentException("The input is not Karamba3D material setting!");
            }

            FemMaterial kMaterial = ghFemMaterial.Value;
            DA.SetData(0, new MatchedMaterial(materialName, kMaterial));
        }

        // protected override Bitmap Icon => Resource.SetFamilyName;
        public override Guid ComponentGuid => new Guid("8306859F-624B-426B-B983-B1E7199ADD47");
    }
}
