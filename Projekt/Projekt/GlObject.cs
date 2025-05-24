using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;

namespace Projekt
{
    internal class GlObject
    {
        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }
        public uint? Texture { get; set; } = new uint?();

        private GL Gl;

        public GlObject(uint vao, uint verts, uint colors, uint indices, uint indexArrayLength, GL gl)
        {
            Vao = vao;
            Vertices = verts;
            Colors = colors;
            Indices = indices;
            IndexArrayLength = indexArrayLength;
            this.Gl = gl;
        }

        public void Release()
        {
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Colors);
            Gl.DeleteBuffer(Indices);
            Gl.DeleteVertexArray(Vao);
        }
    }
}
