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
        List<Material> MaterialList = new List<Material>
        {
            new Material()
        };
        List<Face> FaceList = new List<Face>();

        public Scene()
        {
            Debug.LogNow("Scene Started: ", "s");
            Debug.HoldNow("SceneStart");
            // load all obj files
            Load_Multiple_From_TXT("ToRender.txt");

            // foreach (Face face in FaceList)
            // {
            //     LogFace(face, 1);
            // }
            Debug.LogNow("Scene Ended at: ", "s");
            Debug.HoldNow("SceneEnd");
            Debug.LogDiff("SceneEnd","SceneStart","Scene Took: ","s");
            
            
        }
        public void Load_OBJ(string name)
        {
            List<int> temp_index_list = new List<int>();
            List<Vec3> temp_normal_list = new List<Vec3>();

            string fullPath = Path.Combine("./objects", name);
            foreach (var line in File.ReadLines(fullPath))
            {
                if (line.StartsWith("v "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    Vec3 vertex = new Vec3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));

                    // Adds the vec3 to the list if it does not exist, gives the index of the vec 3 if it either added or already exists
                    int index = VerticiesList.IndexOf(vertex);
                    if (index == -1)
                    {
                        index = VerticiesList.Count;
                        VerticiesList.Add(vertex);
                    }
                    temp_index_list.Add(index);
                }
                else if (line.StartsWith("vn "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    Vec3 normal = new Vec3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));

                    temp_normal_list.Add(normal);
                }
                else if (line.StartsWith("f "))
                {
                    List<intVec3> formated = new List<intVec3>();


                    string[] t_parts = line.Split(' ');
                    List<string> parts = new List<string>(t_parts);
                    parts.RemoveAt(0);

                    foreach (var temp in parts)
                    {
                        // formats each n/n/n into intVec3
                        int[] intCombo = Array.ConvertAll(temp.Split("/", StringSplitOptions.RemoveEmptyEntries), int.Parse);

                        formated.Add(new intVec3(intCombo[0], intCombo[1], intCombo[2]));
                    }

                    //Loops 1 face line to create multiple if 4p = 3p,3p
                    for (int i = 1; i < formated.Count - 1; i++)
                    {
                        // TODO : fix this to make it counter clockwise / clockwise instead of cl - ccl - ccl ...

                        // Holds the index of the face for global
                        int index1 = temp_index_list[formated[0].x - 1];
                        int index2 = temp_index_list[formated[i].x - 1];
                        int index3 = temp_index_list[formated[i + 1].x - 1];

                        Count3 vertex_index = new Count3(index1, index2, index3);

                        FaceList.Add(new Face(vertex_index, temp_normal_list[formated[0].z - 1], 0));
                    }

                }
            }
        }
        public void LogFace(Face face, int show_index_value)//show_index_value : 0=nothing, 1=v_Index, 2=material, 3=both
        {
            string show_v_Index_value = "";
            if (show_index_value % 2 == 1)
            {
                show_v_Index_value = $"\n: INDEX_VALUE: \n{VerticiesList[face.v_Index[0]].ToString()}\n{VerticiesList[face.v_Index[1]].ToString()}\n{VerticiesList[face.v_Index[2]].ToString()}\n";
            }

            Console.WriteLine($"FACE: \n: v_Index (index)- {face.v_Index[0]}({face.v_Index[0]+1}), {face.v_Index[1]}({face.v_Index[1]+1}), {face.v_Index[2]}({face.v_Index[2]+1}){show_v_Index_value}\n: normal - {face.normal.ToString()}\n: material (index)- {face.material}\n");
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
    public class Face
    {
        public Count3 v_Index { get; private set; }
        public Vec3 normal { get; private set; }
        public int material { get; private set; }

        public Face(Count3 p, Vec3 Normal, int mat)
        {
            v_Index = p;
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
