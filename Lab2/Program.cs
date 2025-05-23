﻿using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ModelObjectDescriptor[] cube;

        private static CameraDescriptor camera = new CameraDescriptor();

        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();

        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        private static uint program;

        static void Main(string[] args)
        {
            cube = new ModelObjectDescriptor[27];
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Grafika szeminárium";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            for (int i = 0; i < 27; i++)
            {
                cube[i].Dispose();
            }
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }


            int db = 0;
            for (int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    for(int z = -1; z <= 1; z++)
                    {
                        cube[db++] = ModelObjectDescriptor.CreateCube(Gl,i,j,z);
                    }
                }
            }

            Gl.ClearColor(System.Drawing.Color.White);
            
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);


            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {

            }

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    camera.DecreaseZYAngle();
                    break;
                case Key.Right:
                    camera.IncreaseZYAngle();
                    break;
                case Key.Down:
                    camera.IncreaseDistance();
                    break;
                case Key.Up:
                    camera.DecreaseDistance();
                    break;
                case Key.S:
                    camera.IncreaseZXAngle();
                    break;
                case Key.D:
                    camera.DecreaseZXAngle();
                    break;
                case Key.Space:
                    cubeArrangementModel.AnimationEnabled = !cubeArrangementModel.AnimationEnabled;
                    cubeArrangementModel.signum = 1;
                    break;
                case Key.Backspace:
                    cubeArrangementModel.AnimationEnabled = !cubeArrangementModel.AnimationEnabled;
                    cubeArrangementModel.signum = -1;
                    break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            cubeArrangementModel.AdvanceTime(deltaTime);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);


            //var modelMatrixCenterCube = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale);
            //SetMatrix(modelMatrixCenterCube, ModelMatrixVariableName);
            //DrawModelObject(cube);

            int db = 0;
            // **Forgási középpont kiszámítása** (jobb oldali oszlop középpontja)
            float pivotX = 1.0f; // Mivel i == 1, a jobb oldal x = 1 koordinátán van
            float pivotY = 0.0f; // Középen van Y tengelyen
            float pivotZ = 0.0f; // Középen van Z tengelyen

            // **Forgatási mátrix előkészítése**
            Matrix4X4<float> rotationX = Matrix4X4.CreateRotationX((float)(Math.PI * cubeArrangementModel.RightSideCubeRotation / 180.0));
            Matrix4X4<float> rotationY = Matrix4X4.CreateRotationY((float)(Math.PI * cubeArrangementModel.RightSideCubeRotation / 180.0));
            Matrix4X4<float> rotationZ = Matrix4X4.CreateRotationY((float)(Math.PI * cubeArrangementModel.RightSideCubeRotation / 180.0));
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Matrix4X4<float> modelMatrix;

                        // **1. Megnézzük, hogy melyik síkot akarjuk forgatni**
                        bool rotateRightSide = (i == 1); // Jobb oldali oszlop
                        bool rotateLeftSide = (i == -1); // Bal oldali oszlop
                        bool rotateTopSide = (j == 1); // Felső réteg
                        bool rotateBottomSide = (j == -1); // Alsó réteg
                        bool rotateFrontSide = (z == 1); // Elülső oldal
                        bool rotateBackSide = (z == -1); // Hátsó oldal

                        // **2. Kiválasztjuk a megfelelő forgatást**
                        if (rotateRightSide)
                        {
                            modelMatrix = RotateCubeFace(1, 0, 0, cubeArrangementModel.RightSideCubeRotation, i, j, z);
                        }
                        else if (rotateLeftSide)
                        {
                            modelMatrix = RotateCubeFace(-1, 0, 0, cubeArrangementModel.LeftSideCubeRotation, i, j, z);
                        }
                        else if (rotateTopSide)
                        {
                            modelMatrix = RotateCubeFace(0, 1, 0, cubeArrangementModel.TopSideCubeRotation, i, j, z);
                        }
                        else if (rotateBottomSide)
                        {
                            modelMatrix = RotateCubeFace(0, -1, 0, cubeArrangementModel.BottomSideCubeRotation, i, j, z);
                        }
                        else if (rotateFrontSide)
                        {
                            modelMatrix = RotateCubeFace(0, 0, 1, cubeArrangementModel.FrontSideCubeRotation, i, j, z);
                        }
                        else if (rotateBackSide)
                        {
                            modelMatrix = RotateCubeFace(0, 0, -1, cubeArrangementModel.BackSideCubeRotation, i, j, z);
                        }
                        else
                        {
                            // Ha nincs forgatás, csak a méretezést alkalmazzuk
                            modelMatrix = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale);
                        }

                        // **3. Hozzáadjuk az alap eltolást**
                        Matrix4X4<float> translation = Matrix4X4.CreateTranslation((float)i, j, z);
                        Matrix4X4<float> finalModelMatrix = translation * modelMatrix;

                        SetMatrix(finalModelMatrix, ModelMatrixVariableName);
                        DrawModelObject(cube[db++]);
                    }
                }
            }

        }

        private static Matrix4X4<float> RotateCubeFace(int axisX, int axisY, int axisZ, double rotationAngle, int i, int j, int z)
        {
            // **Eltolás a forgás középpontjába**
            Matrix4X4<float> translateToOrigin = Matrix4X4.CreateTranslation((float)-axisX, -axisY, -axisZ);

            // **Forgatás az adott tengely körül**
            Matrix4X4<float> rotation;
            if (axisX != 0)
                rotation = Matrix4X4.CreateRotationX((float)(Math.PI * rotationAngle / 180.0));
            else if (axisY != 0)
                rotation = Matrix4X4.CreateRotationY((float)(Math.PI * rotationAngle / 180.0));
            else
                rotation = Matrix4X4.CreateRotationZ((float)(Math.PI * rotationAngle / 180.0));

            // **Visszatolás az eredeti helyre**
            Matrix4X4<float> translateBack = Matrix4X4.CreateTranslation((float)axisX, axisY, axisZ);

            // **Végső transzformáció: eltolás → forgatás → vissza**
            return translateBack * rotation * translateToOrigin;
        }
        private static unsafe void DrawModelObject(ModelObjectDescriptor modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}