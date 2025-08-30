using System;
using System.IO;

namespace Render
{



    public static class Spare
    {
        public static float Dot(Vec2 a, Vec2 b)
        {
            return a.x * b.x + a.y * b.y;
        }
        public static Vec2 Perpindicular(Vec2 a)
        {
            return new(a.y, -a.x);
        }
        public static void LogRay(Ray ray)
        {
            Console.WriteLine(
                $"Ray Position -> X: {ray.Position.x:F3}, Y: {ray.Position.y:F3}, Z: {ray.Position.z:F3} | " +
                $"Rotation -> Yaw: {ray.Rotation.y:F3}, Pitch: {ray.Rotation.x:F3}, Roll: {ray.Rotation.z:F3}"
            );
        }
        public static void DeleteAllFilesInFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    File.Delete(file);
                }
            }
        }
    }
    public class Ray
    {
        public Vec3 Position { get; set; } = new();
        public Vec3 Rotation { get; set; } = new(); // yaw, pitch, roll
    }


    public struct Vec2
    {
        public float x;
        public float y;

        public Vec2(float X, float Y)
        {
            x = X;
            y = Y;
        }

        // Addition
        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x + b.x, a.y + b.y);
        }

        // Subtraction
        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x - b.x, a.y - b.y);
        }

        // Component-wise multiplication
        public static Vec2 operator *(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x * b.x, a.y * b.y);
        }

        // Component-wise division
        public static Vec2 operator /(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x / b.x, a.y / b.y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }

    public struct intVec3
    {
        public int x;
        public int y;
        public int z;

        public intVec3(int X, int Y, int Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public static Vec3 operator -(Vec3 a, Vec3 b)
        {

            return new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vec3 operator *(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vec3 operator /(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        public static Vec3 Cross(Vec3 a, Vec3 b) =>
    new Vec3(
        a.x * b.z - a.z * b.y,
        a.z * b.x - a.x * b.z,
        a.x * b.y - a.y * b.x
    );

        public static Vec3 Normalize(Vec3 v)
        {
            float len = (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            return new Vec3(v.x / len, v.y / len, v.z / len);
        }



        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
        public override bool Equals(object obj)
        {
            if (obj is Vec3 other)
            {
                return x == other.x && y == other.y && z == other.z;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    };
    public struct Count3
    {
        public int p1;
        public int p2;
        public int p3;

        public Count3(int p1, int p2, int p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        // Optional: indexer for convenience
        public int this[int index]
        {
            get
            {
                return index switch
                {
                    0 => p1,
                    1 => p2,
                    2 => p3,
                    _ => throw new IndexOutOfRangeException("Count3 only has indices 0,1,2")
                };
            }
            set
            {
                switch (index)
                {
                    case 0: p1 = value; break;
                    case 1: p2 = value; break;
                    case 2: p3 = value; break;
                    default: throw new IndexOutOfRangeException("Count3 only has indices 0,1,2");
                }
            }
        }
    }
    public struct ColorRGB
    {
        public float R;
        public float G;
        public float B;

        public ColorRGB(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        // Optional: Clamp method for manual clamping
        public void Clamp()
        {
            R = Math.Max(0.0f, Math.Min(1.0f, R));
            G = Math.Max(0.0f, Math.Min(1.0f, G));
            B = Math.Max(0.0f, Math.Min(1.0f, B));
        }

        // Optional: Add, Multiply, Scale methods for convenience
        public ColorRGB Add(ColorRGB other)
        {
            return new ColorRGB(R + other.R, G + other.G, B + other.B);
        }

        public ColorRGB Multiply(ColorRGB other)
        {
            return new ColorRGB(R * other.R, G * other.G, B * other.B);
        }

        public ColorRGB Scale(float value)
        {
            return new ColorRGB(R * value, G * value, B * value);
        }
    }

}