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

            int width = 320;
            int height = 180;
            int scale = 3;
            string bitmapId = "gradient";

            // Step 1: Create and save the bitmap

            Scene scene = new Scene();

            Render.RenderScene(scene, width, height, bitmapId);


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
            Debug.LogDiff("MainEnd","MainStart","Main Took: ","s");
        }
    }
}