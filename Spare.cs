using System;
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
    };
    public class ColorRGB
    {
        public float R { get; private set; }
        public float G { get; private set; }
        public float B { get; private set; }

        public ColorRGB(float r, float g, float b)
        {
            R = Clamp(r);
            G = Clamp(g);
            B = Clamp(b);
        }

        private float Clamp(float value) => Math.Max(0.0f, Math.Min(1.0f, value));

        public ColorRGB Add(ColorRGB other)
        {
            return new ColorRGB(
                Clamp(R + other.R),
                Clamp(G + other.G),
                Clamp(B + other.B)
            );
        }

        public ColorRGB Multiply(ColorRGB other)
        {
            return new ColorRGB(
                Clamp(R * other.R),
                Clamp(G * other.G),
                Clamp(B * other.B)
            );
        }

        public ColorRGB Scale(float value)
        {
            return new ColorRGB(
                Clamp(R * value),
                Clamp(G * value),
                Clamp(B * value)
            );
        }
    }

}