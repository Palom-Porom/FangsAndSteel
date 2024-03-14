using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game
{
    public static IWorldState _curState;

    public static void SetState(IWorldState state)
    {
        _curState?.Dispose();
        _curState = state;
        _curState.Start();
    }

    public static void SetState<T>() where T : IWorldState, new() => SetState(new T());
}
