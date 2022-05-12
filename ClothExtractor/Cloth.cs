using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UAssetAPI;
using UAssetAPI.PropertyTypes;
using UAssetAPI.StructTypes;
using Newtonsoft.Json;

namespace ClothExtractor
{
    public class Cloth
    {
        public string Name { get; set; }
        public ClothConfig Config { get; set; } = new ClothConfig();
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        public List<Vector3> Normals { get; set; } = new List<Vector3>();
        public List<int> Indices { get; set; } = new List<int>();
        public List<float> MaxDistances { get; set; } = new List<float>();
        public List<float> BackstopDistances { get; set; } = new List<float>();
        public List<float> BackstopRadiuses { get; set; } = new List<float>();
        public List<BonesData> BoneData { get; set; } = new List<BonesData>();
        public List<string> UsedBoneNames { get; set; } = new List<string>();
        public CollisionData CollisionData { get; set; } = new CollisionData();

        public void Read(NormalExport export)
        {
            StructPropertyData CollisionInfo = new StructPropertyData();
            int baseindex = 0;
            Name = export.ObjectName.ToString();

            //Check Cloth Config
            baseindex = export.Data.IndexOf(export.Data.FirstOrDefault(x => x.Name.ToString() == "ClothConfig"));
            if (baseindex != -1)
                Config.Read(export.Data[baseindex] as StructPropertyData);
            baseindex = 0;

            //Check Lod Map
            baseindex = export.Data.IndexOf(export.Data.FirstOrDefault(x => x.Name.ToString() == "LodData"));
            if (baseindex != -1)
            {
                StructPropertyData ClothLodData = ((ArrayPropertyData)export.Data[baseindex]).Value[0] as StructPropertyData;
                StructPropertyData PhyicalMeshData = ClothLodData.Value[0] as StructPropertyData;
                CollisionInfo = ClothLodData.Value[1] as StructPropertyData;
                foreach (StructPropertyData vert in ((ArrayPropertyData)PhyicalMeshData.Value[0]).Value)
                {
                    VectorPropertyData vec = vert.Value[0] as VectorPropertyData;
                    Vertices.Add(new Vector3(vec.Value.X, vec.Value.Y, vec.Value.Z));
                }
                foreach (StructPropertyData norm in ((ArrayPropertyData)PhyicalMeshData.Value[1]).Value)
                {
                    VectorPropertyData vec = norm.Value[0] as VectorPropertyData;
                    Normals.Add(new Vector3(vec.Value.X, vec.Value.Y, vec.Value.Z));
                }
                foreach (UInt32PropertyData uInt32 in ((ArrayPropertyData)PhyicalMeshData.Value[2]).Value)
                {
                    Indices.Add(Convert.ToInt32(uInt32.Value));
                }
                foreach (FloatPropertyData @float in ((ArrayPropertyData)PhyicalMeshData.Value[3]).Value)
                {
                    MaxDistances.Add(Convert.ToInt32(@float.Value));
                }
                foreach (FloatPropertyData @float in ((ArrayPropertyData)PhyicalMeshData.Value[4]).Value)
                {
                    BackstopDistances.Add(Convert.ToInt32(@float.Value));
                }
                foreach (FloatPropertyData @float in ((ArrayPropertyData)PhyicalMeshData.Value[5]).Value)
                {
                    BackstopRadiuses.Add(Convert.ToInt32(@float.Value));
                }
                foreach (StructPropertyData bone in ((ArrayPropertyData)PhyicalMeshData.Value[7]).Value)
                {
                    BonesData bonedata = new BonesData();
                    bonedata.Read(bone);
                    BoneData.Add(bonedata);
                }
            }
            baseindex = 0;

            //Check Bone Names
            baseindex = export.Data.IndexOf(export.Data.FirstOrDefault(x => x.Name.ToString() == "UsedBoneNames"));
            if (baseindex != -1)
            {
                foreach (NamePropertyData name in ((ArrayPropertyData)export.Data[baseindex]).Value)
                {
                    UsedBoneNames.Add(name.Value.ToString());
                }
            }
            baseindex = 0;
            CollisionData.Read(CollisionInfo, UsedBoneNames);
        }

    }

    public class BonesData
    {
        public int NumInfluences { get; set; }
        public List<int> BoneIndices { get; set; } = new List<int>();
        public List<float> BoneWeights { get; set; } = new List<float>();

