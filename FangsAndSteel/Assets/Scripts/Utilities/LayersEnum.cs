using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
enum layers
{
    Default = 1,
    TransparentFX = 2,
    IgnoreRaycast = 4,
    Water = 16,
    UI = 32,
    Terrain = 1024,
    Everything = 1079
}