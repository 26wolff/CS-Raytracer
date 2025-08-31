using ComputeSharp;
using System.Numerics; // for Vector3
using System;

namespace Render.GpuShaders
{
    [EmbeddedBytecode(DispatchAxis.X)]
    public readonly partial struct RaytraceKernel : IComputeShader
    {
        public readonly ReadOnlyBuffer<GpuFace> Faces;
        public readonly ReadOnlyBuffer<Material> Materials;
        public readonly int Count;
        public readonly int Width;
        public readonly int Height;
        public readonly float3 CameraPos;
        public readonly float3 CameraForward;
        public readonly float3 CameraRight;
        public readonly float3 CameraUp;
        public readonly float FovX;
        public readonly float FovY;
        public readonly int MaxBounces;
        public readonly ReadWriteBuffer<float3> Output;
        // Stores hit info for each bounce

        public RaytraceKernel(
            ReadOnlyBuffer<GpuFace> faces,
            ReadOnlyBuffer<Material> materials,
            int count,
            int width,
            int height,
            float3 cameraPos,
            float3 cameraForward,
            float3 cameraRight,
            float3 cameraUp,
            float fovX,
            float fovY,
            int maxBounces,
            ReadWriteBuffer<float3> output)
        {
            Faces = faces;
            Materials = materials;
            Count = count;
            Width = width;
            Height = height;
            CameraPos = cameraPos;
            CameraForward = cameraForward;
            CameraRight = cameraRight;
            CameraUp = cameraUp;
            FovX = fovX;
            FovY = fovY;
            MaxBounces = maxBounces;
            Output = output;
        }

        public void Execute()
        {
            int pixelIndex = ThreadIds.X;
            int x = pixelIndex % Width;
            int y = pixelIndex / Width;

            float u = (float)x / (float)Width;
            float v = 1.0f - ((float)y / (float)Height);

            // Generate ray direction for pixel
            float3 rayDir = GetRayDirection(u, v);
            float3 rayOrigin = CameraPos;

            float3 color = TraceRay(rayOrigin, rayDir, MaxBounces);
            Output[pixelIndex] = color;
        }


        private float3 GetRayDirection(float u, float v)
        {
            float3 dir = CameraForward
                + CameraRight * ((u - 0.5f) * FovX)
                + CameraUp * ((v - 0.5f) * FovY);
            return Hlsl.Normalize(dir);
        }
        public static float3 ColorRGBToFloat3(ColorRGB color)
        {
            return new float3(color.R, color.G, color.B);
        }
        private float3 TraceRay(float3 origin, float3 direction, int bounces)
        {
            float3 currOrigin = origin;
            float3 currDir = direction;
            float3 color = new float3(0f, 0f, 0f);
            float3 throughput = new float3(1f, 1f, 1f);
            float3 background = new float3(0.0f, 0.0f, 0.0f);

            for (int bounce = 0; bounce <= bounces; bounce++)
            {
                float closestT = float.MaxValue;
                int hitFaceIdx = -1;
                for (int i = 0; i < Faces.Length; i++)
                {
                    GpuFace f = Faces[i];
                    if (RayTriangleIntersect(currOrigin, currDir, f.V0, f.V1, f.V2, out float t))
                    {
                        if (t < closestT)
                        {
                            closestT = t;
                            hitFaceIdx = i;
                        }
                    }
                }
                if (hitFaceIdx == -1)
                {
                    color += throughput * background;
                    break;
                }

                GpuFace face = Faces[hitFaceIdx];
                float3 hitPoint = currOrigin + currDir * closestT;
                Material mat = Materials[face.Material];

                // Add emission at the hit surface
                color += throughput * ColorRGBToFloat3(mat.EmissionColor);

                // Russian roulette termination for performance
                if (bounce > 3)
                {
                    float maxThroughput = Hlsl.Max(throughput.X, Hlsl.Max(throughput.Y, throughput.Z));
                    // Use a hash-based pseudo random for GPU
                    float rr = Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(hitPoint, new float3(12.9898f, 78.233f, 37.719f))) * 43758.5453f);
                    if (rr > maxThroughput)
                        break;
                    throughput /= Hlsl.Max(maxThroughput, 1e-6f);
                }

                // Modulate throughput by surface color and reflection coefficient
                throughput *= ColorRGBToFloat3(mat.BaseColor) * mat.Reflectivity;

                // Reflect with roughness
                float3 normal = face.Normal;
                float3 incident = currDir;
                float dotIn = Hlsl.Dot(incident, normal);
                float3 reflectDir = incident - 2 * dotIn * normal;

                // Add roughness (use shininess as inverse roughness)
                float roughness = 1.0f / (mat.Shininess + 1.0f);
                float3 newDir = Hlsl.Normalize(Hlsl.Lerp(reflectDir, RandomHemisphereDirection(normal, hitPoint+ new float3(Count, Count * 1.37f, Count * 3.14f)), roughness));
                currDir = newDir;
                currOrigin = hitPoint + currDir * 1e-4f;
            }

            return color;
        }
        // Struct to hold hit info for each bounce
        public struct RayHit
        {
            public GpuFace Face;
            public float3 HitPoint;

            public RayHit(GpuFace face, float3 hitPoint)
            {
                Face = face;
                HitPoint = hitPoint;
            }
        }

        public static float NormalDifference(Vector3 n1, Vector3 n2)
        {
            // Assumes n1 and n2 are already normalized
            float dot = Vector3.Dot(n1, n2);

            // Clamp to avoid floating-point errors outside [-1,1]
            dot = Hlsl.Clamp(dot, -1f, 1f);

            // Remap: 1 → 0, -1 → 1
            return (1f - dot) * 0.5f;
        }
        private static bool RayTriangleIntersect(float3 orig, float3 dir, float3 v0, float3 v1, float3 v2, out float t)
        {
            t = 0f;
            float3 edge1 = v1 - v0;
            float3 edge2 = v2 - v0;

            float3 h = Hlsl.Cross(dir, edge2);
            float a = Hlsl.Dot(edge1, h);
            if (a > -1e-6f && a < 1e-6f) return false;

            float f = 1.0f / a;
            float3 s = orig - v0;
            float u = f * Hlsl.Dot(s, h);
            if (u < 0f || u > 1f) return false;

            float3 q = Hlsl.Cross(s, edge1);
            float v = f * Hlsl.Dot(dir, q);
            if (v < 0f || u + v > 1f) return false;

            t = f * Hlsl.Dot(edge2, q);
            return t > 1e-6f;
        }

        // -------------------------------
        // New function: Reflection with randomness
        // -------------------------------
        
        private static float3 RandomHemisphereDirection(float3 normal, float3 seed)
        {
            // Hash-based pseudo randomness using seed
            float3 h = Hlsl.Normalize(new float3(
                Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(seed, new float3(12.9898f, 78.233f, 37.719f))) * 43758.5453f),
                Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(seed, new float3(93.9898f, 67.345f, 12.345f))) * 12741.371f),
                Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(seed, new float3(45.332f, 11.234f, 88.889f))) * 98121.111f)
            ));

            if (Hlsl.Dot(h, normal) < 0f)
                h = -h;

            return h;
        }

    }
}
