using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetCodeUITest : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Logger logger;

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
