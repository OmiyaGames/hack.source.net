using UnityEngine;
using UnityEngine.Networking;
using OmiyaGames;

[RequireComponent(typeof(NetworkManager))]
public class GameSetup : MonoBehaviour
{
    public const string playerBulletLayer = "Player Bullet",
        oppositionBulletLayer = "Opposition Bullet",
        neutralBulletLayer = "Neautral Bullet",
        playerAvatarLayer = "Player Avatar",
        oppositionAvatarLayer = "Opposition Avatar";

    static int playerBulletLayerCache = -1,
        oppositionBulletLayerCache = -1,
        neutralBulletLayerCache = -1,
        playerAvatarLayerCache = -1,
        oppositionAvatarLayerCache = -1;

    public static int playerBulletLayerInt
    {
        get
        {
            if(playerBulletLayerCache < 0)
            {
                playerBulletLayerCache = LayerMask.NameToLayer(playerBulletLayer);
            }
            return playerBulletLayerCache;
        }
    }
    public static int oppositionBulletLayerInt
    {
        get
        {
            if (oppositionBulletLayerCache < 0)
            {
                oppositionBulletLayerCache = LayerMask.NameToLayer(oppositionBulletLayer);
            }
            return oppositionBulletLayerCache;
        }
    }
    public static int neutralBulletLayerInt
    {
        get
        {
            if (neutralBulletLayerCache < 0)
            {
                neutralBulletLayerCache = LayerMask.NameToLayer(neutralBulletLayer);
            }
            return neutralBulletLayerCache;
        }
    }
    public static int playerAvatarLayerInt
    {
        get
        {
            if (playerAvatarLayerCache < 0)
            {
                playerAvatarLayerCache = LayerMask.NameToLayer(playerAvatarLayer);
            }
            return playerAvatarLayerCache;
        }
    }
    public static int oppositionAvatarLayerInt
    {
        get
        {
            if (oppositionAvatarLayerCache < 0)
            {
                oppositionAvatarLayerCache = LayerMask.NameToLayer(oppositionAvatarLayer);
            }
            return oppositionAvatarLayerCache;
        }
    }
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
