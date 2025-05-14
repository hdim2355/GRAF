
using Projekt;
using Silk.NET.Maths;

namespace Szeminarium1_24_03_05_2
{
    internal class CameraDescriptor
    {
        public Vector3D<float> Position = new Vector3D<float>(0, 0, 5);
        public Vector3D<float> Target = Vector3D<float>.Zero;

        public Matrix4X4<float> ViewMatrix { get; private set; }

        public void FollowObject(SceneObject obj, float distanceBehind = 5.0f, float heightOffset = 2.0f)
        {
            Vector3D<float> forward = obj.GetForwardDirection();
            Vector3D<float> behind = obj.Position - forward * distanceBehind;
            Position = new Vector3D<float>(behind.X, behind.Y + heightOffset, behind.Z);
            Target = obj.Position;

            ViewMatrix = Matrix4X4.CreateLookAt(Position, Target, Vector3D<float>.UnitY);
        }
    }
}
