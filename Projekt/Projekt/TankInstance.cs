using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using static System.Formats.Asn1.AsnWriter;

namespace Projekt
{
    internal class TankInstance
    {
        public int Direction = 1;

        public float TargetAngle = 0f;
        public float CurrentAngle = 0f;
        public float WavePhase = 0f;

        public Vector3D<float> CircleCenter;
        public float CircleRadius = 50f;
        public float AngleOnCircle = 0f;
        
        public SceneObject TankBody { get; private set; }
        public List<SceneObject> Wheels { get; private set; } = new List<SceneObject>();
        public Vector3D<float> Velocity { get; set; }
        public float WheelRotationSpeed { get; set; } = 3.5f; 
        public float WheelCircumference { get; } = 7.5f;

        public float MovementAngle { get; set; } = 90f;

        private float shootCooldown = 0f;
        public SceneObject getTank()
        {
            return TankBody;
        }
        public TankInstance(Vector3D<float> position, Vector3D<float> rotation,Vector3D<float> circle)
        {
            CircleCenter = circle;
            TankBody = new SceneObject
            {
                Position = position,
                Rotation = rotation,
                Scale = new Vector3D<float>(3.4f, 3.4f, 3.4f)
            };
        }

        public void Update(float deltaTime,double time, Vector3D<float> playerPos, List<EnemyAmmoInstance> enemyAmmos)
        {
            float targetAngle = MathF.Atan2(Velocity.X, Velocity.Z) * (180f / MathF.PI);
            float angleDiff = targetAngle - TankBody.Rotation.Y;
            while (angleDiff > 180) angleDiff -= 360;
            while (angleDiff < -180) angleDiff += 360;
            float turnSpeed = 30f * deltaTime;
            TankBody.Rotation.Y += Clamp(angleDiff, -turnSpeed, turnSpeed);

            float distance = Vector3D.Distance(TankBody.Position, playerPos);
            shootCooldown -= deltaTime;

            if (distance < 80f && shootCooldown <= 0f)
            {
                shootCooldown = 0.5f;
                enemyAmmos.Add(new EnemyAmmoInstance(TankBody.Position, playerPos));
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
