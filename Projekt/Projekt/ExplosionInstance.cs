using System;
using System.Collections.Generic;
using Silk.NET.Maths;

namespace Projekt
{
    public class ExplosionInstance
    {
        private class Particle
        {
            public SceneObject Scene;
            public Vector3D<float> Velocity;
            public float Lifetime;
        }

        private List<Particle> particles = new();
        private const int ParticleCount = 30;
        private const float MaxLifetime = 2.0f;
        private const float Speed = 5.0f;

        public bool IsAlive => particles.Count > 0;

        public ExplosionInstance(Vector3D<float> position)
        {
            var rand = new Random();
            for (int i = 0; i < ParticleCount; i++)
            {
                var dir = RandomDirection(rand);
                var velocity = dir * (float)(rand.NextDouble() * Speed);

                particles.Add(new Particle
                {
                    Scene = new SceneObject
                    {
                        Position = position,
                        Rotation = new Vector3D<float>(0, 0, 0),
                        Scale = new Vector3D<float>(0.55f, 0.55f, 0.55f)
                    },
                    Velocity = velocity,
                    Lifetime = MaxLifetime
                });
            }
        }

        public void Update(float deltaTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Velocity.Y += -9.81f * deltaTime; // gravity
                p.Scene.Position += p.Velocity * deltaTime;
                p.Lifetime -= deltaTime;

                if (p.Lifetime <= 0)
                    particles.RemoveAt(i);
            }
        }

        public IEnumerable<SceneObject> GetParticles()
        {
            foreach (var p in particles)
                yield return p.Scene;
        }

        private Vector3D<float> RandomDirection(Random rand)
        {
            float x = (float)(rand.NextDouble() * 2 - 1);
            float y = (float)(rand.NextDouble() * 2 - 1);
            float z = (float)(rand.NextDouble() * 2 - 1);
            var dir = new Vector3D<float>(x, y, z);
            return Vector3D.Normalize(dir);
        }
    }
}
