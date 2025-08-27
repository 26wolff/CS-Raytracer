using System;
using System.Drawing;

namespace Render
{
    public static class Render
    {
        public static void RenderScene(int width, int height, string id)
        {
            Bitmap bmp = new Bitmap(width, height);
            
            Vec3 CamPos = new Vec3(0, 0, -3);
            Vec3 CamAngle = new Vec3(0, 0, 0);
            Vec2 CamFov = new Vec2((float)Math.PI / 2f, (float)Math.PI / 2f / ((float)width / (float)height));

            Console.WriteLine(
                $"Camera FOV -> X (horizontal): {CamFov.x:F3} rad, Y (vertical): {CamFov.y:F3} rad"
            );

            Camera camera = new Camera(CamPos, CamAngle, CamFov);



            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Ray ray = camera.GetRay((float)x / width, (float)y / height);

                    Vec3 PixelColor = RenderPixel(ray);
                    bmp.SetPixel(x, y, Color.FromArgb((int)PixelColor.x, (int)PixelColor.y, (int)PixelColor.z));
                }
            }

            // Save using BitmapIO
            BitmapIO.SaveBitmap(id, bmp);
        }
        public static Vec3 RenderPixel(Ray ray)
        {

                return new Vec3(255, 255, 255); // white for miss
            
        }

    }
}