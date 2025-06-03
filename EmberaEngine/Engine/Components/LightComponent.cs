using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class LightComponent : Component
    {
        public override string Type => nameof(LightComponent);

        PointLight pointLight;
        SpotLight spotLight;
        DirectionalLight directionalLight;

        private bool enabled = true;
        private Vector3 color = Vector3.One;
        private float intensity = 10;
        private float radius = 30;

        private float outerCutoff = 1f;
        private float innerCutoff = 1f;
        private LightType lightType = LightType.PointLight;

        public LightType LightType
        {
            get { return lightType; }
            set
            {
                OnChangeLightType(lightType, value);
                lightType = value;
                
            }
        }

        public bool Enabled
        {
            get => enabled;
            set { enabled = value; OnChangedValue(); }
        }

        public Color4 Color
        {
            get => new Color4(pointLight.Color.X, pointLight.Color.Y, pointLight.Color.Z, 1);
            set { color = Helper.ToVector4(value).Xyz; OnChangedValue(); }
        }

        public float Radius
        {
            get => radius;
            set { radius = value; OnChangedValue(); }
        }

        public float Intensity
        {
            get => intensity;
            set { intensity = value; OnChangedValue(); }
        }

        public float InnerCutoff
        {
            get => innerCutoff;
            set { innerCutoff = value; OnChangedValue(); }
        }

        public float OuterCutoff
        {
            get => outerCutoff;
            set { outerCutoff = value; OnChangedValue(); }
        }

        public LightComponent()
        {
            pointLight = LightManager.AddPointLight(Vector3.Zero, color, intensity, radius);
        }

        public void OnChangeLightType(LightType previousValue, LightType newValue)
        {
            if (previousValue == newValue) return;

            if (previousValue == LightType.PointLight)
            {
                LightManager.RemovePointLight(pointLight);
            }
            else if (previousValue == LightType.SpotLight)
            {
                LightManager.RemoveSpotLight(spotLight);
            }

            if (newValue == LightType.PointLight)
            {
                pointLight = LightManager.AddPointLight(gameObject.transform.position, color, intensity, radius);
            } else if (newValue == LightType.SpotLight)
            {
                spotLight = LightManager.AddSpotLight(gameObject.transform.position, color, Vector3.Normalize(gameObject.transform.rotation), intensity, radius, innerCutoff, outerCutoff);
            } else if (newValue == LightType.DirectionalLight)
            {
                directionalLight = LightManager.AddDirectionalLight(gameObject.transform.rotation, color, intensity);
            }
        }

        public void OnChangedValue()
        {
            if (LightType == LightType.PointLight)
            {
                pointLight.position = gameObject.transform.position;
                pointLight.Color = color;
                pointLight.range = radius;
                pointLight.intensity = intensity;
                pointLight.enabled = enabled;
            }
            else if (LightType == LightType.SpotLight)
            {
                spotLight.position = gameObject.transform.position;
                spotLight.Color = color;
                spotLight.range = radius;
                spotLight.intensity = intensity;
                spotLight.enabled = enabled;
                spotLight.innerCutoff = innerCutoff;
                spotLight.outerCutoff = outerCutoff;
                spotLight.direction = Vector3.Normalize(gameObject.transform.rotation);
            }
            else if (lightType == LightType.DirectionalLight)
            {
                directionalLight.direction = gameObject.transform.rotation;
                directionalLight.color = color;
                directionalLight.intensity = intensity;
                directionalLight.enabled = enabled;
            }
        }

        public override void OnStart()
        {
            if (lightType == LightType.PointLight)
            {
                pointLight.position = gameObject.transform.position;
            } else if (lightType == LightType.SpotLight)
            {
                spotLight.position = gameObject.transform.position;
            }
        }

        public override void OnUpdate(float dt)
        {
            if (gameObject.transform.hasMoved)
            {
                OnChangedValue();
            }

        }
    }
}
