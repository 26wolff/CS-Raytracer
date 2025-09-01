using ComputeSharp;
using System.Numerics; // Vector3
using System;

namespace Render.GpuShaders
{
    [EmbeddedBytecode(DispatchAxis.X)]
    public readonly partial struct RaytraceKernel : IComputeShader
    {
        public readonly ReadOnlyBuffer<GpuFace> Faces;
        public readonly ReadOnlyBuffer<Material> Materials;

        public readonly int Width;
        public readonly int Height;
        public readonly float3 CameraPos;
        public readonly float3 CameraForward;
        public readonly float3 CameraRight;
        public readonly float3 CameraUp;
        public readonly float FovX;
        public readonly float FovY;
        public readonly int MaxBounces;

        public readonly ReadWriteBuffer<float3> Accum;
        public readonly ReadWriteBuffer<int> SampleCount;
        public readonly ReadWriteBuffer<float3> Output;

        public readonly int FrameSampleIndex;
        public readonly float AntiAliasStrength;

        public RaytraceKernel(
            ReadOnlyBuffer<GpuFace> faces,
            ReadOnlyBuffer<Material> materials,
            int width,
            int height,
            float3 cameraPos,
            float3 cameraForward,
            float3 cameraRight,
            float3 cameraUp,
            float fovX,
            float fovY,
            int maxBounces,
            ReadWriteBuffer<float3> accum,
            ReadWriteBuffer<int> sampleCount,
            ReadWriteBuffer<float3> output,
            int frameSampleIndex,
            float antiAliasStrength)
        {
            Faces = faces;
            Materials = materials;
            Width = width;
            Height = height;
            CameraPos = cameraPos;
            CameraForward = cameraForward;
            CameraRight = cameraRight;
            CameraUp = cameraUp;
            FovX = fovX;
            FovY = fovY;
            MaxBounces = maxBounces;

            Accum = accum;
            SampleCount = sampleCount;
            Output = output;

            FrameSampleIndex = frameSampleIndex;
            AntiAliasStrength = antiAliasStrength;
        }

        public void Execute()
        {
            int pixelIndex = ThreadIds.X;
            if ((uint)pixelIndex >= (uint)(Width * Height)) return;

            int x = pixelIndex % Width;
            int y = pixelIndex / Width;

            // Sobol 2D for anti-alias jitter
            float2 jitter = Sobol2D(FrameSampleIndex, x, y);
            float u = (x + (jitter.X - 0.5f) * AntiAliasStrength) / (float)Width;
            float v = 1.0f - ((y + (jitter.Y - 0.5f) * AntiAliasStrength) / (float)Height);

            float3 rayDir = GetRayDirection(u, v);
            float3 rayOrigin = CameraPos;

            float3 color = TraceRay(rayOrigin, rayDir, MaxBounces, FrameSampleIndex, x, y);

            // Accumulate
            float3 prev = Accum[pixelIndex];
            int n = SampleCount[pixelIndex];

            Accum[pixelIndex] = prev + color;
            SampleCount[pixelIndex] = n + 1;
            Output[pixelIndex] = (prev + color) / (float)(n + 1);
        }

        private float3 GetRayDirection(float u, float v)
        {
            float3 dir = CameraForward + CameraRight * ((u - 0.5f) * FovX) + CameraUp * ((v - 0.5f) * FovY);
            return Hlsl.Normalize(dir);
        }

        private float3 TraceRay(float3 origin, float3 direction, int bounces, int sampleIdx, int pixelX, int pixelY)
        {
            float3 currOrigin = origin;
            float3 currDir = direction;
            float3 color = new float3(0, 0, 0);
            float3 throughput = new float3(1, 1, 1);
            float3 background = new float3(0, 0, 0);

            for (int bounce = 0; bounce <= bounces; bounce++)
            {
                float closestT = float.MaxValue;
                int hitFaceIdx = -1;

                for (int i = 0; i < Faces.Length; i++)
                {
                    GpuFace f = Faces[i];
                    if (RayTriangleIntersect(currOrigin, currDir, f.V0, f.V1, f.V2, out float t) && t < closestT)
                    {
                        closestT = t;
                        hitFaceIdx = i;
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

                color += throughput * new float3(mat.EmissionColor.R, mat.EmissionColor.G, mat.EmissionColor.B);
                throughput *= new float3(mat.BaseColor.R, mat.BaseColor.G, mat.BaseColor.B) * mat.Reflectivity;

                float3 normal = face.Normal;
                float3 reflectDir = currDir - 2 * Hlsl.Dot(currDir, normal) * normal;
                float roughness = 1.0f / (mat.Shininess + 1.0f);

                currDir = Hlsl.Normalize(Hlsl.Lerp(reflectDir,
                    RandomHemisphereDirection(normal, pixelX, pixelY, bounce, sampleIdx),
                    roughness));

                currOrigin = hitPoint + currDir * 1e-4f;

                if (bounce >= 4)
                {
                    float p = Hlsl.Clamp(Hlsl.Max(throughput.X, Hlsl.Max(throughput.Y, throughput.Z)), 0.05f, 0.95f);
                    float r = Hlsl.Frac(Hlsl.Sin(Hlsl.Dot(hitPoint, new float3(12.9898f, 78.233f, 37.719f)) + sampleIdx * 13.0f) * 43758.5453f);
                    if (r > p) break;
                    throughput /= p;
                }
            }

            return color;
        }

        private static float2 Sobol2D(int sampleIndex, int x, int y)
        {
            uint seed = (uint)(sampleIndex + x * 73856093 + y * 19349663);
            return new float2(Sobol(seed, 0), Sobol(seed, 1));
        }

        private static float Sobol(uint i, int dimension)
        {
            uint result = 0;
            uint v = dimension == 0 ? 0x80000000u : 0x40000000u;
            for (; i != 0; i >>= 1)
            {
                if ((i & 1) != 0) result ^= v;
                v ^= v >> 1;
            }
            return (float)result / 4294967296f;
        }

        private static float2 Random2D(int seed, int x, int y)
        {
            // Create a unique uint per pixel + sample
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 982451653);
            h = (h ^ (h >> 13)) * 1274126177u;
            h = h ^ (h >> 16);

            // Split into two floats
            uint hx = h & 0xFFFFu;
            uint hy = (h >> 16) & 0xFFFFu;

            float fx = hx / 65536.0f;
            float fy = hy / 65536.0f;

            return new float2(fx, fy);
        }

        private static float3 RandomHemisphereDirection(float3 normal, int pixelX, int pixelY, int bounce, int sampleIdx)
        {
            // Use the new hashed random function
            float2 rnd = Random2D(sampleIdx + bounce * 17, pixelX + bounce * 31, pixelY + bounce * 23);

            float r = Hlsl.Sqrt(rnd.X);
            float theta = 2.0f * 3.14159265f * rnd.Y;

            float x = r * Hlsl.Cos(theta);
            float y = r * Hlsl.Sin(theta);
            float z = Hlsl.Sqrt(Hlsl.Max(0f, 1.0f - rnd.X));

            float3 up = Hlsl.Abs(normal.Z) < 0.999f ? new float3(0, 0, 1) : new float3(1, 0, 0);
            float3 tangent = Hlsl.Normalize(Hlsl.Cross(up, normal));
            float3 bitangent = Hlsl.Cross(normal, tangent);

            return Hlsl.Normalize(x * tangent + y * bitangent + z * normal);
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
    }
}
