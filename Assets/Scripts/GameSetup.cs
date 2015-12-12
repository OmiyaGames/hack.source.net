using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkManager))]
public class GameSetup : MonoBehaviour
{
    void Awake()
    {
        // Only allow 2 players to connect
        NetworkManager manager = GetComponent<NetworkManager>();
        manager.maxConnections = 2;
    }
}
