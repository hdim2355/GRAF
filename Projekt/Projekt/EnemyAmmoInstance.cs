using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace Projekt
{
    internal class EnemyAmmoInstance
    {
        public SceneObject Scene;
        public Vector3D<float> Direction;
        public float Speed = 120f;

        public EnemyAmmoInstance(Vector3D<float> startPos, Vector3D<float> targetPos)
        {
            Scene = new SceneObject
            {
                Position = startPos,
                Scale = new Vector3D<float>(60.3f, 60.3f, 60.3f)
            };

            Direction = Vector3D.Normalize(targetPos - startPos);
        }

        public void Update(float deltaTime)
        {
            Scene.Position += Direction * Speed * deltaTime;
        }
    }
}
