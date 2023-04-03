using System;
using QFSW.QC.Actions;
using RK.Retales.Utility;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    [Header("Game Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private int laps = 3;

    [Header("Debug")]
    [SerializeField] private LogHandler logger;
    
    private int _currentNumberOfPlayers;
    
    private int SpawnPointsLength => spawnPoints.Length;


    private NetworkVariable<int> _numberOfPlayers = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone);

    private void Awake() {
        Debug();
        Application.targetFrameRate = 300;
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        
        GameEvents.InvokeOnGameManagerReady();
        StartGameWhenAllPlayersAreReady();
    }
    private void StartGame() {
        GameEvents.InvokeOnGameStart();
        logger.Log("Game started!", this);
    }

    private void StartGameWhenAllPlayersAreReady() {
        if(_currentNumberOfPlayers >= maxPlayers) StartGame();
    }
    
    public Transform GetSpawnPoint(int playerNumber) {
        if(_currentNumberOfPlayers >= maxPlayers) return spawnPoints[0];
        
        var spawnPoint = spawnPoints[playerNumber];
        _currentNumberOfPlayers++;
        return spawnPoint;
    }

    private void Debug() {
        if(SpawnPointsLength < maxPlayers || SpawnPointsLength == 0)
            logger.Log("Not enough spawn points!", this);

        if(SpawnPointsLength == 0) logger.Log("No checkpoints!", this);
    }
}