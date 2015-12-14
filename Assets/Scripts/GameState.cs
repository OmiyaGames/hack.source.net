using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using OmiyaGames;

public class GameState : NetworkBehaviour
{
    public const string StartUpState = "Setup";

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
    string localPlayerId = string.Empty;

    public static int NumPlayers
    {
        get
        {
            return allPlayers.Count;
        }
    }

    public static void Reset()
    {
        allPlayers.Clear();
    }

    public string LocalPlayerId
    {
        set
        {
            localPlayerId = value;
        }
    }

    void Start()
    {
        Singleton.Get<GameSetup>().Info = this;
    }

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
        if (localPlayerId == playerLost)
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
