﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class RectTransform : Component
    {
        public override string Type => nameof(RectTransform);

        public Vector2 Position;
        public Vector2 Size;

    }
}
