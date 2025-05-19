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

        private float frequency = 1.5f;

        private Vector2D<float> center;
        private float a = 60f; 
        private float b = 30f; 

        public EnemyPlaneInstance(Vector3D<float> position)
        {
            Scene = new SceneObject
            {
                Position = position,
                Rotation = new Vector3D<float>(0f, 180f, 0f),
                Scale = new Vector3D<float>(0.4f, 0.4f, 0.4f)
            };

            // Ellipszis középpont
            center = new Vector2D<float>(position.X, position.Z);
        }

        public void Update(float deltaTime, double time)
        {
            float angle = (float)time * frequency;

            Scene.Position.X = MathF.Cos(angle) * a + center.X;
            Scene.Position.Z = MathF.Sin(angle) * b + center.Y;

            Vector2D<float> dir = new Vector2D<float>(-MathF.Sin(angle), MathF.Cos(angle));
            Scene.Rotation.Y = MathF.Atan2(dir.X, dir.Y) * (180f / MathF.PI);
        }

        public void TakeDamage()
        {
            HitPoints--;
        }

        public bool IsDestroyed => HitPoints <= 0;
    }

}
