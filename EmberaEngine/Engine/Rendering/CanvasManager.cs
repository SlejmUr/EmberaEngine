using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    public class CanvasObject
    {
        public Vector2 position;
        public Vector2 size;
        public List<UIElement> uiElements;

        public CanvasObject()
        {
            position = new Vector2();
            size = new Vector2();
            uiElements = new List<UIElement>();
        }

        public void Update()
        {

        }

        public void Destroy()
        {
        }

    }

    public class CanvasRenderer
    {

        public static List<CanvasObject> CanvasObjects = new List<CanvasObject>();

        public static void Render()
        {
            for (int i = 0; i < CanvasObjects.Count; i++)
            {
                CanvasObject obj = CanvasObjects[i];

                List<UIElement> uiElements = obj.uiElements;

                for (int j = 0; j < uiElements.Count; j++)
                {
                    uiElements[j].Render();
                }

            }
        }

        public static CanvasObject AddCanvasObject()
        {
            CanvasObject obj = new CanvasObject();

            obj.position = new Vector2(0, 0);
            obj.size = Helper.ToNumerics2(Screen.Size);

            CanvasObjects.Add(obj);
            return obj;
        }

        public static void DestroyCanvasObject(CanvasObject obj)
        {
            CanvasObjects.Remove(obj);
        }


    }
}
