using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using OmiyaGames;

public class GameState : NetworkBehaviour
{
    public const string StartUpState = "Setup";
    static GameState instance = null;

    public enum MatchState
    {
        Setup,
        Countdown,
        Play,
        Finished
    }

    [SerializeField]
    float startupTime = 3f;

    [SyncVar(hook = "OnLosingPlayerSynced")]
    string losingPlayer = string.Empty;
    [SyncVar(hook = "OnMatchStartSynced")]
    double matchStart = -1f;

    readonly static Dictionary<string, PlayerSetup> allPlayers = new Dictionary<string, PlayerSetup>();
    static string localPlayerId = string.Empty;

    public static GameState Instance
    {
        get
        {
            return instance;
        }
    }

    public static string LocalPlayerId
    {
        set
        {
            localPlayerId = value;
        }
    }

    #region Unity events
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        instance = this;
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        instance = null;
    }
    #endregion

    public IEnumerable<PlayerSetup> Oppositions()
    {
        foreach (KeyValuePair<string, PlayerSetup> pair in allPlayers)
        {
            if (pair.Key != localPlayerId)
            {
                yield return pair.Value;
            }
        }
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

    public static void UpdatePlayerSetup(PlayerSetup setup = null, string formerName = null)
    {
        if ((string.IsNullOrEmpty(formerName) == false) && (allPlayers.ContainsKey(formerName) == true))
        {
            allPlayers.Remove(formerName);
        }
        if (setup != null)
        {
            if (allPlayers.ContainsKey(setup.name) == true)
            {
                allPlayers[setup.name] = setup;
            }
            else
            {
                allPlayers.Add(setup.name, setup);
            }
        }

        // Check if the proper number of players are connected
        if((Instance != null) && (allPlayers.Count >= GameSetup.MaxConnections))
        {
            Instance.CmdStartMatch();
        }
    }

    #region Command methods
    [Command]
    public void CmdSetLosingPlayer(string playerId)
    {
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
        if(localPlayerId == playerLost)
        {
            Singleton.Get<MenuManager>().Show<LevelFailedMenu>(CheckButton);
        }
        else
        {
            Singleton.Get<MenuManager>().Show<LevelCompleteMenu>(CheckButton);
        }
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
