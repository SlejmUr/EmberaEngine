using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public class Material
    {
        public Shader shader;

        Dictionary<string, int> propertyInts;
        Dictionary<string, float> propertyFloats;
        Dictionary<string, bool> propertyBools;
        Dictionary<string, Vector2> propertyVec2;
        Dictionary<string, Vector3> propertyVec3;
        Dictionary<string, Vector4> propertyVec4;
        Dictionary<string, Matrix3> propertyMat3;
        Dictionary<string, Matrix4> propertyMat4;
        Dictionary<string, Texture> propertyTextures;

        public Material(Shader shader)
        {
            this.shader = shader;

            propertyInts = new Dictionary<string, int>();
            propertyFloats = new Dictionary<string, float>();
            propertyBools = new Dictionary<string, bool>();
            propertyVec2 = new Dictionary<string, Vector2>();
            propertyVec3 = new Dictionary<string, Vector3>();
            propertyVec4 = new Dictionary<string, Vector4>();
            propertyMat3 = new Dictionary<string, Matrix3>();
            propertyMat4 = new Dictionary<string, Matrix4>();
            propertyTextures = new Dictionary<string, Texture>();
        }

        public Material()
        {
            propertyInts = new Dictionary<string, int>();
            propertyFloats = new Dictionary<string, float>();
            propertyBools = new Dictionary<string, bool>();
            propertyVec2 = new Dictionary<string, Vector2>();
            propertyVec3 = new Dictionary<string, Vector3>();
            propertyVec4 = new Dictionary<string, Vector4>();
            propertyMat3 = new Dictionary<string, Matrix3>();
            propertyMat4 = new Dictionary<string, Matrix4>();
            propertyTextures = new Dictionary<string, Texture>();
        }

        public void Apply()
        {
            if (shader == null) { return; }

            foreach (string key in propertyInts.Keys)
            {
                shader.Set(key, propertyInts[key]);
            }
            foreach (string key in propertyFloats.Keys)
            {
                shader.Set(key, propertyFloats[key]);
            }
            foreach (string key in propertyBools.Keys)
            {
                shader.Set(key, propertyBools[key]);
            }
            foreach (string key in propertyVec2.Keys)
            {
                shader.Set(key, propertyVec2[key]);
            }
            foreach (string key in propertyVec3.Keys)
            {
                shader.Set(key, propertyVec3[key]);
            }
            foreach (string key in propertyVec4.Keys)
            {
                shader.Set(key, propertyVec4[key]);
            }
            foreach (string key in propertyMat3.Keys)
            {
                shader.Set(key, propertyMat3[key]);
            }
            foreach (string key in propertyMat4.Keys)
            {
                shader.Set(key, propertyMat4[key]);
            }
            int i = 0;
            foreach (string key in propertyTextures.Keys)
            {
                shader.Set(key, i);
                propertyTextures[key].SetActiveUnit(TextureUnit.Texture0 + i);
                propertyTextures[key].Bind();
                i++;
            }
        }


        public void Set(string name, int value)
        {
            propertyInts[name] = value;
        }
        public void Set(string name, float value)
        {
            propertyFloats[name] = value;
        }
        public void Set(string name, bool value)
        {
            propertyBools[name] = value;
        }
        public void Set(string name, Vector2 value)
        {
            propertyVec2[name] = value;
        }
        public void Set(string name, Vector3 value)
        {
            propertyVec3[name] = value;
        }
        public void Set(string name, Vector4 value)
        {
            propertyVec4[name] = value;
        }
        public void Set(string name, Matrix3 value)
        {
            propertyMat3[name] = value;
        }
        public void Set(string name, Matrix4 value)
        {
            propertyMat4[name] = value;
        }
        
        public void Set(string name, Texture value)
        {
            propertyTextures[name] = value;
        }

        public int GetInt(string name)
        {
            if (propertyInts.TryGetValue(name, out int value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        public float GetFloat(string name)
        {
            if (propertyFloats.TryGetValue(name, out float value))
            {
                return value;
            }
            else
            {
                return 0f;
            }
        }

        public bool GetBool(string name)
        {
            if (propertyBools.TryGetValue(name, out bool value))
            {
                return value;
            }
            else
            {
                return false;
            }
        }

        public Vector2 GetVector2(string name)
        {
            if (propertyVec2.TryGetValue(name, out Vector2 value))
            {
                return value;
            }
            else
            {
                return Vector2.Zero;
            }
        }

        public Vector3 GetVector3(string name)
        {
            if (propertyVec3.TryGetValue(name, out Vector3 value))
            {
                return value;
            }
            else
            {
                return Vector3.Zero;
            }
        }

        public Vector4 GetVector4(string name)
        {
            if (propertyVec4.TryGetValue(name, out Vector4 value))
            {
                return value;
            }
            else
            {
                return Vector4.Zero;
            }
        }

        public Matrix3 GetMatrix3(string name)
        {
            if (propertyMat3.TryGetValue(name, out Matrix3 value))
            {
                return value;
            }
            else
            {
                return Matrix3.Identity;
            }
        }

        public Matrix4 GetMatrix4(string name)
        {
            if (propertyMat4.TryGetValue(name, out Matrix4 value))
            {
                return value;
            }
            else
            {
                return Matrix4.Identity;
            }
        }

        public Texture GetTexture(string name)
        {
            if (propertyTextures.TryGetValue(name, out Texture value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public int GetTextureCount()
        {
            return propertyTextures.Count;
        }

    }
}
