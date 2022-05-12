using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Numerics;

using IONET;
using IONET.Core;
using IONET.Core.Model;
using IONET.Core.Skeleton;
using QueenIO;
using UAssetAPI;
using UAssetAPI.PropertyTypes;
using UAssetAPI.StructTypes;

namespace ClothExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Not Enough Arguments");
                Help();
            }
            else if (args.Contains("-h", StringComparer.OrdinalIgnoreCase) || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
                Help();
            else
            {
                string outfile = "dae";

                if (args.Contains("--dae", StringComparer.OrdinalIgnoreCase) || args.Contains("-dae", StringComparer.OrdinalIgnoreCase))
                    outfile = "dae";
                else if (args.Contains("--smd", StringComparer.OrdinalIgnoreCase) || args.Contains("-smd", StringComparer.OrdinalIgnoreCase))
                    outfile = "smd";
                else if (args.Contains("--obj", StringComparer.OrdinalIgnoreCase) || args.Contains("-obj", StringComparer.OrdinalIgnoreCase))
                    outfile = "obj";


                Relic relic = Blood.Open(args.Last());
                var Imports = relic.GetImports();
                List<int> clothIndexes = new List<int>();

                foreach (Export export in relic.Exports)
                {
                    if (Imports[export.ClassIndex.Index].ObjectName == "ClothingAsset")
                        clothIndexes.Add(relic.Exports.IndexOf(export));
                }

                foreach (int index in clothIndexes)
                {
                    Cloth cloth = new Cloth();
                    cloth.Read(relic.Exports[index] as NormalExport);
                    MakeModel(cloth, outfile);
                }
            }
        }

        static void MakeModel(Cloth cloth, string Outformat)
        {
            Console.WriteLine("Reading Cloth data...");
            var scene = new IOScene();
            var model = new IOModel() { Name = cloth.Name };
            scene.Models.Add(model);

            model.Skeleton.RootBones.Add(new IOBone() { Name = "Root" });
            foreach (string bone in cloth.UsedBoneNames)
            {
                model.Skeleton.RootBones[0].AddChild(new IOBone() { Name = bone });
            }
            model.Skeleton.RootBones[0].AddChild(new IOBone() { Name = "MaxDistances" });
            model.Skeleton.RootBones[0].AddChild(new IOBone() { Name = "BackstopDistances" });
            model.Skeleton.RootBones[0].AddChild(new IOBone() { Name = "BackstopRadiuses" });


            IOMesh mesh = new IOMesh() { Name = cloth.Name };
            for (int i = 0; i < cloth.Vertices.Count; i++)
            {
                IOVertex iOVertex = new IOVertex() { Position = cloth.Vertices[i] };
                iOVertex.Normal = cloth.Normals[i];
                iOVertex.Normal.X = -iOVertex.Normal.X;
                iOVertex.ResetEnvelope(model.Skeleton);
                for (int j = 0; j < cloth.BoneData[i].NumInfluences; j++)
                {
                    iOVertex.Envelope.Weights.Add(new IOBoneWeight() { BoneName = cloth.UsedBoneNames[cloth.BoneData[i].BoneIndices[j]], Weight = cloth.BoneData[i].BoneWeights[j] });
                }
                iOVertex.Envelope.Weights.Add(new IOBoneWeight() { BoneName = "MaxDistances", Weight = cloth.MaxDistances[i] / 100 });
                iOVertex.Envelope.Weights.Add(new IOBoneWeight() { BoneName = "BackstopDistances", Weight = cloth.BackstopDistances[i] / 100 });
                iOVertex.Envelope.Weights.Add(new IOBoneWeight() { BoneName = "BackstopRadiuses", Weight = cloth.BackstopRadiuses[i] / 100 });
                mesh.Vertices.Add(iOVertex);
            }
            
            int y = 0;
            while (y < cloth.Indices.Count)
            {
                IOPolygon poly = new IOPolygon() { Indicies = new List<int>() { cloth.Indices[y++], cloth.Indices[y++], cloth.Indices[y++] } };
                mesh.Polygons.Add(poly);
            }
            if (Outformat == "dae")
            {
                Matrix4x4 matrix4X4 = new Matrix4x4(1, 0, 0, 0,
                                                    0, 0, 1, 0,
                                                    0, 1, 0, 0,
                                                    0, 0, 0, 1);
                mesh.TransformVertices(matrix4X4);
            }
            model.Meshes.Add(mesh);

            dynamic Exporter = new IONET.Collada.ColladaExporter();

            switch (Outformat)
            {
                case "smd":
                    Exporter = new IONET.SMD.SMDExporter();
                    break;
                case "obj":
                    Exporter = new IONET.Wavefront.OBJExporter();
                    break;
                default:
                    Exporter = new IONET.Collada.ColladaExporter();
                    break;
            }

            Exporter.ExportScene(scene, $"{cloth.Name}.{Outformat}", new ExportSettings());
            Console.WriteLine($"Exported Cloth to {cloth.Name}.{Outformat}.");
            string json = JsonConvert.SerializeObject(cloth.Config, Formatting.Indented);
            File.WriteAllText($"{cloth.Name}-ClothConfig.json", json);
            Console.WriteLine($"Wrote Cloth config to {cloth.Name}-ClothConfig.json.");
            json = JsonConvert.SerializeObject(cloth.CollisionData, Formatting.Indented);
            File.WriteAllText($"{cloth.Name}-CollisionData.json", json);
            Console.WriteLine($"Wrote Cloth config to {cloth.Name}-CollisionData.json.");
        }

        static void Help()
        {
            Console.WriteLine("Usage: ClothExtractor [Options] [File]\n");
            Console.WriteLine("Options:\n  -h, --help\t\tPrints this help text and exits");
            Console.WriteLine("  -dae/-smd/-obj\t\tExports the driver mesh in the given format (Default dae)");
        }
    }
}
