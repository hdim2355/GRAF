﻿using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using StbImageSharp; // Make sure you have this

namespace Szeminarium1_24_03_05_2
{
    internal class ObjectResourceReader
    {
        public class FaceData
        {
            public int[] Vertices;
            public string Material;
            public string Group;
            public string Smoothing;
            public FaceData(int[] v, string mat, string g, string s)
            {
                Vertices = v;
                Material = mat;
                Group = g;
                Smoothing = s;
            }
        }

        public class MaterialData
        {
            public string Name;
            public Vector4D<float> DiffuseColor;
            public string? DiffuseTexturePath;
            public uint? TextureID;

            public MaterialData(string name, Vector4D<float> diffuse, string? texturePath = null)
            {
                Name = name;
                DiffuseColor = diffuse;
                DiffuseTexturePath = texturePath;
            }
        }

        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream textureStream =
                typeof(GlObject).Assembly.GetManifestResourceStream("Szeminarium1_24_03_05_2.textures." + textureResource.Replace('/', '.')))
            {
                if (textureStream == null)
                    throw new FileNotFoundException($"Resource not found: {textureResource}");

                result = ImageResult.FromStream(textureStream, ColorComponents.RedGreenBlueAlpha);
            }

            return result;
        }


        public static unsafe GlObject CreateObjectFromResource(GL Gl, string resourceName)
        {
            List<float[]> objVertices = new();
            List<float[]> objNormals = new();
            List<float[]> objTexCoords = new();
            Dictionary<string, MaterialData> materials = new();

            string fullResourceName = "Szeminarium1_24_03_05_2.Resources." + resourceName;
            string currentMaterial = "default";

            var vertexMap = new Dictionary<string, uint>();
            var uniqueVertices = new List<float>();
            var uniqueUVs = new List<float>();
            var uniqueNormals = new List<float>();
            var uniqueColors = new List<float>();
            var indices = new List<uint>();

            using var objStream = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream(fullResourceName);
            using var objReader = new StreamReader(objStream);

            while (!objReader.EndOfStream)
            {
                var line = objReader.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                int spaceIndex = line.IndexOf(' ');
                string lineClassifier = spaceIndex == -1 ? line : line.Substring(0, spaceIndex);
                string[] lineData = spaceIndex == -1 ? Array.Empty<string>() : line.Substring(spaceIndex + 1).Trim().Split(' ');

                switch (lineClassifier)
                {
                    case "v": objVertices.Add(ParseFloatArray(lineData, 3)); break;
                    case "vn": objNormals.Add(ParseFloatArray(lineData, 3)); break;
                    case "vt": objTexCoords.Add(ParseFloatArray(lineData, 2)); break;
                    case "f":
                        if (lineData.Length < 3) break;
                        for (int i = 1; i < lineData.Length - 1; i++)
                        {
                            int[] tri = { 0, i, i + 1 };
                            foreach (int j in tri)
                            {
                                string key = lineData[j];
                                if (!vertexMap.ContainsKey(key))
                                {
                                    var parts = key.Split('/');
                                    int vi = int.Parse(parts[0]) - 1;
                                    int ti = parts.Length > 1 && parts[1] != "" ? int.Parse(parts[1]) - 1 : -1;
                                    int ni = parts.Length > 2 ? int.Parse(parts[2]) - 1 : -1;

                                    var v = objVertices[vi];
                                    uniqueVertices.AddRange([v[0], v[1], v[2]]);

                                    if (ni >= 0 && ni < objNormals.Count)
                                        uniqueNormals.AddRange(objNormals[ni]);
                                    else
                                        uniqueNormals.AddRange([0, 0, 0]);

                                    if (ti >= 0 && ti < objTexCoords.Count)
                                        uniqueUVs.AddRange(objTexCoords[ti]);
                                    else
                                        uniqueUVs.AddRange([0, 0]);

                                    Vector4D<float> color = materials.ContainsKey(currentMaterial) ? materials[currentMaterial].DiffuseColor : new(1f, 0f, 0f, 1f);
                                    uniqueColors.AddRange([color.X, color.Y, color.Z, color.W]);

                                    vertexMap[key] = (uint)vertexMap.Count;
                                }
                                indices.Add(vertexMap[key]);
                            }
                        }
                        break;
                    case "usemtl": currentMaterial = string.Join(" ", lineData); break;
                    case "mtllib": LoadMaterialLibrary(Gl, "Szeminarium1_24_03_05_2.Resources." + string.Join(" ", lineData), materials); break;
                }
            }

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vertexSize = 6 * sizeof(float);
            List<float> vertexBuffer = new();
            for (int i = 0; i < uniqueVertices.Count / 3; i++)
            {
                vertexBuffer.AddRange(uniqueVertices.Skip(i * 3).Take(3));
                vertexBuffer.AddRange(uniqueNormals.Skip(i * 3).Take(3));
            }

            uint vbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertexBuffer.ToArray(), BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)0);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)(3 * sizeof(float)));
            Gl.EnableVertexAttribArray(2);

            uint cbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, cbo);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)uniqueColors.ToArray(), BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint tbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, tbo);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)uniqueUVs.ToArray(), BufferUsageARB.StaticDraw);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(3);

            uint ebo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (ReadOnlySpan<uint>)indices.ToArray(), BufferUsageARB.StaticDraw);

            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            Gl.BindVertexArray(0);

            return new GlObject(vao, vbo, cbo, ebo, (uint)indices.Count, Gl);
        }

        private static float[] ParseFloatArray(string[] data, int count)
        {
            float[] result = new float[count];
            for (int i = 0; i < count && i < data.Length; ++i)
                result[i] = float.Parse(data[i], CultureInfo.InvariantCulture);
            return result;
        }

        private static void LoadMaterialLibrary(GL Gl, string mtlResourceName, Dictionary<string, MaterialData> materials)
        {
            using var mtlStream = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream(mtlResourceName);
            if (mtlStream == null) return;

            using var reader = new StreamReader(mtlStream);
            string? currentName = null;
            Vector4D<float> currentColor = new(1f, 1f, 1f, 1f);
            string? textureFile = null;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "newmtl":
                        if (currentName != null)
                        {
                            materials[currentName] = new MaterialData(currentName, currentColor, textureFile);
                            if (textureFile != null)
                            {
                                var image = ReadTextureImage(textureFile);
                                var texID = Gl.GenTexture();
                                Gl.ActiveTexture(TextureUnit.Texture0);
                                Gl.BindTexture(TextureTarget.Texture2D, texID);
                                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan<byte>)image.Data.AsSpan());
                                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                                Gl.BindTexture(TextureTarget.Texture2D, 0);
                                materials[currentName].TextureID = texID;
                            }
                        }
                        currentName = parts[1];
                        textureFile = null;
                        break;
                    case "Kd":
                        currentColor = new Vector4D<float>(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture),
                            1f);
                        break;
                    case "map_Kd":
                        textureFile = parts[1];
                        break;
                }
            }

            if (currentName != null)
            {
                materials[currentName] = new MaterialData(currentName, currentColor, textureFile);
                if (textureFile != null)
                {
                    var image = ReadTextureImage(textureFile);
                    var texID = Gl.GenTexture();
                    Gl.ActiveTexture(TextureUnit.Texture0);
                    Gl.BindTexture(TextureTarget.Texture2D, texID);
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan<byte>)image.Data.AsSpan());
                    Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    Gl.BindTexture(TextureTarget.Texture2D, 0);
                    materials[currentName].TextureID = texID;
                }
            }
        }
    }
}