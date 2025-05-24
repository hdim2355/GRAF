using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using StbImageSharp;
using Silk.NET.Maths;

namespace Projekt
{
    internal class AirportDescriptor
    {
        private class VertexTransformation
        {
            public Vector3D<float> Coordinates;
            public Vector3D<float> Normal;
            private int normalContributionCount;

            public VertexTransformation(Vector3D<float> coordinates)
            {
                Coordinates = coordinates;
                Normal = new Vector3D<float>(0, 0, 0);
                normalContributionCount = 0;
            }

            public void UpdateNormalWithContributionFromAFace(Vector3D<float> faceNormal)
            {
                Normal += faceNormal;
                normalContributionCount++;
            }

            public void NormalizeNormal()
            {
                if (normalContributionCount > 0)
                {
                    Normal /= normalContributionCount;
                    Normal = Vector3D.Normalize(Normal);
                }
            }
        }
        public static unsafe GlObject CreateAirportObject(GL Gl)
        {
            List<float[]> objVertices = new();
            List<float[]> objNormals = new();
            List<float[]> objTexCoords = new();
            Dictionary<string, MaterialData> materials = new();
            List<int[]> objFaces = new();
            string currentMaterial = "default";

            var vertexMap = new Dictionary<string, uint>();
            var uniqueVertices = new List<float>();
            var uniqueNormals = new List<float>();
            var uniqueUVs = new List<float>();
            var uniqueColors = new List<float>();
            var indices = new List<uint>();
            List<float[]> objUVs = new List<float[]>();
            List<(int vertexIndex, int texCoordIndex, int normalIndex)> vertexFullAssignments = new();
            List<(int vertexIndex, int normalIndex)> vertexNormalAssignments = new();

            string fullResourceName = "Projekt.Resources.car.obj";

            using var objStream = typeof(AirportDescriptor).Assembly.GetManifestResourceStream(fullResourceName);
            using var objReader = new StreamReader(objStream ?? throw new FileNotFoundException("Nem található: car.obj"));

            List<VertexTransformation> vertexTransformations = new();
            foreach (var vertex in objVertices)
            {
                vertexTransformations.Add(new VertexTransformation(new Vector3D<float>(vertex[0], vertex[1], vertex[2])));
            }

            int vnExists = 0;

            while (!objReader.EndOfStream)
            {
                var line = objReader.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                int spaceIndex = line.IndexOf(' ');
                string lineClassifier = spaceIndex == -1 ? line : line.Substring(0, spaceIndex);
                string[] lineData = spaceIndex == -1 ? Array.Empty<string>() : line.Substring(spaceIndex + 1).Trim().Split(' ');


                switch (lineClassifier)
                {
                    case "v":
                        objVertices.Add(ParseFloatArray(lineData, 3));
                        break;
                    case "vn":
                        vnExists = 1;
                        objNormals.Add(ParseFloatArray(lineData, 3));
                        break;
                    case "vt":
                        float[] uv = new float[2];
                        for (int i = 0; i < 2; ++i)
                            uv[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                        objUVs.Add(uv);
                        break;

                    case "f":
                        if (lineData[0].Contains('/'))
                        {
                            int[] face = new int[3];
                            for (int i = 0; i < 3; ++i)
                            {
                                var parts = lineData[i].Split('/');
                                int vertexIndex = int.Parse(parts[0]) - 1;
                                int texCoordIndex = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? int.Parse(parts[1]) - 1 : -1;
                                int normalIndex = parts.Length > 2 ? int.Parse(parts[2]) - 1 : -1;

                                face[i] = vertexIndex + 1;

                                vertexFullAssignments.Add((vertexIndex, texCoordIndex, normalIndex));
                            }
                            objFaces.Add(face);
                        }
                        else
                        {
                            int[] face = new int[3];
                            for (int i = 0; i < face.Length; ++i)
                                face[i] = int.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objFaces.Add(face);
                        }
                        break;


                    case "usemtl":
                        currentMaterial = string.Join(" ", lineData);
                        break;

                    case "mtllib":
                        string mtlFileName = string.Join(" ", lineData);
                        LoadMaterialLibrary("Projekt.Resources." + mtlFileName, materials);
                        break;
                }
            }

            // Vertex interleaved
            List<float> vertexBuffer = new();
            for (int i = 0; i < uniqueVertices.Count / 3; i++)
            {
                vertexBuffer.AddRange(uniqueVertices.GetRange(i * 3, 3));
                vertexBuffer.AddRange(uniqueNormals.GetRange(i * 3, 3));
                vertexBuffer.AddRange(uniqueUVs.GetRange(i * 2, 2));
            }

            uint[] indexArray = indices.ToArray();
            float[] vertexArray = vertexBuffer.ToArray();
            float[] colorArray = uniqueColors.ToArray();

            // load texture if exists
            string textureFile = FindMapKdFromMtl(materials, "car");

            //var textureImage = null;
            //if (!string.IsNullOrEmpty(textureFile))
            //{
            var textureImage = ReadTextureImage("car.jpg");
            //}

            if (vnExists == 1)
            {
                foreach (var (vertexIndex, normalIndex) in vertexNormalAssignments)
                {
                    var normal = objNormals[normalIndex];
                    vertexTransformations[vertexIndex].UpdateNormalWithContributionFromAFace(
                        new Vector3D<float>(normal[0], normal[1], normal[2])
                    );
                }
            }
            else
            {
                foreach (var objFace in objFaces)
                {
                    var a = vertexTransformations[objFace[0] - 1];
                    var b = vertexTransformations[objFace[1] - 1];
                    var c = vertexTransformations[objFace[2] - 1];

                    var normal = Vector3D.Normalize(Vector3D.Cross(b.Coordinates - a.Coordinates, c.Coordinates - a.Coordinates));

                    a.UpdateNormalWithContributionFromAFace(normal);
                    b.UpdateNormalWithContributionFromAFace(normal);
                    c.UpdateNormalWithContributionFromAFace(normal);
                }
            }

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            foreach (var vertexTransformation in vertexTransformations)
            {
                glVertices.Add(vertexTransformation.Coordinates.X);
                glVertices.Add(vertexTransformation.Coordinates.Y);
                glVertices.Add(vertexTransformation.Coordinates.Z);

                glVertices.Add(vertexTransformation.Normal.X);
                glVertices.Add(vertexTransformation.Normal.Y);
                glVertices.Add(vertexTransformation.Normal.Z);

                glColors.AddRange([1.0f, 0.0f, 0.0f, 1.0f]);
            }

            List<uint> glIndexArray = new List<uint>();
            foreach (var objFace in objFaces)
            {
                glIndexArray.Add((uint)(objFace[0] - 1));
                glIndexArray.Add((uint)(objFace[1] - 1));
                glIndexArray.Add((uint)(objFace[2] - 1));
            }

            foreach (var vt in vertexTransformations)
            {
                vt.NormalizeNormal();
            }

            return CreateObjectDescriptorFromArrays(Gl, vertexArray, colorArray, indexArray, textureImage);
        }

        private static unsafe GlObject CreateObjectDescriptorFromArrays(GL Gl, float[] vertexArray, float[] colorArray, uint[] indexArray,
            ImageResult? textureImage = null)
        {

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.ToArray(), GLEnum.StaticDraw);

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
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            uint? texture = null;

            if (textureImage != null)
            {
                texture = Gl.GenTexture();
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2D, texture.Value);
                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)textureImage.Width,
                    (uint)textureImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan<byte>)textureImage.Data.AsSpan());
                //Console.WriteLine(textureImage.Width+" "+textureImage.Height);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
                Gl.BindTexture(TextureTarget.Texture2D, 0);
            }

            var obj = new GlObject(vao, vertices, colors, indices, (uint)indexArray.Length, Gl)
            {
                Texture = texture
            };
            return obj;
        }

        private static float[] ParseFloatArray(string[] data, int count)
        {
            float[] result = new float[count];
            for (int i = 0; i < count && i < data.Length; ++i)
                result[i] = float.Parse(data[i], CultureInfo.InvariantCulture);
            return result;
        }

        private static void LoadMaterialLibrary(string mtlResourceName, Dictionary<string, MaterialData> materials)
        {
            using var mtlStream = typeof(AirportDescriptor).Assembly.GetManifestResourceStream(mtlResourceName);
            if (mtlStream == null) return;

            using var reader = new StreamReader(mtlStream);
            string? currentName = null;
            MaterialData? currentMaterial = null;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "newmtl":
                        if (currentName != null && currentMaterial != null)
                            materials[currentName] = currentMaterial;
                        currentName = parts[1];
                        currentMaterial = new MaterialData(currentName);
                        break;

                    case "Ka":
                        currentMaterial!.Ka = new Vector3D<float>(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture));
                        break;

                    case "Kd":
                        currentMaterial!.Kd = new Vector3D<float>(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture));
                        break;

                    case "Ks":
                        currentMaterial!.Ks = new Vector3D<float>(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture));
                        break;

                    case "Ns":
                        currentMaterial!.Ns = float.Parse(parts[1], CultureInfo.InvariantCulture);
                        break;

                    case "map_Kd":
                        currentMaterial!.TextureMap = parts[1];
                        break;
                }
            }

            if (currentName != null && currentMaterial != null)
                materials[currentName] = currentMaterial;
        }


        private static string? FindMapKdFromMtl(Dictionary<string, MaterialData> materials, string name)
        {
            using var mtlStream = typeof(AirportDescriptor).Assembly.GetManifestResourceStream("Projekt.Resources.car.mtl");
            if (mtlStream == null) return null;

            using var reader = new StreamReader(mtlStream);
            string? currentName = null;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "newmtl":
                        currentName = parts[1];
                        break;

                    case "map_Kd":
                        if (currentName == name)
                            return parts[1];
                        break;
                }
            }

            return null;
        }

        private static ImageResult ReadTextureImage(string textureResource)
        {
            using Stream stream = typeof(AirportDescriptor).Assembly.GetManifestResourceStream("Projekt.Resources." + textureResource)
                ?? throw new FileNotFoundException("Nem található: " + textureResource);
            return ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        }

        public class MaterialData
        {
            public string Name;
            public Vector3D<float> Ka;
            public Vector3D<float> Kd;
            public Vector3D<float> Ks;
            public float Ns;
            public string? TextureMap;
            public Vector4D<float> DiffuseColor;
            public MaterialData(string name)
            {
                Name = name;
                Ka = new Vector3D<float>(1, 1, 1);
                Kd = new Vector3D<float>(1, 1, 1);
                Ks = new Vector3D<float>(0, 0, 0);
                Ns = 0;
            }
        }

    }
}