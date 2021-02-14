using System;
using System.Collections.Generic;
using System.Drawing;
using Karamba.CrossSections;
using Karamba.Materials;
using STBReader;
using STBReader.Member;
using STBReader.Section;

namespace KarambaConnect.S2K
{
    public static class CrossSection
    {
        public static List<string>[] GetIndex(StbData stbData)
        {
            var k3Ids = new List<string>[2];
            k3Ids[0] = new List<string>();
            k3Ids[1] = new List<string>();

            var frameList = new List<StbFrame> { stbData.Columns, stbData.Girders, stbData.Braces };
            foreach (StbFrame frame in frameList)
            {
                int index = frame is StbBraces ? 1 : 0;
                for (var eNum = 0; eNum < frame.Id.Count; eNum++)
                {
                    k3Ids[index].Add("Id" + frame.IdSection[eNum]);
                }
            }

            return k3Ids;
        }

        public static IEnumerable<CroSec> GetCroSec(StbData stbData, CroSecFamilyName familyName, List<MatchedMaterial> materials)
        {
            // TODO: 材軸の回転は未設定（部材軸を設定する箇所指定できる）
            var k3CroSec = new List<CroSec>();
            materials.AddRange(DefaultMaterials());

            // TODO: STBReader じゃそもそも材料の情報をとってきてないのでマッチも何もなかった。STBDotNet化が必要！！！！
            k3CroSec.AddRange(ColumnRc(stbData, materials, familyName));
            k3CroSec.AddRange(BeamRc(stbData, materials, familyName));
            k3CroSec.AddRange(Steel(stbData, materials, familyName));

            return k3CroSec;
        }

        private static IEnumerable<MatchedMaterial> DefaultMaterials()
        {
            return new List<MatchedMaterial>
            {
                new MatchedMaterial(
                    new List<string> {"fc21"},
                    new FemMaterial_Isotrop("Concrete", "Fc21", 2186_0000, 911_0000, 911_0000, 24, 14_0000, 1.00E-05,
                        Color.Gray)
                ),
                new MatchedMaterial(
                    new List<string> {"SN400"},
                    new FemMaterial_Isotrop("Steel", "SN400", 20500_0000, 8076_0000, 8076_0000, 78.5, 235_0000,
                        1.20E-05, Color.Brown)
                )
            };
        }

        private static IEnumerable<CroSec> ColumnRc(StbData stbData, FemMaterial material, CroSecFamilyName familyName)
        {
            var k3CroSec = new List<CroSec>();

            for (var i = 0; i < stbData.SecColumnRc.Id.Count; i++)
            {
                double p1 = stbData.SecColumnRc.Height[i] / 10d;
                double p2 = stbData.SecColumnRc.Width[i] / 10d;
                var name = $"CD-{p2 * 10}x{p1 * 10}";

                ShapeTypes shapeType = stbData.SecColumnRc.Height[i] <= 0 ? ShapeTypes.Pipe : ShapeTypes.BOX;
                CroSec_Beam croSec;
                if (shapeType == ShapeTypes.BOX)
                {
                    // TODO:材料の設定は直す
                    croSec = new CroSec_Trapezoid(familyName.Box, name, null, null, material,
                        p1, p2, p2);
                }
                else
                {
                    // TODO: Karambaは中実円断面ないため、PIPEに置換してる。任意断面設定できるはずなので、そっちの方がいい気がする。
                    croSec = new CroSec_Circle(familyName.Circle, name, null, null, material,
                        p2, p2 / 2);
                }
                croSec.AddElemId("Id" + stbData.SecColumnRc.Id[i]);
                k3CroSec.Add(croSec);
            }

            return k3CroSec;
        }

        private static IEnumerable<CroSec> BeamRc(StbData stbData, FemMaterial material, CroSecFamilyName familyName)
        {
            var k3CroSec = new List<CroSec>();

            for (var i = 0; i < stbData.SecBeamRc.Id.Count; i++)
            {
                double p1 = stbData.SecBeamRc.Depth[i] / 10d;
                double p2 = stbData.SecBeamRc.Width[i] / 10d;
                var name = $"BD-{p2 * 10}x{p1 * 10}";

                var croSec = new CroSec_Trapezoid(familyName.Box, name, null, null, material,
                    p1, p2, p2);
                croSec.AddElemId("Id" + stbData.SecBeamRc.Id[i]);
                k3CroSec.Add(croSec);
            }

            return k3CroSec;
        }

