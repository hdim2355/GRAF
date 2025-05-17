using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Szeminarium1_24_03_05_2;
using static System.Formats.Asn1.AsnWriter;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace Projekt
{
    internal class Program
    {
        enum WeaponType
        {
            Bomb,
            Ammo
        }

        private static WeaponType currentWeapon = WeaponType.Bomb;

        private static List<AmmoInstance> activeAmmos = new();

        private static int maxAmmoCount = 30;
        private static int currentAmmoCount = 30;

        private static ImGuiController imguiController;

        private static IWindow graphicWindow;

        private static GL Gl;

        private static ShaderDescriptor shaders;

        private static CameraDescriptor camera;

        private static uint program;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";

        private const string TextureVariableName = "uTexture";

        private static GlObject plane;

        private static GlObject tank;

        private static GlObject bomb;

        private static GlObject propeller;

        private static GlObject skybox;

        private static GlObject ground;

        private static GlObject ammo;

        private static GlObject explosionModel;

        private static SceneObject groundObject;

        private static SceneObject planeObject;

        private static SceneObject propellerObject;

        private static HashSet<Key> pressedKeys = new();

        private static List<ExplosionInstance> activeExplosions3D = new();

        private static float planeSpeed = 14.0f;

        private static int maxBombCount = 5;

        private static int currentBombCount = 5;

        private static bool moveForward = false;

        private static List<BombInstance> activeBombs;

        private static List<SceneObject> tanks = new();
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

            if (key == Key.Q)
            {
                if (currentWeapon == WeaponType.Bomb)
                    currentWeapon = WeaponType.Ammo;
                else
                    currentWeapon = WeaponType.Bomb;
            }

            if (key == Key.Space)
            {
                if (currentWeapon == WeaponType.Bomb && currentBombCount > 0)
                {
                    var forward = planeObject.GetForwardDirection();
                    var initialVelocity = -forward * 10.0f;
                    var newBomb = new BombInstance(planeObject.Position, initialVelocity);
                    activeBombs.Add(newBomb);
                    currentBombCount--;
                }
                else if (currentWeapon == WeaponType.Ammo && currentAmmoCount > 0)
                {
                    var forward = planeObject.GetForwardDirection();
                    var leftWing = planeObject.Position + Vector3D.Cross(planeObject.GetUpDirection(), forward) * 1.5f;
                    var rightWing = planeObject.Position - Vector3D.Cross(planeObject.GetUpDirection(), forward) * 1.5f;

                    activeAmmos.Add(new AmmoInstance(leftWing, -forward));
                    activeAmmos.Add(new AmmoInstance(rightWing, -forward));

                    currentAmmoCount -= 2;
                }
            }

            if (key == Key.Enter)
            {
                moveForward = !moveForward;
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

            tank = TankDescriptor.CreatePlane(Gl);

            bomb = BombDescriptor.CreateBomb(Gl);

            ammo = AmmoDescriptor.CreatePlane(Gl);

            tanks.Add(new SceneObject
            {
                Position = new Vector3D<float>(100, -10, 100),
                Rotation = new Vector3D<float>(0f, 180f, 0f),
                Scale = new Vector3D<float>(3.4f, 3.4f, 3.4f)
            });

            tanks.Add(new SceneObject
            {
                Position = new Vector3D<float>(-100, -10, 80),
                Rotation = new Vector3D<float>(0f, 0f, 0f),
                Scale = new Vector3D<float>(3.4f, 3.4f, 3.4f)
            });

            tanks.Add(new SceneObject
            {
                Position = new Vector3D<float>(0, -10, -120),
                Rotation = new Vector3D<float>(0f, 90f, 0f),
                Scale = new Vector3D<float>(3.4f, 3.4f, 3.4f)
            });

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
                Position = new Vector3D<float>(0f, -10.01f, 0f),
                Rotation = new Vector3D<float>(0f, 0f, 0f),
                Scale = new Vector3D<float>(1f, 1f, 1f)
            };

            skybox = SkyboxDescriptor.CreateSkyBox(Gl);

            camera = new CameraDescriptor();

            shaders = new ShaderDescriptor();

            explosionModel = ExplosionDescriptor.CreateExplosion(Gl);

            program = shaders.LinkProgram(Gl);

            imguiController = new ImGuiController(Gl, graphicWindow, inputContext);

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

            foreach (var ammos in activeAmmos)
            {
                DrawObject(ammo, ammos.Scene);
            }


            DrawObject(plane,planeObject);

            foreach (var t in tanks)
            {
                DrawObject(tank, t);
            }

            foreach (var explosion in activeExplosions3D)
            {
                foreach (var particle in explosion.GetParticles())
                {
                    DrawObject(explosionModel, particle);
                }
            }


            DrawObject(propeller,propellerObject);

            imguiController.Update((float)deltaTime);

            ImGuiNET.ImGui.Begin("HUD", ImGuiNET.ImGuiWindowFlags.NoTitleBar | ImGuiNET.ImGuiWindowFlags.NoResize | ImGuiNET.ImGuiWindowFlags.NoMove | ImGuiNET.ImGuiWindowFlags.NoBackground | ImGuiNET.ImGuiWindowFlags.NoScrollbar);
            ImGuiNET.ImGui.SetWindowPos(new System.Numerics.Vector2(10, 10));
            ImGuiNET.ImGui.SetWindowSize(new System.Numerics.Vector2(200, 50));
            ImGuiNET.ImGui.Text($"Fegyver: {currentWeapon}");
            if (currentWeapon == WeaponType.Bomb)
                ImGuiNET.ImGui.Text($"Bombák száma: {currentBombCount}");
            else
                ImGuiNET.ImGui.Text($"Ammo száma: {currentAmmoCount}");
            ImGuiNET.ImGui.End();

            imguiController.Render();

            return; 
        }

        private static void TriggerExplosion(Vector3D<float> position)
        {
            activeExplosions3D.Add(new ExplosionInstance(position));
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
            float rotationSpeed = 60f;
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

            foreach (var ammo in activeAmmos)
            {
                ammo.Update((float)deltaTime);
            }

            //activeAmmos.RemoveAll(a => a.Scene.Position.Length() > 2000);

            for (int i = activeBombs.Count - 1; i >= 0; i--)
            {
                var bomb = activeBombs[i];

                if (bomb.HasExploded)
                {
                    TriggerExplosion(bomb.Scene.Position);

                    // Töröljük a tankot, amelyik közel van
                    for (int j = tanks.Count - 1; j >= 0; j--)
                    {
                        if (Vector3D.Distance(bomb.Scene.Position, tanks[j].Position) < 15f)
                        {
                            tanks.RemoveAt(j);
                        }
                    }

                    activeBombs.RemoveAt(i);
                }
            }

            for (int i = activeBombs.Count - 1; i >= 0; i--)
            {
                var bomb = activeBombs[i];

                if (bomb.HasExploded)
                {
                    TriggerExplosion(bomb.Scene.Position);
                    activeBombs.RemoveAt(i);
                }
            }

            camera.FollowObject(planeObject);

            foreach (var explosion in activeExplosions3D.ToList())
            {
                explosion.Update((float)deltaTime);
                if (!explosion.IsAlive)
                    activeExplosions3D.Remove(explosion);
            }
        
            float tankMoveSpeed = 10f * (float)deltaTime;

            for (int i = 0; i < tanks.Count; i++)
            {
                var tank = tanks[i];

                float offset = i * 3.14f / 1.5f; 
                tank.Position.X += MathF.Sin((float)graphicWindow.Time + offset) * tankMoveSpeed * 0.5f;
                tank.Position.Z += MathF.Cos((float)graphicWindow.Time + offset) * tankMoveSpeed * 0.5f;
            }

        }

        private static unsafe void DrawGround()
        {
            Matrix4X4<float> modelMatrix = groundObject.GetModelMatrix();

            SetModelMatrix(modelMatrix);

            // set the texture
            int textureLocation = Gl.GetUniformLocation(program, TextureVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);
            Gl.ActiveTexture(TextureUnit.Texture0);
            //Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            //Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
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
            plane.Release();
            skybox.Release();
            propeller.Release();
            bomb.Release();
            ground.Release();
            explosionModel.Release();
            tank.Release();
            ammo.Release();
            imguiController.Dispose();
            Gl.DeleteProgram(program);
        }

    }
}
