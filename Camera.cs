using System;
namespace Render
{
    public class Camera
    {
        // Position in world space
        public Vec3 Position { get; set; } = new();

        // Rotation angles (Pitch, Yaw, Roll) in radians
        public Vec3 Rotation { get; set; } = new();

        // Fov angles (FovX, FovY) in radians
        public Vec2 Fov { get; set; } = new();

        // Constructor
        public Camera(Vec3 position, Vec3 rotation, Vec2 fov)
        {
            Position = position;
            Rotation = rotation;
            Fov = fov;
        }

        // Default constructor
        public Camera() { }

        // Example: Move camera by offset
        public void Move(Vec3 offset)
        {
            Position += offset;
        }

        // Example: Rotate camera by offset angles
        public void Rotate(Vec3 deltaAngles)
        {
            Rotation += deltaAngles;
        }

        public Ray GetRay(float x, float y)
        {
            Ray ray = new Ray
            {
                Position = this.Position,
                Rotation = this.Rotation + new Vec3((this.Fov.x * x) - this.Fov.x / 2, (this.Fov.y * (1 - y)) - this.Fov.y / 2, 0)
            };

            return ray;

        }

    }

}