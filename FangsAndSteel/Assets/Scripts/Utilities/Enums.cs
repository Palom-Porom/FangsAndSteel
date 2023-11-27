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
    Raycast = 8,
    Water = 16,
    UI = 32,
    Selectable = 128,
    Terrain = 1024,
    Everything = 1215
}

enum basicAnims
{
    Rest,
    Movement,
    Attack,
    Recharge,
    Death
}