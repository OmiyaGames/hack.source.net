using UnityEngine;
using UnityEngine.Networking;
using OmiyaGames;

public class GameState : NetworkBehaviour
{
    public enum MatchState
    {
        Setup,
        Countdown,
        Play,
        Finished
    }

    private static GameState instance;

    public static GameState Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField]
    float startupTime = 3f;

    [SyncVar(hook = "OnLosingPlayerSynced")]
    string losingPlayer = string.Empty;
    [SyncVar(hook = "OnMatchStartSynced")]
    double matchStart = -1f;

    void Start()
    {
        instance = this;
    }

    void OnDestroy()
    {
        instance = null;
    }

    public MatchState State
    {
        get
        {
            MatchState state = MatchState.Setup;
            if (string.IsNullOrEmpty(losingPlayer) == false)
            {
                state = MatchState.Finished;
            }
            else if (matchStart > 0)
            {
                state = MatchState.Countdown;
                if (Network.time > (matchStart + startupTime))
                {
                    state = MatchState.Play;
                }
            }
            return state;
        }
    }

    #region Command methods
    [Command]
    public void CmdSetLosingPlayer(string playerId)
    {
        Debug.Log("GameState: set lose to " + playerId);
        losingPlayer = playerId;
    }

    [Command]
    public void CmdStartMatch()
    {
        matchStart = Network.time;
    }
    #endregion

    // Update is called once per frame
    void OnLosingPlayerSynced(string playerLost)
    {
        Debug.Log("GameState: show window");
        //if (localPlayerId != playerLost)
        //{
        //    Singleton.Get<MenuManager>().Hide<PauseMenu>();
        //    if (Singleton.Get<MenuManager>().NumManagedMenus <= 0)
        //    {
        //        Singleton.Get<MenuManager>().Show<LevelCompleteMenu>(CheckButton);
        //    }
        //}
    }

    void OnMatchStartSynced(double matchTime)
    {
        // FIXME: notify all players to respawn
    }

    void CheckButton(IMenu menu)
    {
        // Do nothing for now
    }
}
