using EmberaEngine.Engine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.CustomEditors
{
    public class CustomEditorScript
    {
        public Component component;

        public virtual void OnEnable() { }

        public virtual void OnGUI() { }
    }
}
