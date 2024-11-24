using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    public class UIElement
    {

        public Vector4 dimensions;

        public bool isTabSelected = false;

        /// <summary>
        /// This is assigned true when the mouse is over the element
        /// </summary>
        public bool isMouseOver {
            get;
            internal set;
        }
        /// <summary>
        /// This property is assigned when mouseBtn event occurs over this element
        /// </summary>
        public MouseButtonEvent mouseBtnEvent;
        /// <summary>
        /// This property is true when isMouseOver property is true
        /// </summary>
        public MouseMoveEvent mouseMoveEvent;

        public virtual void Render() { }
        public virtual void Start() { }
        public virtual void Update() { }
    }

    public static class UIConstants
    {
        public static Texture circleKnob;
        public static Texture squareKnob;
        public static Texture stylizedKnob;
        public static Texture nullTexture;
        public static Texture border;

        public static Shader NineSliceShader;

        static UIConstants()
        {
            circleKnob = Helper.loadImageAsTex("./Engine/Content/Textures/UI/slider_circle.png");
            squareKnob = Helper.loadImageAsTex("./Engine/Content/Textures/UI/slider_square_knob.png");

            stylizedKnob = Helper.loadImageAsTex("./Engine/Content/Textures/UI/slider_stylized_knob.png");

            nullTexture = Helper.loadImageAsTex("./Engine/Content/Textures/Placeholders/null.png");
            border = Helper.loadImageAsTex("./Engine/Content/Textures/UI/border.png");

            NineSliceShader = new Shader("./Engine/Content/Shaders/2D/sprite2d.vert", "./Engine/Content/Shaders/2D/nine_slice.frag", "");
        }
    }

    public class UIManager
    {
        public UIManager() { }

        static List<UIElement> instances = new List<UIElement>();
        
        public static T CreateUIElement<T>(CanvasComponent canvasComponent) where T : UIElement, new()
        {
            T elem = new T();

            canvasComponent.GetCanvasObject().uiElements.Add(elem);

            instances.Add(elem);

            return elem;
        }

        static bool hasStarted = false;

        public static void Start()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                instances[i].Start();
            }
        }

        public static void Update()
        {
            if (!hasStarted)
            {
                hasStarted = true;
                Start();   
            }

            UpdateMouseEvent();
            UpdateKeyboardEvent();
            for (int i = 0; i < instances.Count; i++)
            {
                instances[i].Update();
            }
        }

        static void UpdateMouseEvent()
        {
            Vector2 mousePos = Input.mousePosition;

            for (int i = 0; i < instances.Count; i++)
            {
                UIElement instance = instances[i];

                if (IsMouseInside(instance.dimensions, mousePos))
                {
                    instance.isMouseOver = true;
                    instance.mouseBtnEvent = Input.mouseBtnEvent;
                    instance.mouseMoveEvent = Input.mouseMoveEvent;
                } else
                {
                    instance.isMouseOver = false;
                }

            }
        } 

        static void UpdateKeyboardEvent()
        {

        }

        public static bool IsMouseInside(Vector4 rect, Vector2 mousePos)
        {
            return (mousePos.X > rect.X && mousePos.Y > rect.Y && mousePos.X < rect.Z + rect.X && mousePos.Y < rect.W + rect.Y);
        }

        public static Vector2 GetRelativeMouseCoverage(Vector4 rect, Vector2 mousePosition)
        {
            float relativeX = (mousePosition.X - rect.X) / rect.Z;
            float relativeY = (mousePosition.Y - rect.Y) / rect.W;

            relativeY = 1 - relativeY;
            return new Vector2(relativeX, relativeY);
        }


}


public class UIButton : UIElement
    {

        public override void Render()
        {
            
        }

        public override void Update()
        {
            if (this.isMouseOver)
            {
                if (Input.GetMouseDown(MouseButton.Left))
                {
                    Console.WriteLine("BUTTON CLICKED");
                }
            }
        }
    }

    public class UISlider : UIElement
    {

        Vector4 knobDimensions;
        Vector4 sliderDimensions;
        Vector4 borderDimensions;

        float progress = 0;
        bool isAlreadyDragging = false;
        bool keepFilled = false;


        public override void Start()
        {
            sliderDimensions = dimensions;

            knobDimensions.W = this.dimensions.W;
            knobDimensions.Z = this.dimensions.W;

            knobDimensions.X = sliderDimensions.X - knobDimensions.Z / 2;

            sliderDimensions = dimensions;
            sliderDimensions.Y = dimensions.Y + (dimensions.W) / 4;
            sliderDimensions.W = (dimensions.W) / 2;

            borderDimensions = sliderDimensions;
            borderDimensions.X -= 3.4f;
            borderDimensions.Z += 6.8f;
            borderDimensions.Y -= 3.4f;
            borderDimensions.W += 6.8f;

            
        }

        public override void Update()
        {

            knobDimensions.W = this.dimensions.W;
            knobDimensions.Z = this.dimensions.W;

            knobDimensions.Y = dimensions.Y;


            if ((isMouseOver && UIManager.IsMouseInside(knobDimensions, mouseMoveEvent.position)) || isAlreadyDragging)
            {
                isAlreadyDragging = Input.isDragging;
                if (isAlreadyDragging)
                {
                    isAlreadyDragging = Input.isDragging;
                    


                    SetProgress((knobDimensions.X - sliderDimensions.X + knobDimensions.Z / 2) / (sliderDimensions.Z));
                }
            }


            else if (UIManager.IsMouseInside(sliderDimensions, Input.mousePosition))
            {
                if (Input.GetMouseDown(MouseButton.Left))
                {
                    SetProgress(UIManager.GetRelativeMouseCoverage(sliderDimensions, mouseMoveEvent.position).X);
                }
            }

        }

        public void SetProgress(float progress)
        {
            knobDimensions.X = Math.Clamp(Input.mousePosition.X, sliderDimensions.X, sliderDimensions.Z + sliderDimensions.X) - knobDimensions.Z / 2;
            this.progress = progress;
        }

        public float GetProgress()
        {
            return progress;
        }

        Vector4 newSliderDimension = new Vector4();

        public override void Render()
        {
            if (!keepFilled)
            {
                newSliderDimension = sliderDimensions;
                newSliderDimension.Z = sliderDimensions.Z * progress;
            } else
            {
                newSliderDimension = sliderDimensions;
            }

            UIConstants.NineSliceShader.Set("u_dimensions", borderDimensions.Zw);
            UIConstants.NineSliceShader.Set("borders", new Vector4(5,5,5,5));

            Renderer2D.RenderColorRect(new Color4(65, 65, 65, 255), sliderDimensions);
            Renderer2D.RenderCustomShaderRect(UIConstants.border, borderDimensions, UIConstants.NineSliceShader);
            Renderer2D.RenderTexturedRect(Texture.GetWhite2D(), newSliderDimension, 0);
            Renderer2D.RenderTexturedRect(UIConstants.stylizedKnob, knobDimensions, 0);
        }
    }

    public class UIText : UIElement
    {

    }


}
