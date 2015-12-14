using UnityEngine;
using UnityEngine.Networking;
using OmiyaGames;
using System.Collections;

[RequireComponent(typeof(NetworkManager))]
public class GameSetup : ISingletonScript
{
    public const int MaxConnections = 2;
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

    [SerializeField]
    GameState gameInfoPrefab;

    GameState currentState;
    SceneManager scenes;
    NetworkManager network;
    Singleton eventBind;
    string queueLocalId = null;

    #region Properties
    public GameState Info
    {
        get
        {
            return currentState;
        }
    }

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
#endregion

    public override void SingletonAwake(Singleton instance)
    {
        scenes = Singleton.Get<SceneManager>();
        network = Singleton.Get<NetworkManager>();

        // Only allow 2 players to connect
        network.maxConnections = MaxConnections;
    }

    public override void SceneAwake(Singleton instance)
    {
        // Check if we're in the game scene
        if (scenes.CurrentScene == scenes.Levels[0])
        {
            //// Setup cursors
            //SceneManager.CursorMode = CursorLockMode.None;
            //Singleton.Get<MenuManager>().CursorModeOnPause = CursorLockMode.None;

            //// Setup game state
            //GameObject clone = Instantiate(gameInfoPrefab.gameObject);
            //NetworkServer.Spawn(clone);
            if(eventBind != null)
            {
                eventBind.OnUpdate -= CheckPlayerNumber;
            }
            instance.OnUpdate += CheckPlayerNumber;
            eventBind = instance;
        }
        else
        {
            currentState = null;
            GameState.Reset();
        }
    }

    private void CheckPlayerNumber(float obj)
    {
        if((queueLocalId != null) && (Info == null))
        {
            // Check if the server is active
            if (NetworkServer.active == true)
            {
                // Spawn GameState
                GameObject clone = Instantiate(gameInfoPrefab.gameObject);
                NetworkServer.Spawn(clone);

                // Update its information
                currentState = clone.GetComponent<GameState>();
                currentState.LocalPlayerId = queueLocalId;

                // Indicate we're done
                queueLocalId = null;
            }
        }
        else if((GameState.NumPlayers >= MaxConnections) && (Info != null))
        {
            // Check if the proper number of players are connected
            Info.CmdStartMatch();
            eventBind.OnUpdate -= CheckPlayerNumber;
            eventBind = null;
        }
    }

    public GameState.MatchState State
    {
        get
        {
            GameState.MatchState state = GameState.MatchState.Setup;
            if (currentState != null)
            {
                state = currentState.State;
            }
            return state;
        }
    }

    public void Setup(string localId)
    {
        // Setup game state
        queueLocalId = localId;
    }
}
