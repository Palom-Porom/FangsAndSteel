using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public interface IWorldState : IDisposable
{
    public IEnumerator Start();
}


public struct AuthorizeConnectionAsPlayer : IComponentData { }