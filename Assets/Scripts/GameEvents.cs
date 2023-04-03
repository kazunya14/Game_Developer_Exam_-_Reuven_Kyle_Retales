using System;
using Unity.Netcode;

public class GameEvents : NetworkBehaviour {
    public static Action OnGameStart;
    public static Action<int> OnPlayerJoined;
    
    public static void InvokeOnGameStart() {
        OnGameStart?.Invoke();
    }
    
    public static void InvokeOnPlayerJoined(int playerNumber) {
        OnPlayerJoined?.Invoke(playerNumber);
    }
}