using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace Projekt
{
    public class BombInstance
    {
        public SceneObject Scene { get; private set; }

        private Vector3D<float> velocity;
        private const float Gravity = -9.81f;

        public BombInstance(Vector3D<float> startPosition, Vector3D<float> initialVelocity)
        {
            Scene = new SceneObject
            {
                Position = startPosition,
                Rotation = new Vector3D<float>(0, 0, 0),
                Scale = new Vector3D<float>(0.04f, 0.04f, 0.04f)
            };

            velocity = initialVelocity;
        }

        public void Update(float deltaTime)
        {
            velocity.Y += Gravity * deltaTime;
            Scene.Position += velocity * deltaTime;

            if (Scene.Position.Y <= -10f)
            {
                Scene.Position.Y = -10f;
                velocity = Vector3D<float>.Zero;
            }
        }
    }
}
