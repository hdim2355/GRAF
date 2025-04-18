﻿using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrafikaSzeminarium
{
    internal class ModelObjectDescriptor:IDisposable
    {
        private bool disposedValue;

        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }

        private GL Gl;
        public unsafe static ModelObjectDescriptor CreateCube(GL Gl,int x,int y,int z)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);
            Gl.BindVertexArray(vao);
            // counter clockwise is front facing
            
            var vertexArray = new float[] {
                -0.48f, 0.48f, 0.48f,
                0.48f, 0.48f, 0.48f,
                0.48f, 0.48f, -0.48f,
                -0.48f, 0.48f, -0.48f,
                //teteje
                -0.48f, 0.48f, 0.48f,
                -0.48f, -0.48f, 0.48f,
                0.48f, -0.48f, 0.48f,
                0.48f, 0.48f, 0.48f,
                //jobb oldal
                -0.48f, 0.48f, 0.48f,
                -0.48f, 0.48f, -0.48f,
                -0.48f, -0.48f, -0.48f,
                -0.48f, -0.48f, 0.48f,
                //hatso
                -0.48f, -0.48f, 0.48f,
                0.48f, -0.48f, 0.48f,
                0.48f, -0.48f, -0.48f,
                -0.48f, -0.48f, -0.48f,
                //also
                0.48f, 0.48f, -0.48f,
                -0.48f, 0.48f, -0.48f,
                -0.48f, -0.48f, -0.48f,
                0.48f, -0.48f, -0.48f,
                //bal oldali
                0.48f, 0.48f, 0.48f,
                0.48f, 0.48f, -0.48f,
                0.48f, -0.48f, -0.48f,
                0.48f, -0.48f, 0.48f,
                //szemben
            };

            float[] colorArray = new float[] {
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

            };

            if (y == 1)
            {
                for(int i = 0; i < 16; i++)
                {
                        colorArray[i] =((i%4==0)? 1.0f :0.0f);
                }
            }
            if (z == 1)
            {
                for (int i = 0; i < 16; i++)
                {
                    colorArray[(1*16)+i] = ((i % 4 == 1) ? 1.0f : 0.0f);
                }
            }
            if (x == -1)
            {
                for (int i = 0; i < 16; i++)
                {
                    colorArray[(2 * 16) + i] = ((i % 4 == 2) ? 1.0f : 0.0f);
                }
            }
            if (y == -1)
            {
                for (int i = 0; i < 16; i++)
                {
                    colorArray[(3*16)+i] = ((i % 4 == 0 || i % 4 ==2) ? 1.0f : 0.0f);
                }
            }
            if (z == -1)
            {
                for (int i = 0; i < 16; i++)
                {
                    colorArray[(4 * 16) + i] = ((i % 4 == 1 || i % 4 == 2) ? 1.0f : 0.0f);
                }
            }
            if (x == 1)
            {
                for (int i = 0; i < 16; i++)
                {
                    colorArray[(5 * 16) + i] = ((i % 4 == 1 || i % 4 == 0) ? 1.0f : 0.0f);
                }
            }

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,

                4, 5, 6,
                4, 6, 7,

                8, 9, 10,
                10, 11, 8,

                12, 14, 13,
                12, 15, 14,

                17, 16, 19,
                17, 19, 18,

                20, 22, 21,
                20, 23, 22
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            return new ModelObjectDescriptor() {Vao= vao, Vertices = vertices, Colors = colors, Indices = indices, IndexArrayLength = (uint)indexArray.Length, Gl = Gl};

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null


                // always unbound the vertex buffer first, so no halfway results are displayed by accident
                Gl.DeleteBuffer(Vertices);
                Gl.DeleteBuffer(Colors);
                Gl.DeleteBuffer(Indices);
                Gl.DeleteVertexArray(Vao);

                disposedValue = true;
            }
        }

        ~ModelObjectDescriptor()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        

    }
}
