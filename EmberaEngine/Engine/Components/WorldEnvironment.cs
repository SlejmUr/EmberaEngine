using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    class WorldEnvironment : Component
    {
        public override string Type => nameof(WorldEnvironment);

        private float exposure = 1f;
        private TonemapFunction tonemapFunction = TonemapFunction.ACES;
        private bool useBloom = true;
        private bool useSSAO = true;
        private bool useIBL = true;
        private bool renderSkybox = true;
        private Color4 ambientColor = Color4.Black;
        private float ambientFactor = 1f;


        private RenderSetting renderSetting;

        public float Exposure
        {
            get => exposure;
            set
            {
                exposure = value;
                OnChangeValue();
            }
        }

        public TonemapFunction Tonemapper
        {
            get => tonemapFunction;
            set
            {
                tonemapFunction = value;
                OnChangeValue();
            }
        }

        public bool UseBloom
        {
            get => useBloom;
            set
            {
                useBloom = value;
                OnChangeValue();
            }
        }

        public bool UseSSAO
        {
            get => useSSAO;
            set
            {
                useSSAO = value;
                OnChangeValue();
            }
        }

        public bool UseIBL
        {
            get => useIBL;
            set
            {
                useIBL = value;
                OnChangeValue();
            }
        }

        public bool RenderSkybox
        {
            get => renderSkybox;
            set
            {
                renderSkybox = value;
                OnChangeValue();
            }
        }

        public Color4 AmbientColor
        {
            get => ambientColor;
            set
            {
                ambientColor = value;
                OnChangeValue();
            }
        }

        public float AmbientFactor
        {
            get => ambientFactor;
            set
            {
                ambientFactor = value;
                OnChangeValue();
            }
        }


        public override void OnStart()
        {
            OnChangeValue();
        }

        public void OnChangeValue()
        {
            renderSetting = Renderer3D.ActiveRenderingPipeline.GetRenderSettings();

            renderSetting.useBloom = useBloom;
            renderSetting.useSSAO = useSSAO;
            renderSetting.Exposure = exposure;
            renderSetting.tonemapFunction = tonemapFunction;
            renderSetting.useIBL = useIBL;
            renderSetting.useSkybox = renderSkybox;
            renderSetting.AmbientColor = ambientColor;
            renderSetting.AmbientFactor = ambientFactor;

            Renderer3D.ActiveRenderingPipeline.SetRenderSettings(renderSetting);
        }

        public override void OnUpdate(float dt)
        {

        }

        public override void OnDestroy()
        {

        }

    }
}
