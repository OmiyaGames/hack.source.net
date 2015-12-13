using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(PlayerSetup))]
[RequireComponent(typeof(CharacterController))]
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

    [Header("Reflection")]
    [SerializeField]
    GameObject reflector;
    [SerializeField]
    float reflectDuration = 1f;
    [SerializeField]
    float cooldownDuration = 0.5f;

    [SyncVar(hook = "OnPlayerHealthSynced")]
    int health = MaxHealth;
    [SyncVar]
    int currentState = (int)State.Alive;    // FIXME: change this to forcedstill at some point
    [SyncVar(hook = "OnReflectionSynced")]
    bool reflectEnabled = false;

    PlayerSetup playerSetup;
    CharacterController controller;
    float timeLastInvincible = -1f, timeRemoveReflector = -1f, timeAllowReflector = -1f;
    readonly GameObject[] healthIndicators = new GameObject[MaxHealth];

    #region Properties
    public int Health
    {
        get
        {
            return health;
        }
        set
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
        playerSetup = GetComponent<PlayerSetup>();
        controller = GetComponent<CharacterController>();
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
            UpdateReflection();
        }
    }

    [Client]
    void OnControllerColliderHit(ControllerColliderHit info)
    {
        // Only check if this very character is hit
        if((isLocalPlayer == true) && (info.collider.CompareTag("Bullet") == true))
        {
            Bullet bullet = null;
            if(Bullet.TryGetBullet(info.collider, out bullet) == true)
            {
                bullet.PlayerHit(controller, this);
            }
        }
    }

    void OnReflectionSynced(bool newReflectStatus)
    {
        reflector.SetActive(newReflectStatus);
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

    [Client]
    private void UpdateReflection()
    {
        if((CurrentState != State.Dead) && (IsReflectEnabled == false) && (Time.time > timeAllowReflector))
        {
            // Check if the player pressed reflection
            Debug.Log("Waiting for input");
            if ((CrossPlatformInputManager.GetButtonDown("Reflect") == true) && ((playerSetup.CurrentActiveControls & PlayerSetup.ActiveControls.Reflect) != 0))
            {
                Debug.Log("Reflect detected");
                IsReflectEnabled = true;
                timeRemoveReflector = Time.time + reflectDuration;
                timeAllowReflector = timeRemoveReflector + cooldownDuration;
            }
        }
        else if((IsReflectEnabled == true) && (Time.time > timeRemoveReflector))
        {
            IsReflectEnabled = false;
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
