using System.Numerics;
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

        private static ShaderDescriptor shaders;

        private static CameraDescriptor camera;

        private static uint program;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";

        private const string TextureVariableName = "uTexture";

        private static GlObject plane;

        private static GlObject bomb;

        private static GlObject propeller;

        private static GlObject skybox;

        private static GlObject ground;

        private static SceneObject groundObject;

        private static SceneObject planeObject;

        private static SceneObject propellerObject;

        private static HashSet<Key> pressedKeys = new();

        private static float planeSpeed = 14.0f;

        private static bool moveForward = false;

        private static List<BombInstance> activeBombs;
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
            pressedKeys.Add(key);

            if (key == Key.Space)
            {
                moveForward = !moveForward;
            }

            if (key == Key.Enter)
            {
                var forward = planeObject.GetForwardDirection();
                var initialVelocity = -forward * 10.0f;
                var newBomb = new BombInstance(planeObject.Position, initialVelocity);
                activeBombs.Add(newBomb);
            }
        }
        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            pressedKeys.Remove(key);
        }
        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                keyboard.KeyUp += Keyboard_KeyUp;
            }

            Gl.ClearColor(System.Drawing.Color.White);

            //Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            planeObject = new SceneObject
            {
                Position = new Vector3D<float>(0, 100, 0),
                Rotation = new Vector3D<float>(0f, 180f, 0f),
                Scale = new Vector3D<float>(0.4f, 0.4f, 0.4f)
            };

            plane = PlaneDescriptor.CreatePlane(Gl);

            bomb = BombDescriptor.CreateBomb(Gl);

            activeBombs = new();

            propeller = PropellerDescriptor.CreatePropeller(Gl);

            propellerObject = new SceneObject
            {
                Position = new Vector3D<float>(-0.3f, 0.3f, -0.05f),
                Rotation = new Vector3D<float>(0f, 180f, 0f),
                Scale = new Vector3D<float>(0.2f, 0.2f, 0.2f)
            };

            ground = GroundDescriptor.CreateGround(Gl);

            groundObject = new SceneObject
            {
                Position = new Vector3D<float>(0f, -0.1f, 0f),
                Rotation = new Vector3D<float>(0f, 0f, 0f),
                Scale = new Vector3D<float>(100f, 1f, 100f)
            };

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

            DrawSkyBox();

            DrawGround();

            foreach (var bombInstance in activeBombs)
            {
                DrawObject(bomb, bombInstance.Scene);
            }

            DrawObject(plane,planeObject);

            DrawObject(propeller,propellerObject);

            return; 
        }

        private static unsafe void DrawObject(GlObject objects,SceneObject scene)
        {
            Gl.BindVertexArray(objects.Vao);

            if (objects.Texture.HasValue)
            {
                int textureLocation = Gl.GetUniformLocation(program, "uTexture");
                Gl.Uniform1(textureLocation, 0);
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2D, objects.Texture.Value);
            }

            Matrix4X4<float> modelMatrix = scene.GetModelMatrix();

            SetModelMatrix(modelMatrix);

            Gl.DrawElements(PrimitiveType.Triangles, objects.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindVertexArray(0);

            if (objects.Texture.HasValue)
                Gl.BindTexture(TextureTarget.Texture2D, 0);
        }
        private static void GraphicWindow_Update(double deltaTime)
        {
            float rotationSpeed = 60f; // fok per másodperc
            float moveSpeed = planeSpeed * (float)deltaTime;

            if (pressedKeys.Contains(Key.Left))
                planeObject.Rotation.Y += rotationSpeed * (float)deltaTime;

            if (pressedKeys.Contains(Key.Right))
                planeObject.Rotation.Y -= rotationSpeed * (float)deltaTime;

            if (pressedKeys.Contains(Key.Up))
                planeObject.Rotation.X -= rotationSpeed * (float)deltaTime;

            if (pressedKeys.Contains(Key.Down))
                planeObject.Rotation.X += rotationSpeed * (float)deltaTime;

            if (moveForward)
            {
                var forward = planeObject.GetForwardDirection();
                var deltaMove = forward * (float)deltaTime * planeSpeed;
                planeObject.Position -= deltaMove;
            }

            foreach (var bomb in activeBombs)
            {
                bomb.Update((float)deltaTime);
            }

            camera.FollowObject(planeObject);
        }

        private static unsafe void DrawGround()
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
            Gl.BindTexture(TextureTarget.Texture2D, ground.Texture.Value);

            DrawModelObject(ground);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
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
        
        private static void GraphicWindow_Closing()
        {

            //cube.Dispose();
            //teapot.Dispose();
            plane.Release();
            skybox.Release();
            propeller.Release();
            bomb.Release();
            ground.Release();
            Gl.DeleteProgram(program);
        }

    }
}
