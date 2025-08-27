using System;
using System.IO;
using System.Drawing;


public static class BitmapIO
{
    // Define the output folder
    private static readonly string OutputFolder = Path.Combine(Environment.CurrentDirectory, "output");

    static BitmapIO()
    {
        // Create the folder if it doesn't exist
        if (!Directory.Exists(OutputFolder))
            Directory.CreateDirectory(OutputFolder);
    }

    // Save a bitmap to a file inside the output folder
    public static void SaveBitmap(string id, Bitmap bitmap)
    {
        string filename = Path.Combine(OutputFolder, $"{id}.png");
        bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
    }

    // Load a bitmap from the output folder
    public static Bitmap LoadBitmap(string id)
    {
        string filename = Path.Combine(OutputFolder, $"{id}.png");
        return new Bitmap(filename);
    }
}
