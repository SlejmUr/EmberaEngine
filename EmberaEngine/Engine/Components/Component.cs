using System;
using EmberaEngine.Engine.Core;
using MessagePack;

namespace EmberaEngine.Engine.Components
{
    public abstract class Component
    {
        public abstract string Type { get; }
        public Component()
        {

        }

        [IgnoreMember]
        public GameObject gameObject;
        public virtual void OnStart() { }
        public virtual void OnUpdate(float dt) { }

        public virtual void OnDestroy() { }

    }
}

