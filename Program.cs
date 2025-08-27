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
            Stopwatch sw = Stopwatch.StartNew();

            int width = 320;
            int height = 180;
            int scale = 3;
            string bitmapId = "gradient";

            float CON_RenderStart = (float) sw.Elapsed.TotalSeconds;
            Console.WriteLine($"Render Started at : {CON_RenderStart} seconds");
            // Step 1: Create and save the bitmap
            Render.RenderScene(width, height, bitmapId);

            float CON_RenderTotalTime = (float) sw.Elapsed.TotalSeconds - CON_RenderStart;
            
            Console.WriteLine($"Render Ended at : {sw.Elapsed.TotalSeconds:F5} seconds");
            Console.WriteLine($"Render Took : {CON_RenderTotalTime} seconds");
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
            Console.WriteLine($"Drawing Ended at : {sw.Elapsed.TotalSeconds:F5} seconds");
            Application.Run(form);
        }
    }
}