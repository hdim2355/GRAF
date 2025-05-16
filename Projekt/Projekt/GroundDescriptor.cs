using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using StbImageSharp;
using System;
using System.IO;
using StbImageSharp;
using System.IO;
using System.Reflection;
using Szeminarium1_24_03_05_2;

namespace Projekt
{
    internal class GroundDescriptor
    {
        public unsafe static GlObject CreateGround(GL Gl)
        {
            float size = 1000f; 

            float[] vertices = {
            -size, 0f, -size,   0f, 1f, 0f,        0f, 0f,
             size, 0f, -size,   0f, 1f, 0f,        1f, 0f,
             size, 0f,  size,   0f, 1f, 0f,        1f, 1f,
            -size, 0f,  size,   0f, 1f, 0f,        0f, 1f,
        };

            float[] colorArray = {
            1f, 1f, 1f, 1f,
            1f, 1f, 1f, 1f,
            1f, 1f, 1f, 1f,
            1f, 1f, 1f, 1f
        };

            uint[] indices = {
            0, 1, 2,
            2, 3, 0
        };

            var image = ReadTextureImage("Rock062_2K-PNG_Color.png");

            return CreateObjectDescriptorFromArrays(Gl, vertices, colorArray, indices, image);
        }

        private static ImageResult ReadTextureImage(string textureResource)
        {
            using Stream stream = typeof(GroundDescriptor).Assembly.GetManifestResourceStream("Projekt.Resources." + textureResource)
                ?? throw new FileNotFoundException("Nem található: " + textureResource);
            return ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        }

        private static unsafe GlObject CreateObjectDescriptorFromArrays(GL Gl, float[] vertexArray, float[] colorArray, uint[] indexArray,
            ImageResult textureImage = null)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            // 0 is position
            // 2 is normals
            // 3 is texture
            uint offsetPos = 0;
            uint offsetNormals = offsetPos + 3 * sizeof(float);
            uint offsetTexture = offsetNormals + 3 * sizeof(float);
            uint vertexSize = offsetTexture + (textureImage == null ? 0u : 2 * sizeof(float));

            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexture);
            Gl.EnableVertexAttribArray(3);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);


            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            // 1 is color
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            uint? texture = new uint?();

            if (textureImage != null)
            {
                // set texture
                // create texture
                texture = Gl.GenTexture();

                // activate texture 0
                Gl.ActiveTexture(TextureUnit.Texture0);
                // bind texture
                Gl.BindTexture(TextureTarget.Texture2D, texture.Value);
                // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)textureImage.Width,
                    (uint)textureImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan<byte>)textureImage.Data.AsSpan());
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                // unbinde texture
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);

                Gl.BindTexture(TextureTarget.Texture2D, 0);
            }

            GlObject skyBoxGl = new GlObject(vao, vertices, colors, indices, (uint)indexArray.Length, Gl);
            skyBoxGl.Texture = texture;
            return skyBoxGl;
        }

    }
}
