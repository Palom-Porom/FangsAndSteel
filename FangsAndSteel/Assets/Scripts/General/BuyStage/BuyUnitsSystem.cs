using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public partial class BuyUnitsSystem : SystemBase
{
    EntityQuery notBoughtYetQuery;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<BuyStageNotCompletedTag>();

        notBoughtYetQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<NotBoughtYetTag>().Build(EntityManager);
    }

    protected override void OnUpdate()
    {
        StaticUIData uiData = SystemAPI.GetSingleton<StaticUIData>();

        if (uiData.BuyInfantryManButton || uiData.BuyMachineGunnerButton || uiData.BuyAntyTankButton || uiData.BuyTankButton)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            ecb.DestroyEntity(notBoughtYetQuery, EntityQueryCaptureMode.AtRecord);

            PrefubsComponent prefubs = SystemAPI.GetSingleton<PrefubsComponent>();
            int currentTeam = SystemAPI.GetSingleton<CurrentTeamComponent>().value;
            Entity spawnedUnit = Entity.Null;

            #region Spawn needed type of Unit
            if (uiData.BuyInfantryManButton)
            {
                switch (currentTeam)
                {
                    case 1:
                        spawnedUnit = ecb.Instantiate(prefubs.FirstRobot_Red);
                        break;
                    case 2:
                        spawnedUnit = ecb.Instantiate(prefubs.FirstRobot_Blue);
                        break;
                }
            }
            else if (uiData.BuyMachineGunnerButton)
            {
                switch (currentTeam)
                {
                    case 1:
                        spawnedUnit = ecb.Instantiate(prefubs.MachineGunner_Red);
                        break;
                    case 2:
                        spawnedUnit = ecb.Instantiate(prefubs.MachineGunner_Blue);
                        break;
                }
            }
            else if (uiData.BuyAntyTankButton)
            {
                switch (currentTeam)
                {
                    case 1:
                        spawnedUnit = ecb.Instantiate(prefubs.AntyTank_Red);
                        break;
                    case 2:
                        spawnedUnit = ecb.Instantiate(prefubs.AntyTank_Blue);
                        break;
                }
            }
            else if (uiData.BuyTankButton)
            {
                switch (currentTeam)
                {
                    case 1:
                        spawnedUnit = ecb.Instantiate(prefubs.Tank_Red);
                        break;
                    case 2:
                        spawnedUnit = ecb.Instantiate(prefubs.Tank_Blue);
                        break;
                }
            }
            #endregion

            ecb.AddComponent<NotBoughtYetTag>(spawnedUnit);
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            ecb.RemoveComponent<NotBoughtYetTag>(notBoughtYetQuery, EntityQueryCaptureMode.AtRecord);
        }
    }
}


public struct NotBoughtYetTag : IComponentData { }
