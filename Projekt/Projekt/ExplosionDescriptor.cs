using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using StbImageSharp;
using System.IO;
using Silk.NET.Vulkan;

namespace Projekt
{
    internal class ExplosionDescriptor
    {
        public static GlObject CreateExplosion(GL Gl)
        {

            float[] vertices = new float[]
            {
                -0.5f, 0.0f, -0.5f,   0, 1, 0,    0, 0,
                 0.5f, 0.0f, -0.5f,   0, 1, 0,    1, 0,
                 0.5f, 0.0f,  0.5f,   0, 1, 0,    1, 1,
                -0.5f, 0.0f,  0.5f,   0, 1, 0,    0, 1
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                2, 3, 0
            };

            float[] colors = new float[]
            {
                1,1,1,1,
                1,1,1,1,
                1,1,1,1,
                1,1,1,1
            };

            ImageResult textureImage;
            using (Stream skyeboxStream
                = typeof(GlObject).Assembly.GetManifestResourceStream("Projekt.Resources.explosion.jpg"))
                textureImage = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return CreateObjectDescriptorFromArrays(Gl, vertices, colors, indices, textureImage);
        }

        private unsafe static GlObject CreateObjectDescriptorFromArrays(GL Gl, float[] vertexArray, float[] colorArray, uint[] indexArray,
            ImageResult textureImage)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertices);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), BufferUsageARB.StaticDraw);

            int stride = 8 * sizeof(float);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
            Gl.EnableVertexAttribArray(0);

            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)(3 * sizeof(float)));
            Gl.EnableVertexAttribArray(2);

            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)(6 * sizeof(float)));
            Gl.EnableVertexAttribArray(3);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, colors);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, (void*)0);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indices);
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), BufferUsageARB.StaticDraw);

            // Textúra betöltés
            uint texture = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2D, texture);
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)textureImage.Width,
                (uint)textureImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan<byte>)textureImage.Data.AsSpan());

            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            Gl.BindTexture(TextureTarget.Texture2D, 0);

            var obj = new GlObject(vao, vertices, colors, indices, (uint)indexArray.Length, Gl);
            obj.Texture = texture;

            return obj;
        }
    }
}
