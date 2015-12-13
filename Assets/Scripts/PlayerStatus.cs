using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerSetup))]
public class PlayerStatus : NetworkBehaviour
{
    public const int MaxHealth = 4;
    public const float InvincibilityDuration = 1f;

    public enum State
    {
        ForcedStill,
        Alive,
        Invincible,
        Dead
    }

    [SerializeField]
    GameObject healthIndicator;

    [SyncVar(hook = "OnPlayerHealthSynced")]
    int health = MaxHealth;
    [SyncVar]
    int currentState = (int)State.Alive;    // FIXME: change this to forcedstill at some point
    [SyncVar]
    bool reflectEnabled = false;  // FIXME: move this to another script

    PlayerSetup playerSetup;
    float timeLastInvincible = -1f;
    readonly GameObject[] healthIndicators = new GameObject[MaxHealth];

    #region Properties
    public int Health
    {
        get
        {
            return health;
        }
        private set
        {
            int setValueTo = Mathf.Clamp(value, 0, MaxHealth);
            if (health != setValueTo)
            {
                if (setValueTo < health)
                {
                    if (setValueTo > 0)
                    {
                        CurrentState = State.Invincible;
                        timeLastInvincible = (Time.time + InvincibilityDuration);
                    }
                    else
                    {
                        CurrentState = State.Dead;
                    }
                }
                CmdSetHealth(setValueTo);
            }
        }
    }

    public State CurrentState
    {
        get
        {
            return (State)currentState;
        }
        private set
        {
            int setValueTo = (int)value;
            if (currentState != setValueTo)
            {
                CmdSetState(setValueTo);
            }
        }
    }

    public bool IsReflectEnabled
    {
        get
        {
            return reflectEnabled;
        }
        set
        {
            if (reflectEnabled != value)
            {
                CmdSetReflect(value);
            }
        }
    }
    #endregion

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        SetupHud();

        // Reset variables
        Health = MaxHealth;
        CurrentState = State.Alive;
    }

    void Update()
    {
        if (isLocalPlayer == true)
        {
            UpdateInvincibleState();
            // FIXME: not working
            if (Input.GetKeyDown(KeyCode.E) == true)
            {
                foreach (KeyValuePair<string, PlayerSetup> pair in PlayerSetup.AllIdentifiedPlayers)
                {
                    if (pair.Key != name)
                    {
                        pair.Value.Status.Health -= 1;
                    }
                }
            }
        }
    }

    #region Commands
    [Command]
    void CmdSetHealth(int newHealth)
    {
        health = newHealth;
    }

    [Command]
    void CmdSetState(int newState)
    {
        currentState = newState;
    }

    [Command]
    void CmdSetReflect(bool newReflect)
    {
        reflectEnabled = newReflect;
    }
    #endregion

    #region Helper Methods
    [Client]
    private void OnPlayerHealthSynced(int latestHealth)
    {
        if (isLocalPlayer == true)
        {
            for (int i = 0; i < MaxHealth; ++i)
            {
                healthIndicators[i].SetActive(i < latestHealth);
            }
        }
    }

    [Client]
    private void UpdateInvincibleState()
    {
        if ((CurrentState == State.Invincible) && (Time.time > timeLastInvincible))
        {
            CurrentState = State.Alive;
        }
    }

    private void SetupHud()
    {
        healthIndicators[0] = healthIndicator;
        GameObject newIndicator = null;
        for (int i = 1; i < MaxHealth; ++i)
        {
            newIndicator = Instantiate<GameObject>(healthIndicator);
            newIndicator.transform.SetParent(healthIndicator.transform.parent, false);
            newIndicator.transform.SetAsLastSibling();
            newIndicator.transform.localScale = Vector3.one;
            healthIndicators[i] = newIndicator;
        }
    }
    #endregion
}
