using System.Collections.Generic;
using System.Linq;
using Karamba.Materials;

namespace KarambaConnect.S2K
{
    public class MatchedMaterial
    {
        public IReadOnlyList<string> MaterialNames { get; set; }
        public FemMaterial KarambaMaterial { get; set; }

        public MatchedMaterial()
        { }

        public MatchedMaterial(IReadOnlyList<string> materialNames, FemMaterial kMaterial)
        {
            MaterialNames = materialNames;
            KarambaMaterial = kMaterial;
        }

        public override string ToString()
        {
            string str = MaterialNames.Aggregate("STB Material Names : \n", (current, name) => current + (name + ", "));
            str += "\n";
            str += "Karamba3D Material to match the above : \n" + KarambaMaterial;
            return str;
        }
    }
}
