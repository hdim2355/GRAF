using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt
{
    internal class PlayerPlane
    {
        public SceneObject Scene;
        public int Health = 10;

        public PlayerPlane(SceneObject planeObject)
        {
            Scene = planeObject;
        }

        public void TakeDamage()
        {
            Health--;
            if (Health <= 0)
            {
                Console.WriteLine("Player destroyed!");
                // Trigger explosion, game over, stb.
            }
        }
    }
}
