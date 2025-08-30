using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;

namespace Render
{

    class Program
    {
        static void Main()
        {
            Debug.LogNow("Main Start at: ", "s");
            Debug.HoldNow("MainStart");

            int width = 1280;
            int height = 720;
            int scale = 1;
            string bitmapId = "gradient";

            // Step 1: Create and save the bitmap

            Scene scene = new Scene();

            Vec3 CamPos = new Vec3(0, 0, -6);
            Vec3 CamAngle = new Vec3(0, 0, 0); // pitch, yaw (45 deg), roll
            Vec2 CamFov = new Vec2((float)Math.PI / 2f, (float)Math.PI / 2f / ((float)width / (float)height));

            Camera camera = new Camera(CamPos, CamAngle, CamFov);

            Spare.DeleteAllFilesInFolder("output");

            Render.RenderScene(scene, camera, width, height, $"{bitmapId}");


            // Step 2: Load the bitmap
            Bitmap loadedImage = BitmapIO.LoadBitmap(bitmapId);

            // Step 3: Create the form
            Form form = new Form();
            form.Text = "Gradient Bitmap";
            form.ClientSize = new Size(width * scale, height * scale);
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;

            // Step 4: Draw bitmap scaled with nearest-neighbor
            form.Paint += (s, e) =>
            {
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

                e.Graphics.DrawImage(loadedImage, 0, 0, width * scale, height * scale);
            };

            Application.Run(form);
            Debug.LogNow("Main Ended at: ", "s");
            Debug.HoldNow("MainEnd");
            Debug.LogDiff("MainEnd", "MainStart", "Main Took: ", "s");
        }
    }
}