using System;
using RK.Retales.Utility;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
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
    
    // TODO: Add Game Win-Lose Conditions

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

    
#if UNITY_EDITOR
        private void DrawCheckpointGizmos(Transform checkpoint)
        {
            Gizmos.color = Color.blue;

            var currentMatrix = Gizmos.matrix;
            Gizmos.matrix = checkpoint.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = currentMatrix;

            Gizmos.DrawSphere(checkpoint.transform.position, 0.5f);
        }

        private void OnDrawGizmos()
        {
            if (checkpoints == null || checkpoints.Length == 0) return;

            for (var i = 0; i < checkpoints.Length; i++)
            {
                if (checkpoints[i] == null) continue;

                DrawCheckpointGizmos(checkpoints[i]);

                // Draw a line towards the next point in the path
                if ((i + 1) < checkpoints.Length && checkpoints[i + 1] != null)
                {
                    StaticGizmoDrawer.DrawGizmoArrow(checkpoints[i].transform.position,
                        checkpoints[i + 1].transform.position - checkpoints[i].transform.position, Color.blue);
                }

                // Draw a line from the first to the last point if we're looping
                if (i == (checkpoints.Length - 1))
                {
                    StaticGizmoDrawer.DrawGizmoArrow(checkpoints[i].transform.position,
                        checkpoints[0].transform.position - checkpoints[i].transform.position, Color.blue);
                }
            }
        }
#endif
}
