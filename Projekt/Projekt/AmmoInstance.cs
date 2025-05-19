using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace Projekt
{
    internal class AmmoInstance
    {
        public SceneObject Scene;
        public Vector3D<float> Velocity;
        
        public AmmoInstance(Vector3D<float> startPosition, Vector3D<float> direction)
        {
            Scene = new SceneObject
            {
                Position = startPosition,
                Rotation = new Vector3D<float>(0, 0, 0),
                Scale = new Vector3D<float>(10.1f, 10.1f, 10.1f)
            };

            Velocity = direction * 60f; // gyorsabb mint a bomba
        }

        public void Update(float deltaTime)
        {
            Scene.Position += Velocity * deltaTime;
        }
    }
}