        public void Read(StructPropertyData data)
        {
            int index = 0;
            NumInfluences = ((IntPropertyData)data.Value[index++]).Value;
            for (int i = 0; i < 8; i++)
            {
                BoneIndices.Add( Convert.ToInt32( ((UInt16PropertyData)data.Value[index++]).Value ) );
            }
            for (int i = 0; i < 8; i++)
            {
                BoneWeights.Add(((FloatPropertyData)data.Value[index++]).Value);
            }
        }
    }

    public class ClothConfig
    {
        [JsonIgnore]
        public string Name { get; set; } = "ClothConfig";
        [JsonIgnore]
        public string Type { get; internal set; } = "ClothConfig";
        public ClothConstranitSetup VerticalConstraintConfig { get; set; } = new ClothConstranitSetup();
        public ClothConstranitSetup HorizontalConstraintConfig { get; set; } = new ClothConstranitSetup();
        public ClothConstranitSetup BendConstraintConfig { get; set; } = new ClothConstranitSetup();
        public ClothConstranitSetup ShearConstraintConfig { get; set; } = new ClothConstranitSetup();
        public float SelfCollisionRadius { get; set; } = 0;
        public float SelfCollisionStiffness { get; set; } = 0;
        public float SelfCollisionCullScale { get; set; } = 1.0f;
        public Vector3 Damping { get; set; } = new Vector3(0.4f, 0.4f, 0.4f);
        public float Friction { get; set; } = 0.1f;
        public float WindDragCoefficient { get; set; } = 0.0002f;
        public float WindLiftCoefficient { get; set; } = 0.0002f;
        public Vector3 LinearDrag { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);
        public Vector3 AngularDrag { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);
        public Vector3 LinearInertiaScale { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);
        public Vector3 AngularInertiaScale { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);
        public Vector3 CentrifugalInertiaScale { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);
        public float SolverFrequency { get; set; } = 120;
        public float StiffnessFrequency { get; set; } = 100;
        public float GravityScale { get; set; } = 1.0f;
        public float TetherStiffness { get; set; } = 1.0f;
        public float TetherLimit { get; set; } = 1.0f;
        public float CollisionThickness { get; set; } = 1.0f;

        public void Read(StructPropertyData data)
        {
            Name = data.Name.ToString();
            foreach (PropertyData item in data.Value)
            {
                switch (item.PropertyType.Value.Value)
                {
                    case "StructProperty":
                        StructPropertyData @struct = item as StructPropertyData;
                        ReadStruct(@struct);
                        break;
                    case "FloatProperty":
                        FloatPropertyData @float = item as FloatPropertyData;
                        ReadFloat(@float);
                        break;
                }
            }
        }

        private void ReadStruct(StructPropertyData data)
        {
            switch (data.StructType.ToString())
            {
                case "ClothConstraintSetup":
                    switch (data.Name.ToString())
                    {
                        case "VerticalConstraintConfig":
                            VerticalConstraintConfig = new ClothConstranitSetup();
                            VerticalConstraintConfig.Read(data);
                            break;
                        case "HorizontalConstraintConfig":
                            HorizontalConstraintConfig = new ClothConstranitSetup();
                            HorizontalConstraintConfig.Read(data);
                            break;
                        case "BendConstraintConfig":
                            BendConstraintConfig = new ClothConstranitSetup();
                            BendConstraintConfig.Read(data);
                            break;
                        case "ShearConstraintConfig":
                            ShearConstraintConfig = new ClothConstranitSetup();
                            ShearConstraintConfig.Read(data);
                            break;
                    }
                    break;
                case "Vector":
                    VectorPropertyData @vector = data.Value[0] as VectorPropertyData;
                    switch (data.Name.ToString())
                    {
                        case "Damping":
                            Damping = new Vector3(@vector.Value.X, @vector.Value.Y, @vector.Value.Z);
                            break;
                        case "LinearDrag":
                            LinearDrag = new Vector3(@vector.Value.X, @vector.Value.Y, @vector.Value.Z);
                            break;
                        case "AngularDrag":
                            AngularDrag = new Vector3(@vector.Value.X, @vector.Value.Y, @vector.Value.Z);
                            break;
                        case "LinearInertiaScale":
                            LinearInertiaScale = new Vector3(@vector.Value.X, @vector.Value.Y, @vector.Value.Z);
                            break;
                        case "AngularInertiaScale":
                            AngularInertiaScale = new Vector3(@vector.Value.X, @vector.Value.Y, @vector.Value.Z);
                            break;
                        case "CentrifugalInertiaScale":
                            CentrifugalInertiaScale = new Vector3(@vector.Value.X, @vector.Value.Y, @vector.Value.Z);
                            break;
                    }
                    break;
            }
        }

