using RK.Retales.Utility;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private GameObject lobbyScreen;

    [Header("Game Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private int laps = 3;

    [Header("Debug")]
    [SerializeField] private LogHandler logger;
    
    private int SpawnPointsLength => spawnPoints.Length;

    private void Awake() {
        Debug();
        Application.targetFrameRate = 300;
    }

    public override void OnDestroy() {
        GameEvents.OnPlayerJoined -= OnPlayerJoined;
        GameEvents.OnGameStart -= OnGameStart;
    }

    public override void OnNetworkSpawn() {
        GameEvents.OnPlayerJoined += OnPlayerJoined;
        GameEvents.OnGameStart += OnGameStart;
    }

    private void OnGameStart() {
        lobbyScreen.SetActive(false);
    }

    private void OnPlayerJoined(int current) {
        var prefix = IsHost
            ? "Start the game by pressing \nEnter or Start"
            : "Waiting for the Host to start the game...";

        playerCountText.SetText($"{prefix}\nPlayers: {current}/{maxPlayers}");
    }

    public Transform SpawnPointHandler(int playerNumber) {
        if(playerNumber >= maxPlayers) return spawnPoints[0];

        var spawnPoint = spawnPoints[playerNumber];
        return spawnPoint;
    }

    private void Debug() {
        if(!IsServer) return;
        if(SpawnPointsLength < maxPlayers || SpawnPointsLength == 0)
            logger.Log("Not enough spawn points!", this);

        if(SpawnPointsLength == 0) logger.Log("No checkpoints!", this);
    }
}