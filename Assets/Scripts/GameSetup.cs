using UnityEngine;
using UnityEngine.Networking;
using OmiyaGames;

[RequireComponent(typeof(NetworkManager))]
public class GameSetup : MonoBehaviour
{
    void Awake()
    {
        // Only allow 2 players to connect
        NetworkManager manager = GetComponent<NetworkManager>();
        manager.maxConnections = 2;
    }

    void Start()
    {
        SceneManager.CursorMode = CursorLockMode.None;
        Singleton.Get<MenuManager>().CursorModeOnPause = CursorLockMode.None;
    }
}
