using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace Projekt
{
    public enum PickupType { Ammo, Bomb }
    internal class Pickup
    {
        public SceneObject Scene;
        public PickupType Type;
        private float rotationSpeed = 90f; // fok/sec

        public Pickup(Vector3D<float> position, PickupType type)
        {
            Scene = new SceneObject
            {
                Position = position,
                Scale = new Vector3D<float>(0.4f, 0.4f, 0.4f)
            };
            Type = type;
        }

        public void Update(float deltaTime)
        {
            Scene.Rotation.Y += rotationSpeed * deltaTime;
            if (Scene.Rotation.Y > 360) Scene.Rotation.Y -= 360;
        }
    }

}
