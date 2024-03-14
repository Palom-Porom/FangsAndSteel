using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class MainMenuState : IWorldState
{
    public IEnumerator Start()
    {
        //Local = ClientServerBootstrap.CreateLocalWorld(NetworkBootstrap.DefaultWorldName);

        ///TODO: Activate main menu GOs

        Debug.Log("MainMenuState.Start was done");

        return null;
    }

    //public static World Local { get; private set; }

    public void Dispose()
    {
        //Local.Dispose();

        ///TODO: Deactivate main menu GOs

        Debug.Log("MainMenuState.Dispose was done");
    }
}
