using System;
using System.Collections.Generic;
using System.Drawing;

namespace Render
{
    // A face (triangle, quad, etc.)
    class Scene
    {
        List<Vec3> VerticiesList = new List<Vec3>();
        List<Face> FaceList = new List<Face>();




    }
    class Face
    {
        public List<int> v_Index { get; private set; }
        public Vec3 normal { get; private set; }
        public Material material { get; private set; }

        public Face(Vec3 p, Vec3 Normal, Material mat)
        {
            v_Index = new List<int> { (int) p.x, (int) p.y, (int) p.z };
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
