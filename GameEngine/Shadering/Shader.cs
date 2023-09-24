using OpenTK.Graphics.ES20;
using System;
using System.IO;
using System.Numerics;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace GameEngine.Shadering
{
    public class Shader
    {
        public int id { get; private set; }

        public Shader(string vertexShaderLocation, string fragmentShaderLocation)
        {
            id = LoadShaderProgram(vertexShaderLocation, fragmentShaderLocation);
        }

        public void Use()
        {
            GL.UseProgram(id);
        }

        public void UnBind()
        {
            GL.UseProgram(0);
        }

        public int GetAttribLocation(string attribName)
        {
            int location = GL.GetAttribLocation(id, attribName);
            if (location == -1)
                throw new Exception();
            return location;
        }

        public void SetInt(string name, int data)
        {
            Use();
            int location = GL.GetUniformLocation(id, name);
            GL.Uniform1(location, data);
        }

        public void SetFloat(string name, float data)
        {
            Use();
            int location = GL.GetUniformLocation(id, name);
            GL.Uniform1(location, data);
        }

        public void SetMatrix(string name, Matrix4 data)
        {
            Use();
            int location = GL.GetUniformLocation(id, name);
            GL.UniformMatrix4(location, false, ref data);
        }

        public void SetVec3(string name, Vector3 data)
        {
            Use();
            int location = GL.GetUniformLocation(id, name);
            GL.Uniform3(location, data);
        }
        public void SetVec3(string name, float dataX, float dataY, float dataZ)
        {
            Use();
            int location = GL.GetUniformLocation(id, name);
            GL.Uniform3(location, new Vector3(dataX, dataY, dataZ));
        }

        private int LoadShader(string location, ShaderType type)
        {
            int shaderId = GL.CreateShader(type);
            GL.ShaderSource(shaderId, File.ReadAllText(location));
            GL.CompileShader(shaderId);

            string infoLog = GL.GetShaderInfoLog(shaderId);
            if (!string.IsNullOrEmpty(infoLog))
            {
                throw new Exception(infoLog);
            }

            return shaderId;
        }

        private int LoadShaderProgram(string vertexShaderLocation, string fragmentShaderLocation)
        {
            int shaderPorgramId = GL.CreateProgram();

            int vertexShader = LoadShader(vertexShaderLocation, ShaderType.VertexShader);
            int fragmentShader = LoadShader(fragmentShaderLocation, ShaderType.FragmentShader);

            GL.AttachShader(shaderPorgramId, vertexShader);
            GL.AttachShader(shaderPorgramId, fragmentShader);
            GL.LinkProgram(shaderPorgramId);

            GL.DetachShader(shaderPorgramId, vertexShader);
            GL.DetachShader(shaderPorgramId, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            string infoLog = GL.GetProgramInfoLog(shaderPorgramId);
            if (!string.IsNullOrEmpty(infoLog))
            {
                throw new Exception(infoLog);
            }

            return shaderPorgramId;
        }
    }
}
