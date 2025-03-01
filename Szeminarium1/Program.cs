using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
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

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. szeminárium - háromszög";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            //float[] vertexArray = new float[] {
            //    -0.5f, -0.5f, 0.0f,
            //    +0.5f, -0.5f, 0.0f,
            //     0.0f, +0.5f, 0.0f,
            //     1f, 1f, 0f
            //};

            //float[] colorArray = new float[] {
            //    1.0f, 0.0f, 0.0f, 1.0f,
            //    0.0f, 1.0f, 0.0f, 1.0f,
            //    0.0f, 0.0f, 1.0f, 1.0f,
            //    1.0f, 0.0f, 0.0f, 1.0f,
            //};

            //uint[] indexArray = new uint[] { 
            //    0, 1, 2,
            //    2, 1, 3
            //};

            /*
            float[] vertexArray = new float[] {
                0.0f, +0.5f, 0.0f,//0
                0.0f, -0.5f, 0.0f,//1
                -0.5f, -0.2f, 0.0f,//2
                -0.6f, +0.8f, 0f,//3
            
                 +0.5f,-0.2f, 0.0f,//4
                 +0.6f, +0.8f, 0f,//5

                 0.0f,1.0f,0.0f,//6

                 0.0f, +0.5f, 0.0f,//7,0
                0.0f, -0.5f, 0.0f,//8,1

                0.0f, +0.5f, 0.0f,//9,0
                -0.6f, +0.8f, 0f,//10,3
                +0.6f, +0.8f, 0f,//11,5
            };

            float[] colorArray = new float[] {
                1.0f, 0.0f, 0.0f, 1.0f,//0
                1.0f, 0.0f, 0.0f, 1.0f,//1
                1.0f, 0.0f, 0.0f, 1.0f,//2
                1.0f, 0.0f, 0.0f, 1.0f,//3

                0.0f, 0.0f, 1.0f, 1.0f,//4
                0.0f, 0.0f, 1.0f, 1.0f,//5
                0.0f, 1.0f, 0.0f, 1.0f,//6
                0.0f, 0.0f, 1.0f, 1.0f,//7
                0.0f, 0.0f, 1.0f, 1.0f,//8

                0.0f, 1.0f, 0.0f, 1.0f,//9
                0.0f, 1.0f, 0.0f, 1.0f,//10
                0.0f, 1.0f, 0.0f, 1.0f,//11
            };

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,

                7,8,4,
                7,4,5,

                9,6,11,
                9,6,10,
            };*/

            float[] vertexArray = new float[] {
                 0.0f, +0.1f, 0.0f,//0
                0.0f, -0.1f, 0.0f,//1
                -0.11f, -0.04f, 0.0f,//2
                -0.12f, +0.16f, 0f,//3

                -0.14f, +0.16f, 0.0f,//4
                -0.13f, -0.04f, 0.0f,//5
                -0.24f, +0.02f, 0.0f,//6
                -0.26f, +0.22f, 0f,//7

                -0.28f, +0.22f, 0.0f,//8
                -0.26f, +0.02f, 0.0f,//9
                -0.38f, +0.08f, 0.0f,//10
                -0.40f, +0.28f, 0f,//11

                 0.0f, +0.32f, 0.0f,//12
                0.0f, +0.12f, 0.0f,//13
                -0.12f, +0.18f, 0.0f,//14
                -0.14f, +0.38f, 0f,//15

                -0.16f, +0.38f, 0.0f,//16
                -0.15f, +0.18f, 0.0f,//17
                -0.26f, +0.23f, 0.0f,//18
                -0.28f, +0.44f, 0f,//19

                -0.30f, +0.44f, 0.0f,//20
                -0.28f, +0.24f, 0.0f,//21
                -0.40f, +0.29f, 0.0f,//22
                -0.42f, +0.50f, 0f,//23

                0.0f, +0.54f, 0.0f,//24
                0.0f, +0.34f, 0.0f,//25
                -0.14f, +0.40f, 0.0f,//26
                -0.16f, +0.60f, 0f,//27

                -0.18f, +0.60f, 0.0f,//28
                -0.16f, +0.40f, 0.0f,//29
                -0.28f, +0.46f, 0.0f,//30
                -0.30f, +0.66f, 0f,//31

                -0.32f, +0.66f, 0.0f,//32
                -0.30f, +0.46f, 0.0f,//33
                -0.42f, +0.52f, 0.0f,//34
                -0.44f, +0.72f, 0f,//35

                
                0.02f, +0.1f, 0.0f,//36
                0.02f, -0.1f, 0.0f,//37
                +0.13f, -0.04f, 0.0f,//38
                +0.14f, +0.16f, 0f,//39

                +0.16f, +0.16f, 0.0f,//40
                +0.15f, -0.04f, 0.0f,//41
                +0.26f, +0.02f, 0.0f,//42
                +0.28f, +0.22f, 0f,//43

                +0.30f, +0.22f, 0.0f,//44
                +0.28f, +0.02f, 0.0f,//45
                +0.40f, +0.08f, 0.0f,//46
                +0.42f, +0.28f, 0f,//47

                0.02f, +0.32f, 0.0f,//48
                0.02f, +0.12f, 0.0f,//49
                +0.14f, +0.18f, 0.0f,//50
                +0.16f, +0.38f, 0f,//51

                +0.18f, +0.38f, 0.0f,//52
                +0.17f, +0.18f, 0.0f,//53
                +0.28f, +0.23f, 0.0f,//54
                +0.30f, +0.44f, 0f,//55

                +0.32f, +0.44f, 0.0f,//56
                +0.30f, +0.24f, 0.0f,//57
                +0.42f, +0.29f, 0.0f,//58
                +0.44f, +0.50f, 0f,//59

                +0.02f, +0.54f, 0.0f,//60
                +0.02f, +0.34f, 0.0f,//61
                +0.16f, +0.40f, 0.0f,//62
                +0.18f, +0.60f, 0f,//63

                +0.20f, +0.60f, 0.0f,//64
                +0.18f, +0.40f, 0.0f,//65
                +0.30f, +0.46f, 0.0f,//66
                +0.32f, +0.66f, 0f,//67

                +0.34f, +0.66f, 0.0f,//68
                +0.32f, +0.46f, 0.0f,//69
                +0.44f, +0.52f, 0.0f,//70
                +0.46f, +0.72f, 0f,//71

                +0.01f, +0.66f, 0.0f,//72
                +0.01f, +0.56f, 0.0f,//73
                -0.15f, +0.61f, 0.0f,//74
                +0.16f, +0.61f, 0f,//75

                +0.19f, +0.62f, 0.0f,//76
                +0.19f, +0.72f, 0.0f,//77
                +0.31f, +0.67f, 0.0f,//78
                +0.03f, +0.67f, 0f,//79

                +0.33f, +0.68f, 0.0f,//80
                +0.33f, +0.77f, 0.0f,//81
                +0.45f, +0.73f, 0.0f,//82
                +0.21f, +0.73f, 0f,//83

                -0.14f, +0.72f, 0.0f,//84
                -0.17f, +0.62f, 0.0f,//85
                -0.01f, +0.67f, 0.0f,//86
                -0.28f, +0.67f, 0f,//87

                
                -0.30f, +0.77f, 0.0f,//88
                -0.31f, +0.68f, 0.0f,//89
                -0.43f, +0.73f, 0.0f,//90
                -0.17f, +0.73f, 0f,//91



            };

            float[] colorArray = new float[] {
                1.0f, 0.0f, 0.0f, 1.0f,//0  
                1.0f, 0.0f, 0.0f, 1.0f,//1
                1.0f, 0.0f, 0.0f, 1.0f,//2
                1.0f, 0.0f, 0.0f, 1.0f,//3

                1.0f, 0.0f, 0.0f, 1.0f,//4  
                1.0f, 0.0f, 0.0f, 1.0f,//5
                1.0f, 0.0f, 0.0f, 1.0f,//6
                1.0f, 0.0f, 0.0f, 1.0f,//7

                1.0f, 0.0f, 0.0f, 1.0f,//8
                1.0f, 0.0f, 0.0f, 1.0f,//9
                1.0f, 0.0f, 0.0f, 1.0f,//10
                1.0f, 0.0f, 0.0f, 1.0f,//11

                1.0f, 0.0f, 0.0f, 1.0f,//12
                1.0f, 0.0f, 0.0f, 1.0f,//13
                1.0f, 0.0f, 0.0f, 1.0f,//14
                1.0f, 0.0f, 0.0f, 1.0f,//15

                1.0f, 0.0f, 0.0f, 1.0f,//16
                1.0f, 0.0f, 0.0f, 1.0f,//17
                1.0f, 0.0f, 0.0f, 1.0f,//18
                1.0f, 0.0f, 0.0f, 1.0f,//19

                1.0f, 0.0f, 0.0f, 1.0f,//20
                1.0f, 0.0f, 0.0f, 1.0f,//21
                1.0f, 0.0f, 0.0f, 1.0f,//22
                1.0f, 0.0f, 0.0f, 1.0f,//23

                1.0f, 0.0f, 0.0f, 1.0f,//24
                1.0f, 0.0f, 0.0f, 1.0f,//25
                1.0f, 0.0f, 0.0f, 1.0f,//26
                1.0f, 0.0f, 0.0f, 1.0f,//27

                1.0f, 0.0f, 0.0f, 1.0f,//28
                1.0f, 0.0f, 0.0f, 1.0f,//29
                1.0f, 0.0f, 0.0f, 1.0f,//30
                1.0f, 0.0f, 0.0f, 1.0f,//31

                1.0f, 0.0f, 0.0f, 1.0f,//32
                1.0f, 0.0f, 0.0f, 1.0f,//33
                1.0f, 0.0f, 0.0f, 1.0f,//34
                1.0f, 0.0f, 0.0f, 1.0f,//35

                0.0f, 1.0f, 0.0f, 1.0f,//36
                0.0f, 1.0f, 0.0f, 1.0f,//37
                0.0f, 1.0f, 0.0f, 1.0f,//38
                0.0f, 1.0f, 0.0f, 1.0f,//39

                0.0f, 1.0f, 0.0f, 1.0f,//40
                0.0f, 1.0f, 0.0f, 1.0f,//41
                0.0f, 1.0f, 0.0f, 1.0f,//42
                0.0f, 1.0f, 0.0f, 1.0f,//43

                0.0f, 1.0f, 0.0f, 1.0f,//44
                0.0f, 1.0f, 0.0f, 1.0f,//45
                0.0f, 1.0f, 0.0f, 1.0f,//46
                0.0f, 1.0f, 0.0f, 1.0f,//47

                0.0f, 1.0f, 0.0f, 1.0f,//48
                0.0f, 1.0f, 0.0f, 1.0f,//49
                0.0f, 1.0f, 0.0f, 1.0f,//50
                0.0f, 1.0f, 0.0f, 1.0f,//51

                0.0f, 1.0f, 0.0f, 1.0f,//52
                0.0f, 1.0f, 0.0f, 1.0f,//53
                0.0f, 1.0f, 0.0f, 1.0f,//54
                0.0f, 1.0f, 0.0f, 1.0f,//55

                0.0f, 1.0f, 0.0f, 1.0f,//56
                0.0f, 1.0f, 0.0f, 1.0f,//57
                0.0f, 1.0f, 0.0f, 1.0f,//58
                0.0f, 1.0f, 0.0f, 1.0f,//59

                0.0f, 1.0f, 0.0f, 1.0f,//60
                0.0f, 1.0f, 0.0f, 1.0f,//61
                0.0f, 1.0f, 0.0f, 1.0f,//62
                0.0f, 1.0f, 0.0f, 1.0f,//63

                0.0f, 1.0f, 0.0f, 1.0f,//64
                0.0f, 1.0f, 0.0f, 1.0f,//65
                0.0f, 1.0f, 0.0f, 1.0f,//66
                0.0f, 1.0f, 0.0f, 1.0f,//67

                0.0f, 1.0f, 0.0f, 1.0f,//68
                0.0f, 1.0f, 0.0f, 1.0f,//69
                0.0f, 1.0f, 0.0f, 1.0f,//70
                0.0f, 1.0f, 0.0f, 1.0f,//71

            };

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,

                4, 5, 6,
                4, 6, 7,

                8, 9, 10,
                8, 10, 11,

                12,13,14,
                12,14,15,

                16,17,18,
                16,18,19,

                20,21,22,
                20,22,23,

                24,25,26,
                24,26,27,

                28,29,30,
                28,30,31,

                32,33,34,
                32,34,35,

                36,37,38,
                36,38,39,

                40,41,42,
                40,42,43,

                44,45,46,
                44,46,47,

                48,49,50,
                48,50,51,

                52,53,54,
                52,54,55,

                56,57,58,
                56,58,59,

                60,61,62,
                60,62,63,

                64,65,66,
                64,66,67,

                68,69,70,
                68,70,71,

                72,73,74,
                72,73,75,

                76,77,78,
                76,77,79,

                80,81,82,
                80,81,83,

                84,85,86,
                84,85,87,

                88,89,90,
                88,89,91,

            };


            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);
            
            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}
