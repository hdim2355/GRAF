using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace Projekt
{
    internal class EnemyPlaneInstance
    {
        public SceneObject Scene;
        public int HitPoints = 6;

        private float moveSpeed = 10f;
        private float amplitude = 20f;
        private float frequency = 2f;
        private float baseZ;

        public EnemyPlaneInstance(Vector3D<float> position)
        {
            Scene = new SceneObject
            {
                Position = position,
                Rotation = new Vector3D<float>(0f, 180f, 0f),
                Scale = new Vector3D<float>(0.4f, 0.4f, 0.4f)
            };

            baseZ = position.Z;
        }

        public void Update(float deltaTime, double time)
        {
            Scene.Position.Z = baseZ + MathF.Sin((float)time * frequency) * amplitude;
        }

        public void TakeDamage()
        {
            HitPoints--;
        }

        public bool IsDestroyed => HitPoints <= 0;
    }
}
