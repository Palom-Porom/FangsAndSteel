using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Scenes;
using Unity.VisualScripting;
using UnityEngine;

public class ClientState : IWorldState
{
    private readonly NetworkEndpoint _networkEndpoint;
    public static World Client { get; private set; }

    public ClientState(NetworkEndpoint networkEndpoint)
    {
        if (!networkEndpoint.IsValid)
        {
            throw new($"Address: {networkEndpoint.Address} is not valid.");
        }

        _networkEndpoint = networkEndpoint;
        Debug.Log("ClientState instance was created");
    }

    public IEnumerator Start()
    {
        Client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        World.DefaultGameObjectInjectionWorld = Client;

        var list = new List<Entity>();
        foreach (var subScene in Object.FindObjectsByType<SubScene>(FindObjectsSortMode.None))
        {
            if (subScene.AutoLoadScene && subScene.SceneName != "TutorialScene")
            {
                var scene = SceneSystem.LoadSceneAsync(Client.Unmanaged, subScene.SceneGUID);
                Client.EntityManager.AddComponentObject(scene, subScene);
                list.Add(scene);
            }
        }

        while (list.Count > 0)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (SceneSystem.IsSceneLoaded(Client.Unmanaged, list[i]))
                {
                    list.RemoveAt(i);
                }
            }

            yield return null;
            //await UniTask.Yield(ct);
        }

        using var drvQuery = Client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        var connection = drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW
            .Connect(Client.EntityManager, _networkEndpoint);
        Client.EntityManager.AddComponent<AuthorizeConnectionAsPlayer>(connection);

        //Game.Messenger.Send(Navigate<GameLobby>.Message(true));

        Debug.Log("ClientState.Start was done");
    }

    public void Dispose()
    {
        if (Client != null && Client.IsCreated)
        {
            Client.Dispose();
        }

        Client = null;
        Debug.Log("ClientState.Dispose was done");
    }
}
