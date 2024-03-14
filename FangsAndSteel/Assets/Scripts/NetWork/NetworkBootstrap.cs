using System.Collections;
using System.Collections.Generic;
using Unity.NetCode;
using UnityEngine;

public class NetworkBootstrap : ClientServerBootstrap
{
    public static string DefaultWorldName { get; private set; }

    public override bool Initialize(string defaultWorldName)
    {
        DefaultWorldName = defaultWorldName;
        Game.SetState<MainMenuState>();
        return true;
    }
}
