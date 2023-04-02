using RK.Retales.Utility;
using Unity.Netcode;
using UnityEngine;

public class NetCodeUITest : MonoBehaviour {
    [SerializeField] private LogHandler logger;

    public void OnHostGame() {
        logger.Log("Hosting...", this);
        NetworkManager.Singleton.StartHost();
        Hide();
    }

    public void OnJoinGame() {
        logger.Log("Joining...", this);
        NetworkManager.Singleton.StartClient();
        logger.Log($"Joined: {NetworkManager.Singleton.ConnectedHostname}", this);
        Hide();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}