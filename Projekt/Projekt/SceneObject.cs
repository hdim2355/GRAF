using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace Projekt
{
    internal class SceneObject
    {
        public Vector3D<float> Position = Vector3D<float>.Zero;
        public Vector3D<float> Rotation = Vector3D<float>.Zero;
        public Vector3D<float> Scale = new Vector3D<float>(1f, 1f, 1f);

        private float DegreesToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180f);
        }

        public Matrix4X4<float> GetModelMatrix()
        {
            var scaleMatrix = Matrix4X4.CreateScale(Scale);
            var rotationMatrix =
                Matrix4X4.CreateRotationX(DegreesToRadians(Rotation.X)) *
                Matrix4X4.CreateRotationY(DegreesToRadians(Rotation.Y)) *
                Matrix4X4.CreateRotationZ(DegreesToRadians(Rotation.Z));
            var translationMatrix = Matrix4X4.CreateTranslation(Position);

            return scaleMatrix * rotationMatrix * translationMatrix;
        }

        public Vector3D<float> GetForwardDirection()
        {
            float yaw = DegreesToRadians(Rotation.Y);
            float pitch = DegreesToRadians(Rotation.X);

            return new Vector3D<float>(
                -MathF.Sin(yaw) * MathF.Cos(pitch),
                 MathF.Sin(pitch),
                -MathF.Cos(yaw) * MathF.Cos(pitch)
            );
        }
    }
}
