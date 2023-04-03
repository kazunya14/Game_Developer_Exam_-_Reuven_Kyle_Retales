using System;
using Unity.Netcode;

public class GameEvents : NetworkBehaviour {
    public static Action OnGameStart;
    public static Action OnGameManagerReady;
    
    public static void InvokeOnGameStart() {
        OnGameStart?.Invoke();
    }
    
    public static void InvokeOnGameManagerReady() {
        OnGameManagerReady?.Invoke();
    }
    
}