using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct NewWorldsBootstrap : ICustomBootstrap
{
    public bool Initialize(string defaultWorldName)
    {
        World replayStartWorld = new World("ReplayStartWorld", WorldFlags.Simulation);
        World replayWorld = new World("ReplayWorld", WorldFlags.Game);

        //replayStartWorld.EntityManager.CopyAndReplaceEntitiesFrom(World.DefaultGameObjectInjectionWorld.EntityManager);
        //Debug.Log(replayWorld.EntityManager.GetAllEntities(Unity.Collections.Allocator.Temp).Length);
        //Debug.Log(replayStartWorld.EntityManager.GetAllEntities(Unity.Collections.Allocator.Temp).Length);
        //Debug.Log(World.DefaultGameObjectInjectionWorld.EntityManager.GetAllEntities(Unity.Collections.Allocator.Temp).Length);

        return false;
    }
}