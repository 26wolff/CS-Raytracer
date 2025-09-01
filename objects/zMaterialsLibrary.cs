using System.Collections.Generic;

namespace Render
{
    public static class MaterialsLibrary
    {
        public static readonly Material Mirror = new Material(
            baseColor: new ColorRGB(1f, 1f, 1f),
            specularColor: new ColorRGB(1f, 1f, 1f),
            ambientColor: new ColorRGB(0f, 0f, 0f),
            emissionColor: new ColorRGB(0f, 0f, 0f),
            shininess: 1f,
            reflectivity: 1f,
            transparency: 0f,
            refractiveIndex: 1f
        );

        public static readonly Material Red = new Material(
            baseColor: new ColorRGB(1f, 0f, 0f),
            specularColor: new ColorRGB(0.8f, 0.8f, 0.8f),
            ambientColor: new ColorRGB(0.1f, 0.1f, 0.1f),
            emissionColor: new ColorRGB(0f, 0f, 0f),
            shininess: 0.3f,
            reflectivity: 0.2f,
            transparency: 0f,
            refractiveIndex: 1f
        );

        public static readonly Material Blue = new Material(
            baseColor: new ColorRGB(0f, 0f, 1f),
            specularColor: new ColorRGB(0.8f, 0.8f, 0.8f),
            ambientColor: new ColorRGB(0.1f, 0.1f, 0.1f),
            emissionColor: new ColorRGB(0f, 0f, 0f),
            shininess: 0.3f,
            reflectivity: 0.2f,
            transparency: 0f,
            refractiveIndex: 1f
        );

        public static readonly Material Green = new Material(
            baseColor: new ColorRGB(0f, 1f, 0f),
            specularColor: new ColorRGB(0.8f, 0.8f, 0.8f),
            ambientColor: new ColorRGB(0.1f, 0.1f, 0.1f),
            emissionColor: new ColorRGB(0f, 0f, 0f),
            shininess: 0.3f,
            reflectivity: 0.2f,
            transparency: 0f,
            refractiveIndex: 1f
        );
        public static readonly Material Black = new Material(
            baseColor: new ColorRGB(0f, 0f, 0f),
            specularColor: new ColorRGB(0.8f, 0.8f, 0.8f),
            ambientColor: new ColorRGB(0.1f, 0.1f, 0.1f),
            emissionColor: new ColorRGB(0f, 0f, 0f),
            shininess: 0.3f,
            reflectivity: 0.2f,
            transparency: 0f,
            refractiveIndex: 1f
        );
        public static readonly Material DGrey = new Material(
            baseColor: new ColorRGB(0.4f, 0.4f, 0.4f),
            specularColor: new ColorRGB(0.8f, 0.8f, 0.8f),
            ambientColor: new ColorRGB(0.4f, 0.4f, 0.4f),
            emissionColor: new ColorRGB(0f, 0f, 0f),
            shininess: 0.3f,
            reflectivity: 0.2f,
            transparency: 0f,
            refractiveIndex: 1f
        );

        public static readonly Material White = new Material(
    baseColor: new ColorRGB(1f, 1f, 1f),          // pure white
    specularColor: new ColorRGB(0.2f, 0.2f, 0.2f), // low specular highlight
    ambientColor: new ColorRGB(0.1f, 0.1f, 0.1f),  // small ambient reflection
    emissionColor: new ColorRGB(0.01f, 0.01f, 0.01f),       // no self-emission
    shininess: 0.05f,                              // almost matte
    reflectivity: 0.05f,                            // mostly diffuse, tiny reflection
    transparency: 0f,                              // opaque
    refractiveIndex: 1f                             // irrelevant since opaque
);


        public static readonly Material Debug = new Material(
            baseColor: new ColorRGB(0.75f, 0f, 1f),
            specularColor: new ColorRGB(0.71f, 0f, 1f),
            ambientColor: new ColorRGB(0.1f, 0.1f, 0.1f),
            emissionColor: new ColorRGB(0.71f, 0f, 1f),
            shininess: 0.3f,
            reflectivity: 0.2f,
            transparency: 0f,
            refractiveIndex: 1f
        );

        public static readonly Material WhiteLight = new Material(
            baseColor: new ColorRGB(1f, 1f, 1f),       // white base
            specularColor: new ColorRGB(1f, 1f, 1f),   // bright highlights
            ambientColor: new ColorRGB(1f, 1f, 1f),    // fully lit under ambient light
            emissionColor: new ColorRGB(15f, 15f, 15f),   // emits white light
            shininess: 1f,                             // mirror-like sharpness
            reflectivity: 0f,                          // not reflective, just emits light
            transparency: 0f,                          // opaque
            refractiveIndex: 1f                         // normal light propagation
        );

        // Dictionary for string lookup
        private static readonly Dictionary<string, Material> materialLookup = new Dictionary<string, Material>()
        {
            { "Mirror", Mirror },
            { "WhiteLight", WhiteLight },
            { "Debug", Debug },
            { "Red", Red },
            { "White", White },
            { "Black", Black },
            { "DGrey", DGrey },
            { "Green", Green },
            { "Blue", Blue}
            // Add more as needed
        };

        // Get material by name
        public static Material GetMaterialByName(string name)
        {
            if (materialLookup.TryGetValue(name, out var mat))
                return mat;
            else if (materialLookup.TryGetValue("Debug", out var debugmat))
                return debugmat;
            else
            {
                throw new KeyNotFoundException($"Material '{name}' not found in MaterialsLibrary.");
            }
        }
    }
}