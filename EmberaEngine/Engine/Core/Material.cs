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
        }

        public void Use()
        {
            shader.Use();

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

            shader.Apply();
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

    }
}
