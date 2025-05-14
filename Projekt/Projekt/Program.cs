using Silk.NET.Input;
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

        private static GlObject skybox;

        private static ShaderDescriptor shaders;

        private static CameraDescriptor camera;

        private static uint program;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";

        private const string TextureVariableName = "uTexture";

        private static SceneObject planeObject = new SceneObject
        {
            Scale = new Vector3D<float>(0.4f, 0.4f, 0.4f)
        };

        private static float planeSpeed = 14.0f;

        private static bool moveForward = false;
        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Project";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);
            graphicWindow.FramebufferResize += newSize => { Gl.Viewport(newSize); };

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();

        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    planeObject.Rotation.Y += 5; 
                    break;
                case Key.Right:
                    planeObject.Rotation.Y -= 5;
                    break;
                case Key.Up:
                    planeObject.Rotation.X -= 5;
                    break;
                case Key.Down:
                    planeObject.Rotation.X += 5;
                    break;
                case Key.Space:
                    moveForward = !moveForward;
                    break;
                case Key.S:
                    //camera.IncreaseZXAngle();
                    break;
                case Key.D:
                    //camera.DecreaseZXAngle();
                    Console.Write(camera.Position);
                    break;
            }
        }
        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl.ClearColor(System.Drawing.Color.White);

            //Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            planeObject = new SceneObject
            {
                Position = new Vector3D<float>(0, 0, 0),
                Rotation = new Vector3D<float>(0f, 180f, 0f),
                Scale = new Vector3D<float>(0.4f, 0.4f, 0.4f)
            };

            plane = PlaneDescriptor.CreatePlane(Gl);

            skybox = SkyboxDescriptor.CreateSkyBox(Gl);

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

            Gl.DepthMask(false); // disable writing to depth buffer
            DrawSkyBox();
            Gl.DepthMask(true); // re-enable depth writing

            DrawCenteredPulsingPlane();

            return; 
        }

        private static unsafe void DrawSkyBox()
        {
            var modelMatrixSkyBox = Matrix4X4.CreateScale(1000f);
            SetModelMatrix(modelMatrixSkyBox);

            // set the texture
            int textureLocation = Gl.GetUniformLocation(program, TextureVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skybox.Texture.Value);

            DrawModelObject(skybox);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawModelObject(GlObject modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }
        private static unsafe void DrawCenteredPulsingPlane()
        {
            Gl.BindVertexArray(plane.Vao);

            //Matrix4X4<float> scale = Matrix4X4.CreateScale(0.4f);
            Matrix4X4<float> modelMatrix = planeObject.GetModelMatrix();

            SetModelMatrix(modelMatrix);

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
            if (moveForward)
            {
                var forward = planeObject.GetForwardDirection() ;
                var deltaMove = forward * (float)deltaTime * planeSpeed;
                planeObject.Position -= deltaMove;
            }

            camera.FollowObject(planeObject);
        }
        private static void GraphicWindow_Closing()
        {

            //cube.Dispose();
            //teapot.Dispose();
            plane.Release();
            Gl.DeleteProgram(program);
        }

    }
}
