using Projekt;
using Silk.NET.Maths;

namespace Szeminarium1_24_03_05_2
{
    internal class CameraDescriptor
    {
        private Vector3D<float> position = new(0, 480.0f, 0f);
        private Vector3D<float> target = Vector3D<float>.Zero;
        private Vector3D<float> upVector = Vector3D<float>.UnitY;

        public float distanceBehind { get; set; } = -1.0f;
        public float heightOffset { get; set; } = 0.0f;
        public Vector3D<float> GetVector() { return position; }
        public Vector3D<float> Position => position;
        public Vector3D<float> Target => target;
        public Vector3D<float> UpVector => upVector;

        public void FollowObject(SceneObject obj)
        {
            var forward = obj.GetForwardDirection();
            var up = obj.GetUpDirection();

            position = obj.Position - forward * distanceBehind + up * heightOffset;
            target = obj.Position;
            upVector = up;
        }

    }
}
