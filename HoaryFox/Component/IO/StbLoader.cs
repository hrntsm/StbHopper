using System;
using Grasshopper.Kernel;
using STBDotNet;
using STBDotNet.Elements;

namespace HoaryFox.Component.IO
{
    public class StbLoader:GH_Component
    {
        public StbLoader()
          : base("Load stb data", "Loader", "Read ST-Bridge file and display", "HoaryFox", "IO")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("path", "path", "input ST-Bridge file path", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "output StbData", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var path = string.Empty;
            if (!DA.GetData("path", ref path)) { return; }
            
            var serializer = new STBDotNet.Serialization.Serializer();
            StbElements stbElements = serializer.Deserialize(path);
            
            DA.SetData(0, stbElements);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.LoadStb;
        public override Guid ComponentGuid => new Guid("B8B7631C-BCAE-4549-95F7-1954D4781D24");
    }
}
