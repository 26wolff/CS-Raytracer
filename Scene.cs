using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ComputeSharp;

namespace Render
{
    // A face (triangle, quad, etc.)
    public class Scene
    {
        public List<Vec3> VerticiesList = new List<Vec3>();
        public List<Material> MaterialList = new List<Material>
        {
            new Material()
        };
        List<Face> FaceList = new List<Face>();

        // New: GPU-friendly faces (no indices, direct verts)
        public List<GpuFace> GpuFaces = new List<GpuFace>();

        public Scene()
        {
            Debug.LogNow("Scene Started: ", "s");
            Debug.HoldNow("SceneStart");

            // load all obj files
            Load_Multiple_From_TXT("ToRender.txt");

            // Build GPU buffer after loading
            BuildGpuFaces();

            Debug.LogNow("Scene Ended at: ", "s");
            Debug.HoldNow("SceneEnd");
            Debug.LogDiff("SceneEnd", "SceneStart", "Scene Took: ", "s");
        }

        public void Load_OBJ(string name)
        {
            // Track starting index for vertices and normals for this file
            int vertexStart = VerticiesList.Count;
            int normalStart = 0; // If you want to support global normals, use NormalsList.Count

            List<Vec3> temp_vertices = new List<Vec3>();
            List<Vec3> temp_normals = new List<Vec3>();
            List<(int vIdx, int nIdx)[]> temp_triangles = new List<(int, int)[]>();

            string fullPath = Path.Combine("./objects", name);
            foreach (var line in File.ReadLines(fullPath))
            {
                if (line.StartsWith("v "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    temp_vertices.Add(new Vec3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                }
                else if (line.StartsWith("vn "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    temp_normals.Add(new Vec3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                }
                else if (line.StartsWith("f "))
                {
                    string[] t_parts = line.Split(' ');
                    var face = new List<(int vIdx, int nIdx)>();
                    for (int i = 1; i < t_parts.Length; i++)
                    {
                        var temp = t_parts[i];
                        var split = temp.Split('/');
                        int vIdx = int.Parse(split[0]) - 1 + vertexStart; // Offset by starting index
                        int nIdx = (split.Length >= 3 && !string.IsNullOrWhiteSpace(split[2])) ? int.Parse(split[2]) - 1 + normalStart : -1;
                        face.Add((vIdx, nIdx));
                    }
                    // Triangulate polygon face (CCW order)
                    for (int i = 1; i < face.Count - 1; i++)
                    {
                        temp_triangles.Add(new[] { face[0], face[i], face[i + 1] });
                    }
                }
            }

            // Add all vertices in order, no deduplication
            VerticiesList.AddRange(temp_vertices);
            // If you want to support global normals, add: NormalsList.AddRange(temp_normals);

            // Build FaceList using exact indices from OBJ
            foreach (var tri in temp_triangles)
            {
                int idx0 = tri[0].vIdx;
                int idx1 = tri[1].vIdx;
                int idx2 = tri[2].vIdx;
                Count3 vIdx = new Count3(idx0, idx1, idx2);

                // Use per-vertex normals if available, else compute geometric normal
                Vec3 normal;
                if (tri[0].nIdx != -1 && tri[1].nIdx != -1 && tri[2].nIdx != -1 &&
                    tri[0].nIdx < temp_normals.Count + normalStart && tri[1].nIdx < temp_normals.Count + normalStart && tri[2].nIdx < temp_normals.Count + normalStart)
                {
                    Vec3 n0 = temp_normals[tri[0].nIdx - normalStart];
                    Vec3 n1 = temp_normals[tri[1].nIdx - normalStart];
                    Vec3 n2 = temp_normals[tri[2].nIdx - normalStart];
                    float nx = (n0.x + n1.x + n2.x) / 3f;
                    float ny = (n0.y + n1.y + n2.y) / 3f;
                    float nz = (n0.z + n1.z + n2.z) / 3f;
                    float mag = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    normal = mag > 1e-6f ? new Vec3(nx / mag, ny / mag, nz / mag) : new Vec3(0, 0, 0);
                }
                else
                {
                    Vec3 a = VerticiesList[idx0];
                    Vec3 b = VerticiesList[idx1];
                    Vec3 c = VerticiesList[idx2];
                    Vec3 ab = new Vec3(b.x - a.x, b.y - a.y, b.z - a.z);
                    Vec3 ac = new Vec3(c.x - a.x, c.y - a.y, c.z - a.z);
                    float nx = ab.y * ac.z - ab.z * ac.y;
                    float ny = ab.z * ac.x - ab.x * ac.z;
                    float nz = ab.x * ac.y - ab.y * ac.x;
                    float mag = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    normal = mag > 1e-6f ? new Vec3(nx / mag, ny / mag, nz / mag) : new Vec3(0, 0, 0);
                }
                FaceList.Add(new Face(vIdx, normal, 0));
            }
        }

        public void BuildGpuFaces()
        {
            GpuFaces.Clear();
            var rand = new Random(12345);
            foreach (var face in FaceList)
            {
                // Guard against out-of-range indices
                if (face.V_Index[0] < 0 || face.V_Index[0] >= VerticiesList.Count ||
                    face.V_Index[1] < 0 || face.V_Index[1] >= VerticiesList.Count ||
                    face.V_Index[2] < 0 || face.V_Index[2] >= VerticiesList.Count)
                {
                    Console.WriteLine($"Warning: Skipping face with invalid vertex indices: {face.V_Index[0]}, {face.V_Index[1]}, {face.V_Index[2]}");
                    continue;
                }
                Vec3 v0 = VerticiesList[face.V_Index[0]];
                Vec3 v1 = VerticiesList[face.V_Index[1]];
                Vec3 v2 = VerticiesList[face.V_Index[2]];

                ComputeSharp.Float3 fv0 = new ComputeSharp.Float3(v0.x, v0.y, v0.z);
                ComputeSharp.Float3 fv1 = new ComputeSharp.Float3(v1.x, v1.y, v1.z);
                ComputeSharp.Float3 fv2 = new ComputeSharp.Float3(v2.x, v2.y, v2.z);
                ComputeSharp.Float3 fnormal = new ComputeSharp.Float3(face.Normal.x, face.Normal.y, face.Normal.z);

                // Assign a random color per face
                float r = (float)rand.NextDouble();
                float g = (float)rand.NextDouble();
                float b = (float)rand.NextDouble();
                ComputeSharp.Float3 fcolor = new ComputeSharp.Float3(r, g, b);

                GpuFaces.Add(new GpuFace(fv0, fv1, fv2, fnormal, face.Material, fcolor));
            }
        }

        public void LogFace(Face face, int show_index_value)
        {
            string show_V_Index_value = "";
            if (show_index_value % 2 == 1)
            {
                show_V_Index_value =
                    $"\n: INDEX_VALUE: \n{VerticiesList[face.V_Index[0]]}\n{VerticiesList[face.V_Index[1]]}\n{VerticiesList[face.V_Index[2]]}\n";
            }

            Console.WriteLine(
                $"FACE: \n: V_Index (index)- {face.V_Index[0]}({face.V_Index[0] + 1}), {face.V_Index[1]}({face.V_Index[1] + 1}), {face.V_Index[2]}({face.V_Index[2] + 1}){show_V_Index_value}\n: normal - {face.Normal}\n: material (index)- {face.Material}\n"
            );
        }

        public void Load_Multiple_From_TXT(string name)
        {
            if (Path.GetExtension(name).ToLower() != ".txt") return;

            string fullPath = Path.Combine("./objects", name);
            foreach (var line in File.ReadLines(fullPath))
            {
                if (Path.GetExtension(line).ToLower() != ".obj" || line.StartsWith("#")) continue;
                Load_OBJ(line);
            }
        }
    }

    // CPU storage (indexed mesh)
    public struct Face
    {
        public Count3 V_Index { get; } // holds 3 indices
        public Vec3 Normal { get; }
        public int Material { get; }

        public Face(Count3 p, Vec3 normal, int mat)
        {
            V_Index = p;
            Normal = normal;
            Material = mat;
        }
    }

    // GPU storage (explicit triangle verts)
    public struct GpuFace
    {
        public float3 V0;
        public float3 V1;
        public float3 V2;
        public float3 Normal;
        public int Material;
        public float3 Color;

        public GpuFace(float3 v0, float3 v1, float3 v2, float3 normal, int material, float3 color)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            Normal = normal;
            Material = material;
            Color = color;
        }
    }

    public class Material
    {
        public ColorRGB BaseColor { get; set; }
        public ColorRGB SpecularColor { get; set; }
        public ColorRGB AmbientColor { get; set; }
        public ColorRGB EmissionColor { get; set; }
        public float Shininess { get; set; }
        public float Reflectivity { get; set; }
        public float Transparency { get; set; }
        public float RefractiveIndex { get; set; }

        public Material(
            ColorRGB baseColor = null,
            ColorRGB specularColor = null,
            ColorRGB ambientColor = null,
            ColorRGB emissionColor = null,
            float shininess = 32f,
            float reflectivity = 0.2f,
            float transparency = 0f,
            float refractiveIndex = 1f
        )
        {
            BaseColor = baseColor ?? new ColorRGB(0.7f, 0.25f, 0.7f);
            SpecularColor = specularColor ?? new ColorRGB(0.3f, 0.3f, 0.3f);
            AmbientColor = ambientColor ?? new ColorRGB(0.1f, 0.1f, 0.1f);
            EmissionColor = emissionColor ?? new ColorRGB(0f, 0f, 0f);

            Shininess = shininess;
            Reflectivity = reflectivity;
            Transparency = transparency;
            RefractiveIndex = refractiveIndex;
        }
    }
}
