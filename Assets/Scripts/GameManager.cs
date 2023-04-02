using RK.Retales.Utility;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    [Header("Game Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private int laps = 3;

    [Header("Debug")]
    [SerializeField] private LogHandler logger;

    private NetworkVariable<int> _numberOfPlayers = new();
    private NetworkVariable<int> _occupiedSpawnPoints = new();

    private void Awake() {
        Debug();
        Application.targetFrameRate = 300;
    }

    public override void OnNetworkSpawn() {
        AddPlayer();
        MovePlayerToSpawnPoint();
        StartGameWhenAllPlayersAreReady();
    }

    public override void OnNetworkDespawn() {
        RemovePlayer();
    }

    private void StartGame() {
        GameEvents.InvokeOnGameStart();
        logger.Log("Game started!", this);
    }

    private void StartGameWhenAllPlayersAreReady() {
        if(!IsHost) return;

        if(_numberOfPlayers.Value >= maxPlayers) StartGame();
    }

    private void AddPlayer() {
        if(!IsHost) return;

        _numberOfPlayers.Value++;
        logger.Log($"A player has joined! | Number of players: {_numberOfPlayers.Value}", this);
    }

    private void RemovePlayer() {
        if(!IsHost) return;

        _numberOfPlayers.Value--;
        logger.Log($"A player has left! | Number of players: {_numberOfPlayers.Value}", this);
    }

    private void MovePlayerToSpawnPoint() {
        var spawnPoint = spawnPoints[ReturnCurrentSpawnPoint()];
        GameEvents.InvokeOnPlayerSpawn(spawnPoint, OwnerClientId);
    }

    private int ReturnCurrentSpawnPoint() {
        if(checkpoints.Length < _occupiedSpawnPoints.Value) return ++_occupiedSpawnPoints.Value;
        
        logger.Log($"SpawnPoint overflow! max: {spawnPoints.Length}", this);
        return _occupiedSpawnPoints.Value;
    }

    private void Debug() {
        if(spawnPoints.Length < maxPlayers || spawnPoints.Length == 0) logger.Log("Not enough spawn points!", this);

        if(checkpoints.Length == 0) logger.Log("No checkpoints!", this);
    }
}