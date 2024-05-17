using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public partial class BuyUnitsSystem : SystemBase
{
    EntityQuery notBoughtYetQuery;
    EntityQuery allSelectedQuery;

    Entity unitStatsRqstEntity;

    ComponentLookup<SelectTag> selectLookup;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<BuyStageNotCompletedTag>();

        notBoughtYetQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<NotBoughtYetTag>().Build(EntityManager);
        allSelectedQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<SelectTag, UnitTypeComponent>().Build(this);

        selectLookup = SystemAPI.GetComponentLookup<SelectTag>();
    }

    protected override void OnUpdate()
    {
        StaticUIData uiData = SystemAPI.GetSingleton<StaticUIData>();

        if (uiData.buyInfantryManButton || uiData.buyMachineGunnerButton || uiData.buyAntyTankButton || uiData.buyTankButton)
        {
            int curMoney = int.Parse(StaticUIRefs.Instance.BalanceText.text);

            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            ecb.DestroyEntity(notBoughtYetQuery, EntityQueryCaptureMode.AtRecord);

            PrefubsComponent prefubs = SystemAPI.GetSingleton<PrefubsComponent>();
            int currentTeam = SystemAPI.GetSingleton<CurrentTeamComponent>().value;
            Entity spawnedUnit = Entity.Null;

            #region Spawn needed type of Unit
            if (uiData.buyInfantryManButton)
            {
                int price = int.Parse(StaticUIRefs.Instance.FirstRobotPrice.text);
                if (curMoney - price < 0)
                    return;
                else
                    StaticUIRefs.Instance.BalanceText.text = (curMoney - price).ToString();
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
            else if (uiData.buyMachineGunnerButton)
            {
                int price = int.Parse(StaticUIRefs.Instance.MachineGunnerPrice.text);
                if (curMoney - price < 0)
                    return;
                else
                    StaticUIRefs.Instance.BalanceText.text = (curMoney - price).ToString();
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
            else if (uiData.buyAntyTankButton)
            {
                int price = int.Parse(StaticUIRefs.Instance.AntyTankPrice.text);
                if (curMoney - price < 0)
                    return;
                else
                    StaticUIRefs.Instance.BalanceText.text = (curMoney - price).ToString();
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
            else if (uiData.buyTankButton)
            {
                int price = int.Parse(StaticUIRefs.Instance.TankPrice.text);
                if (curMoney - price < 0)
                    return;
                else
                    StaticUIRefs.Instance.BalanceText.text = (curMoney - price).ToString();
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
        else if (uiData.removeUnitButton)
        {
            int returnedMoney = 0;
            var unitTypes = allSelectedQuery.ToComponentDataArray<UnitTypeComponent>(Allocator.Temp);
            foreach(var type in unitTypes)
            {
                switch (type.value)
                {
                    case UnitTypes.BaseInfantry:
                        returnedMoney += int.Parse(StaticUIRefs.Instance.FirstRobotPrice.text);
                        break;
                    case UnitTypes.MachineGunner:
                        returnedMoney += int.Parse(StaticUIRefs.Instance.MachineGunnerPrice.text);
                        break;
                    case UnitTypes.AntyTank:
                        returnedMoney += int.Parse(StaticUIRefs.Instance.AntyTankPrice.text);
                        break;
                    case UnitTypes.Tank:
                        returnedMoney += int.Parse(StaticUIRefs.Instance.TankPrice.text);
                        break;
                }
            }
            StaticUIRefs.Instance.BalanceText.text = (int.Parse(StaticUIRefs.Instance.BalanceText.text) + returnedMoney).ToString();

            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            ecb.DestroyEntity(allSelectedQuery, EntityQueryCaptureMode.AtRecord);
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject() && !notBoughtYetQuery.IsEmpty)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            selectLookup.Update(this);
            if (!SystemAPI.TryGetSingletonEntity<UnitStatsRequestTag>(out unitStatsRqstEntity))
                unitStatsRqstEntity = Entity.Null;
            Dependency = new DeselectAllUnitsJob
            {
                selectLookup = selectLookup,
                ecb = ecb.AsParallelWriter(),

                unitStatsRqstEntity = unitStatsRqstEntity
            }.Schedule(allSelectedQuery, Dependency);
            Dependency.Complete();
            ecb.RemoveComponent<NotBoughtYetTag>(notBoughtYetQuery, EntityQueryCaptureMode.AtRecord);
        }
    }
}


public struct NotBoughtYetTag : IComponentData { }
