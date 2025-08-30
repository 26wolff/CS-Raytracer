using ComputeSharp;
using System.Numerics; // for Vector3

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
        public readonly ReadWriteBuffer<float3> Output;
        // Stores hit info for each bounce
        public readonly ReadWriteBuffer<RayHit> Hits;

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
            ReadWriteBuffer<float3> output,
            ReadWriteBuffer<RayHit> hits)
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
            Output = output;
            Hits = hits;
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


            int hitIndex = TraceRay(rayOrigin, rayDir, MaxBounces);
            float3 color = ComputeColor(hitIndex, MaxBounces);
            Output[pixelIndex] = color;
        }

        private float3 ComputeColor(int hitIndex, int MaxBounces)
        {
            if (hitIndex == 0) return new float3(0f, 0f, 0f);

            ColorRGB hue = Materials[Hits[hitIndex].Face.Material].BaseColor;

            return ColorRGBToFloat3(hue);
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
        private int TraceRay(float3 origin, float3 direction, int bounces)
        {
            float3 hitColor = new float3(0f, 0f, 0f);
            float3 sunNormal = new float3(0f, -1f, 0f);
            float3 currOrigin = origin;
            float3 currDir = direction;
            int hitIndex = 0;
            for (int bounce = 0; bounce <= bounces; bounce++)
            {
                float closestT = float.MaxValue;
                int hitFace = -1;
                for (int i = 0; i < Faces.Length; i++)
                {
                    GpuFace face = Faces[i];
                    if (RayTriangleIntersect(currOrigin, currDir, face.V0, face.V1, face.V2, out float t))
                    {
                        if (t < closestT)
                        {
                            closestT = t;
                            hitFace = i;
                        }
                    }
                }
                if (hitFace != -1)
                {
                    GpuFace face = Faces[hitFace];
                    float3 hitPoint = currOrigin + currDir * closestT;
                    // Store hit info for this pixel only

                    Hits[hitIndex].Face = face;
                    Hits[hitIndex].HitPoint = hitPoint;
                    hitIndex++;


                    float3 reflectDir = Hlsl.Reflect(currDir, face.Normal);
                    currOrigin = hitPoint + reflectDir * 1e-4f;
                    currDir = reflectDir;

                }
                else
                {
                    break;
                }
            }
            return hitIndex;
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
    }
}