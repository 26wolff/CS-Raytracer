using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Render
{
    // A face (triangle, quad, etc.)
    public class Scene
    {
        List<Vec3> VerticiesList = new List<Vec3>();
        List<Face> FaceList = new List<Face>();

        public Scene()
        {
            // load all obj files
            LoadOBJ("Untitled.obj");
        }

        public void LoadOBJ(string name)
        {

            string fullPath = Path.Combine("./objects", name);
            foreach (var line in File.ReadLines(fullPath))
            {
                if (line.StartsWith("v "))
                {
                    Console.WriteLine("Vertex: " + line);
                }
                else if (line.StartsWith("f "))
                {
                    Console.WriteLine("Face: " + line);
                }
            }
        }


    }
    class Face
    {
        public List<int> v_Index { get; private set; }
        public Vec3 normal { get; private set; }
        public Material material { get; private set; }

        public Face(Vec3 p, Vec3 Normal, Material mat)
        {
            v_Index = new List<int> { (int)p.x, (int)p.y, (int)p.z };
            normal = Normal;
            material = mat;
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
            ColorRGB baseColor,
            ColorRGB specularColor,
            ColorRGB ambientColor,
            ColorRGB emisionColor,
            float shininess,
            float reflectivity,
            float transparency,
            float refractiveIndex
        )
        {
            BaseColor = baseColor;
            SpecularColor = specularColor;
            AmbientColor = ambientColor;
            EmissionColor = emisionColor;
            Shininess = shininess;
            Reflectivity = reflectivity;
            Transparency = transparency;
            RefractiveIndex = refractiveIndex;
        }
    }

}
