using System;
using System.Drawing;
using Render.GpuShaders;
using System.Collections.Generic;
using ComputeSharp;

namespace Render
{
    public static class Render
    {
        public static void RenderScene(Scene scene, Camera camera, int width, int height, string id)
        {
            Debug.LogNow("Render Started: ", "s");
            Debug.HoldNow("RenderStart");

            Bitmap bmp = new Bitmap(width, height);

            // Prepare camera vectors for GPU
            Vec3 forward = GetForwardFromRotation(camera.Rotation);
            Vec3 right = GetRightFromRotation(camera.Rotation);
            Vec3 up = GetUpFromRotation(camera.Rotation);

            float3 camPos3 = new float3(camera.Position.x, camera.Position.y, camera.Position.z);
            float3 camForward3 = new float3(forward.x, forward.y, forward.z);
            float3 camRight3 = new float3(right.x, right.y, right.z);
            float3 camUp3 = new float3(up.x, up.y, up.z);

            int pixelCount = width * height;
            float antiAliasStrength = 0.5f;
            int maxBounces = 16;
            int totalSamples = 1024; // total progressive samples

            var device = GraphicsDevice.GetDefault();

            using ReadOnlyBuffer<GpuFace> facesBuffer = device.AllocateReadOnlyBuffer(scene.GpuFaces.ToArray());
            using ReadOnlyBuffer<Material> materialBuffer = device.AllocateReadOnlyBuffer(scene.MaterialList.ToArray());

            using ReadWriteBuffer<float3> accumBuffer = device.AllocateReadWriteBuffer<float3>(pixelCount);
            using ReadWriteBuffer<int> sampleCountBuffer = device.AllocateReadWriteBuffer<int>(pixelCount);
            using ReadWriteBuffer<float3> outputBuffer = device.AllocateReadWriteBuffer<float3>(pixelCount);

            // Initialize accum/sampleCount buffers to zero
            device.For(pixelCount, new ClearFloat3Kernel(accumBuffer));
            device.For(pixelCount, new ClearIntKernel(sampleCountBuffer));

            // Progressive accumulation loop
            for (int frameSampleIndex = 0; frameSampleIndex < totalSamples; frameSampleIndex++)
            {
                device.For(pixelCount, new RaytraceKernel(
                    facesBuffer,
                    materialBuffer,
                    width,
                    height,
                    camPos3,
                    camForward3,
                    camRight3,
                    camUp3,
                    camera.Fov.x,
                    camera.Fov.y,
                    maxBounces,
                    accumBuffer,
                    sampleCountBuffer,
                    outputBuffer,
                    frameSampleIndex,
                    antiAliasStrength
                ));

                if (frameSampleIndex % 5 == 0 || frameSampleIndex == totalSamples - 1)
                {
                    PrintProgress(frameSampleIndex + 1, totalSamples);
                }
            }

            // Copy results from GPU to CPU
            var results = outputBuffer.ToArray();

            for (int i = 0; i < pixelCount; i++)
            {
                int x = i % width;
                int y = i / width;

                var result = results[i];
                bmp.SetPixel(x, y, Color.FromArgb(
                    (int)Math.Clamp(result.X * 255f, 0, 255),
                    (int)Math.Clamp(result.Y * 255f, 0, 255),
                    (int)Math.Clamp(result.Z * 255f, 0, 255)
                ));
            }

            BitmapIO.SaveBitmap(id, bmp);
            Debug.LogNow("Render Ended at: ", "s");
            Debug.HoldNow("RenderEnd");
            Debug.LogDiff("RenderEnd", "RenderStart", "Render Took: ", "s");
        }

        private static Vec3 GetForwardFromRotation(Vec3 rotation)
        {
            float cosPitch = (float)Math.Cos(rotation.x);
            float sinPitch = (float)Math.Sin(rotation.x);
            float cosYaw = (float)Math.Cos(rotation.y);
            float sinYaw = (float)Math.Sin(rotation.y);

            Vec3 forward = new Vec3(
                sinYaw * cosPitch,
                sinPitch,
                cosYaw * cosPitch
            );
            return Normalize(forward);
        }

        private static Vec3 GetRightFromRotation(Vec3 rotation)
        {
            float cosYaw = (float)Math.Cos(rotation.y);
            float sinYaw = (float)Math.Sin(rotation.y);
            return new Vec3(cosYaw, 0, -sinYaw);
        }

        private static Vec3 GetUpFromRotation(Vec3 rotation)
        {
            return new Vec3(0, 1, 0);
        }

        private static Vec3 Normalize(Vec3 v)
        {
            float mag = (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            if (mag > 1e-6f)
                return new Vec3(v.x / mag, v.y / mag, v.z / mag);
            return new Vec3(0, 0, 0);
        }
        static void PrintProgress(int current, int total)
        {
            int barWidth = 50; // width of the progress bar
            float progress = (float)current / total;
            int pos = (int)(barWidth * progress);

            Console.Write("[");
            for (int i = 0; i < barWidth; i++)
            {
                if (i < pos) Console.Write("=");
                else if (i == pos) Console.Write(">");
                else Console.Write(" ");
            }
            Console.Write($"] {progress * 100:0.0}%\r"); // \r to overwrite the line
            if (current == total) Console.WriteLine();
        }

    }
}
