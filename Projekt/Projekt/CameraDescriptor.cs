using Projekt;
using Silk.NET.Maths;

namespace Szeminarium1_24_03_05_2
{
    internal class CameraDescriptor
    {
        private Vector3D<float> position = new(0, 2.5f, -3.5f);
        private Vector3D<float> target = Vector3D<float>.Zero;
        private Vector3D<float> upVector = Vector3D<float>.UnitY;

        public Vector3D<float> Position => position;
        public Vector3D<float> Target => target;
        public Vector3D<float> UpVector => upVector;

        public void FollowObject(SceneObject obj, float distanceBehind = -5.0f, float heightOffset = 2.0f)
        {
            var forward = obj.GetForwardDirection();
            var up = obj.GetUpDirection();

            position = obj.Position - forward * distanceBehind + up * heightOffset;
            target = obj.Position;
            upVector = up;
        }
    }
}