        private static IEnumerable<CroSec> Steel(StbData stbData, FemMaterial material, CroSecFamilyName familyName)
        {
            var k3CroSec = new List<CroSec>();

            for (var i = 0; i < stbData.SecSteel.Name.Count; i++)
            {
                string name = stbData.SecSteel.Name[i];
                double p1 = stbData.SecSteel.P1[i] / 10d;
                double p2 = stbData.SecSteel.P2[i] / 10d;
                double p3 = stbData.SecSteel.P3.Count < i + 1 ? 0 : stbData.SecSteel.P3[i] / 10d;
                double p4 = stbData.SecSteel.P4.Count < i + 1 ? 0 : stbData.SecSteel.P4[i] / 10d;
                ShapeTypes shapeType = stbData.SecSteel.ShapeType[i];

                CroSec croSec;
                double eLength;
                switch (shapeType)
                {
                    case ShapeTypes.H:
                        croSec = new CroSec_I(familyName.H, name, null, null, material,
                            p1, p2, p2, p4, p4, p3);
                        break;
                    case ShapeTypes.L:
                        // TODO:Karambaに対応断面形状がないため等価軸断面積置換
                        eLength = Math.Sqrt(p1 * p3 + p2 * p4 - p3 * p4);
                        croSec = new CroSec_Trapezoid(familyName.L, name, null, null, material,
                            eLength, eLength, eLength);
                        break;
                    case ShapeTypes.T:
                        croSec = new CroSec_T(familyName.T, name, null, null, material,
                            p1, p2, p3, p4);
                        break;
                    case ShapeTypes.C:
                        // TODO:Karambaに対応断面形状がないため等価軸断面積置換
                        eLength = Math.Sqrt(p1 * p3 + p2 * p4 - 2 * p3 * p4);
                        croSec = new CroSec_Trapezoid(familyName.Other, name, null, null, material,
                            eLength, eLength, eLength);
                        break;
                    case ShapeTypes.FB:
                        croSec = new CroSec_Trapezoid(familyName.FB, name, null, null, material,
                            p1, p2, p2);
                        break;
                    case ShapeTypes.BOX:
                        throw new ArgumentOutOfRangeException();
                    case ShapeTypes.Bar:
                        // TODO: Karambaは中実円断面ないため、PIPEに置換してる。任意断面設定できるはずなので、そっちの方がいい気がする。
                        croSec = new CroSec_Circle(familyName.Other, name, null, null, material,
                            p2, p2 / 2);
                        break;
                    case ShapeTypes.Pipe:
                        croSec = new CroSec_Circle(familyName.Pipe, name, null, null, material,
                            p2, p1);
                        break;
                    case ShapeTypes.RollBOX:
                        croSec = new CroSec_Box(familyName.Box, name, null, null, material,
                            p1, p2, p2, p3, p3, p3, p4);
                        break;
                    case ShapeTypes.BuildBOX:
                        croSec = new CroSec_Box(familyName.Box, name, null, null, material,
                            p1, p2, p2, p3, p3, p4, 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                for (var j = 0; j < stbData.SecColumnS.Id.Count; j++)
                {
                    if (stbData.SecColumnS.Shape[j] != name)
                    {
                        continue;
                    }
                    croSec?.AddElemId("Id" + stbData.SecColumnS.Id[j]);
                }
                for (var j = 0; j < stbData.SecBeamS.Id.Count; j++)
                {
                    if (stbData.SecBeamS.Shape[j] != name)
                    {
                        continue;
                    }
                    croSec?.AddElemId("Id" + stbData.SecBeamS.Id[j]);
                }
                for (var j = 0; j < stbData.SecBraceS.Id.Count; j++)
                {
                    if (stbData.SecBraceS.Shape[j] != name)
                    {
                        continue;
                    }
                    croSec?.AddElemId("Id" + stbData.SecBraceS.Id[j]);
                }
                k3CroSec.Add(croSec);
            }

            return k3CroSec;
        }
    }
}