        private void ReadFloat(FloatPropertyData data)
        {
            switch (data.Name.ToString())
            {
                case "SelfCollisionRadius":
                    SelfCollisionRadius = data.Value;
                    break;
                case "SelfCollisionStiffness":
                    SelfCollisionStiffness = data.Value;
                    break;
                case "SelfCollisionCullScale":
                    SelfCollisionCullScale = data.Value;
                    break;
                case "Friction":
                    Friction = data.Value;
                    break;
                case "WindDragCoefficient":
                    WindDragCoefficient = data.Value;
                    break;
                case "WindLiftCoefficient":
                    WindLiftCoefficient = data.Value;
                    break;
                case "SolverFrequency":
                    SolverFrequency = data.Value;
                    break;
                case "StiffnessFrequency":
                    StiffnessFrequency = data.Value;
                    break;
                case "GravityScale":
                    GravityScale = data.Value;
                    break;
                case "TetherStiffness":
                    TetherStiffness = data.Value;
                    break;
                case "TetherLimit":
                    TetherLimit = data.Value;
                    break;
                case "CollisionThickness":
                    CollisionThickness = data.Value;
                    break;
            }
        }

    }

    public class ClothConstranitSetup
    {
        [JsonIgnore]
        public string Name { get; set; }
        [JsonIgnore]
        public string Type { get; set; } = "ClothConstraintSetup";
        public float Stiffness { get; set; } = 1.0f;
        public float StiffnessMultiplier { get; set; } = 1.0f;
        public float StretchLimit { get; set; } = 1.0f;
        public float CompressionLimit { get; set; } = 1.0f;

        public void Read(StructPropertyData data)
        {
            Name = data.Name.ToString();
            foreach (PropertyData value in data.Value)
            {
                FloatPropertyData @float = value as FloatPropertyData;
                switch (@float.Name.ToString())
                {
                    case "Stiffness":
                        Stiffness = @float.Value;
                        break;
                    case "StiffnessMultiplier":
                        StiffnessMultiplier = @float.Value;
                        break;
                    case "StretchLimit":
                        StretchLimit = @float.Value;
                        break;
                    case "CompressionLimit":
                        CompressionLimit = @float.Value;
                        break;
                }
            }
        }
    }

    public class CollisionData
    {
        public List<Sphere> Spheres { get; set; } = new List<Sphere>();
        public List<Tuple<int, int>> SphereConnections { get; set; } = new List<Tuple<int, int>>();

        public void Read(StructPropertyData data, List<string> BoneNames)
        {
            ArrayPropertyData array = data.Value[0] as ArrayPropertyData;
            foreach (StructPropertyData sph in array.Value)
            {
                Sphere sp = new Sphere();
                sp.Read(sph, BoneNames);
                Spheres.Add(sp);
            }
            array = data.Value[1] as ArrayPropertyData;
            foreach (StructPropertyData connect in array.Value)
            {
                Tuple<int, int> temp = new Tuple<int, int>(((IntPropertyData)connect.Value[0]).Value, ((IntPropertyData)connect.Value[1]).Value);
                SphereConnections.Add(temp);
            }
        }

        public class Sphere
        {
            public string ParentBone { get; set; }
            public float Radius { get; set; }
            public Vector3 Position { get; set; } = new Vector3();

            public void Read(StructPropertyData data, List<string> BoneNames)
            {
                ParentBone = BoneNames[((IntPropertyData)data.Value[0]).Value];
                Radius = ((FloatPropertyData)data.Value[1]).Value;
                VectorPropertyData vector = ((StructPropertyData)data.Value[2]).Value[0] as VectorPropertyData;
                Position = new Vector3(vector.Value.X, vector.Value.Y, vector.Value.Z);
            }
        }
    }

    
}
