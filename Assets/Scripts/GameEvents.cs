using System;
using UnityEngine;

public class GameEvents {
    public static Action OnGameStart;
    public static Action<Transform, ulong> OnPlayerSpawn;
    
    public static void InvokeOnGameStart() {
        OnGameStart?.Invoke();
    }
    
    public static void InvokeOnPlayerSpawn(Transform transform, ulong clientId) {
        OnPlayerSpawn?.Invoke(transform, clientId);
    }
}