using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public class Input
    {

        public static Vector2 mousePosition = Vector2.Zero;
        public static Vector2 mouseDelta = Vector2.Zero;
        public static Vector2 mouseScrollDelta = Vector2.Zero;
        public static MouseButtonEvent mouseBtnEvent;
        public static MouseMoveEvent mouseMoveEvent;

        public static MouseButtonEvent previousBtnEvent;

        public static bool isDragging;

        public static void OnMouseInput(MouseButtonEvent e)
        {
            mouseBtnEvent = e;
        }

        public static void OnMouseMove(MouseMoveEvent e)
        {

            mouseMoveEvent = e;
            mousePosition = e.position;
            mouseDelta = e.delta;
            

            if (mouseBtnEvent.IsPressed)
            {
                isDragging = true;
            } else
            {
                isDragging = false;
            }
            
        }

        public static void OnMouseWheel(MouseWheelEvent e)
        {
            mouseScrollDelta = e.Offset;
        }

        public static bool IsPressed(MouseButton btn)
        {
            if (btn == mouseBtnEvent.Button)
            {
                if (mouseBtnEvent.IsPressed)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GetMouseDown(MouseButton btn)
        {
            if (btn == mouseBtnEvent.Button)
            {
                if (previousBtnEvent.Button == mouseBtnEvent.Button)
                {
                    if (mouseBtnEvent.Action != InputAction.Release && previousBtnEvent.Action == InputAction.Press)
                    {
                        return false;
                    }
                }

                previousBtnEvent = mouseBtnEvent;
                return mouseBtnEvent.IsPressed;
            }
            return false;
        }

        // --- KEYBOARD ---
        private static HashSet<Keys> keysDown = new();
        private static HashSet<Keys> keysPressedThisFrame = new();
        private static HashSet<Keys> keysReleasedThisFrame = new();

        public static void OnKeyDown(Keys key)
        {
            if (!keysDown.Contains(key))
            {
                keysDown.Add(key);
                keysPressedThisFrame.Add(key);
            }
        }

        public static void OnKeyUp(Keys key)
        {
            if (keysDown.Contains(key))
            {
                keysDown.Remove(key);
                keysReleasedThisFrame.Add(key);
            }
        }

        public static bool GetKey(Keys key)
        {
            return keysDown.Contains(key);
        }

        public static bool GetKeyDown(Keys key)
        {
            Console.WriteLine(keysPressedThisFrame.Contains(key));
            return keysPressedThisFrame.Contains(key);
        }

        public static bool GetKeyUp(Keys key)
        {
            return keysReleasedThisFrame.Contains(key);
        }

        /// <summary>
        /// Call this at the start of every frame to reset per-frame key states.
        /// </summary>
        public static void Update()
        {
            mouseScrollDelta = Vector2.Zero;
            keysPressedThisFrame.Clear();
            keysReleasedThisFrame.Clear();
        }


    }
}
