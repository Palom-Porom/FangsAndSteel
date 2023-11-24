using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SceneAuthoring : MonoBehaviour
{
    public enum Scenes { Game, MainMenu };

    public Scenes scene;

    public class Baker : Baker<SceneAuthoring>
    {
        public override void Bake(SceneAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            switch (authoring.scene)
            {
                case Scenes.Game:
                    AddComponent<GameTag>(entity);
                    break;
                case Scenes.MainMenu:
                    AddComponent<MainMenuTag>(entity);
                    break;
                default:
                    throw new System.Exception("Wrongly Setted Scene enum");
            }
        }
    }
}


public struct GameTag : IComponentData { }
public struct MainMenuTag : IComponentData { }
