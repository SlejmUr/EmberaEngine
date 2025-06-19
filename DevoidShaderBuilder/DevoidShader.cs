namespace DevoidShaderBuilder
{
    public class DevoidShader
    {
        string glslShaderVersion;
        string sourceCode;


        List<Sampler2D> sampler2Ds;



        public DevoidShader(string version)
        {
            this.glslShaderVersion = version;


            AddSampler2D();

            Build();
        }

        public string GetSource()
        {
            return sourceCode;
        }


        public void AddSampler2D()
        {
            Sampler2D sampler2D = new Sampler2D();
            sampler2D.Initialize("texture1");

            sampler2Ds.Add(sampler2D);
        }


        public void Build()
        {
            string shaderSource = "";

            // Add version metadata
            shaderSource += "#version " + glslShaderVersion + " core\n";

            for (int i = 0; i < sampler2Ds.Count; i++)
            {
                shaderSource += sampler2Ds[i].ToGLSL() + "\n";
            }



        }



    }
}
