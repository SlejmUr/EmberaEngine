using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    public abstract class Panel
    {
        public EditorLayer editor;

        public virtual void OnAttach() { }
        public virtual void OnGUI() { }
        public virtual void OnRender() { }
        public virtual void OnLateRender() { }

        public virtual void OnUpdate(float dt) { }

        public virtual void OnMouseButton(MouseButtonEvent buttonEvent) { }
        // This method is called when any mouse movement is made
        public virtual void OnMouseMove(MouseMoveEvent moveEvent) { }
        // This method is called whenever the application window is resized
        public virtual void OnMouseWheel(MouseWheelEvent mouseWheel) { }
        // This method is called whenever the application detects a key down event
        public virtual void OnKeyDown(KeyboardEvent key) { }
        // This method is called whenever the application detects a key up event
        public virtual void OnKeyUp(KeyboardEvent key) { }
    }
}
