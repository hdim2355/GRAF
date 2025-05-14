using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Szeminarium1_24_03_05_2;

namespace Projekt
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static GlObject plane;

        private static ShaderDescriptor shaders;

        private static CameraDescriptor camera;

        private static uint program;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Project";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();

        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            //Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            plane = PlaneDescriptor.CreatePlane(Gl);
            
            camera = new CameraDescriptor();

            shaders = new ShaderDescriptor();
            
            program = shaders.LinkProgram(Gl);
        }
        private static unsafe void GraphicWindow_Render(double deltaTime)
        {

            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            shaders.setViewMatrix(Gl,camera,program);
            shaders.SetProjectionMatrix(Gl, program);
            shaders.SetLight(Gl, camera, program);
            shaders.SetShininess(Gl, program);

            DrawCenteredPulsingTeapot();

            return; 
        }

        private static unsafe void DrawCenteredPulsingTeapot()
        {
            Gl.BindVertexArray(plane.Vao);
            Matrix4X4<float> modelMatrixForCenterCube = Matrix4X4.CreateScale((float)0.4);
            SetModelMatrix(modelMatrixForCenterCube);
            Gl.DrawElements(PrimitiveType.Triangles, plane.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            // G = (M^-1)^T
            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO OpenGL
            // make it threadsafe
            //cubeArrangementModel.AdvanceTime(deltaTime);

            //imGuiController.Update((float)deltaTime);
        }
        private static void GraphicWindow_Closing()
        {

            //cube.Dispose();
            //teapot.Dispose();
            Gl.DeleteProgram(program);
        }

    }
}
