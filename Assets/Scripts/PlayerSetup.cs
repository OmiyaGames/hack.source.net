using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using OmiyaGames;

public class PlayerSetup : NetworkBehaviour
{
    [System.Flags]
    public enum ActiveControls
    {
        None = 0,
        Forward = 1 << 0,
        Back = 1 << 1,
        Right = 1 << 2,
        Left = 1 << 3,
        Jump = 1 << 4,
        Run = 1 << 5,
        Reflect = 1 << 6,

        NumControls = 7,

        // combinations
        All = Forward | Back | Right | Left | Jump | Run | Reflect
    }

    public enum State
    {
        ForcedStill,
        Alive,
        Invincible,
        //Reflect,
        Dead
    }

    public const int MaxHealth = 4;
    public const float InvincibilityDuration = 1f;
    public event System.Action<PlayerSetup> HackChanged;
    static PlayerSetup localInstance = null;//, onlineInstance = null;
    static readonly Dictionary<string, ActiveControls> controlsConversion = new Dictionary<string, ActiveControls>();
    static readonly Dictionary<string, PlayerSetup> allPlayersCache = new Dictionary<string, PlayerSetup>();

    [SerializeField]
    UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController controller;
    [SerializeField]
    Camera view;
    [SerializeField]
    AudioListener listener;

    [Header("HUD info")]
    [SerializeField]
    Canvas hud;
    [SerializeField]
    GameObject healthIndicator;
    [SerializeField]
    Image forwardDisabled;
    [SerializeField]
    Image backDisabled;
    [SerializeField]
    Image rightDisabled;
    [SerializeField]
    Image leftDisabled;
    [SerializeField]
    Image jumpDisabled;
    [SerializeField]
    Image runDisabled;
    [SerializeField]
    Image reflectDisabled;

    [SyncVar]
    int health = MaxHealth;
    [SyncVar]
    int currentActiveControls = (int)ActiveControls.All;
    [SyncVar]
    int currentState = (int)State.Alive;    // FIXME: change this to forcedstill at some point
    [SyncVar]
    bool reflectEnabled = false;

    // Member variables for updating
    ActiveControls lastFramesControls = ActiveControls.All;
    int lastFramesHealth = MaxHealth;
    NetworkInstanceId playerId;
    string uniquePlayerIdName;
    float timeLastInvincible = -1f;

    readonly ActiveControls[] hackedControls = new ActiveControls[] { ActiveControls.None, ActiveControls.None };
    readonly GameObject[] healthIndicators = new GameObject[MaxHealth];
    readonly Dictionary<ActiveControls, Image> disableGraphics = new Dictionary<ActiveControls, Image>();

    public static PlayerSetup FindPlayer(string id)
    {
        PlayerSetup returnScript = null;
        if (allPlayersCache.TryGetValue(id, out returnScript) == false)
        {
            GameObject copy = GameObject.Find(id);
            if(copy != null)
            {
                returnScript = copy.GetComponent<PlayerSetup>();
                if(returnScript != null)
                {
                    allPlayersCache.Add(id, returnScript);
                }
            }
        }
        return returnScript;
    }

    #region Static Properties
    public static PlayerSetup LocalInstance
    {
        get
        {
            return localInstance;
        }
    }

    //public static PlayerSetup OnlineInstance
    //{
    //    get
    //    {
    //        return onlineInstance;
    //    }
    //}

    public static Dictionary<string, ActiveControls> ControlsDictionary
    {
        get
        {
            if (controlsConversion.Count <= 0)
            {
                ActiveControls temp = ActiveControls.None;
                controlsConversion.Add(temp.ToString(), temp);
                for (int i = 0; i < (int)ActiveControls.NumControls; ++i)
                {
                    temp = (ActiveControls)(1 << i);
                    controlsConversion.Add(temp.ToString(), temp);
                }
            }
            return controlsConversion;
        }
    }
    #endregion

    #region Local Properties
    public int Health
    {
        get
        {
            return health;
        }
        private set
        {
            int setValueTo = Mathf.Clamp(value, 0, MaxHealth);
            if(health != setValueTo)
            {
                health = setValueTo;
                if (health > 0)
                {
                    CurrentState = State.Invincible;
                    timeLastInvincible = (Time.time + InvincibilityDuration);
                }
                else
                {
                    CurrentState = State.Dead;
                }
                CmdSetStatus(health, currentState);
            }
        }
    }

    public ActiveControls CurrentActiveControls
    {
        get
        {
            return (ActiveControls)currentActiveControls;
        }
        private set
        {
            int setValueTo = (int)value;
            if (currentActiveControls != setValueTo)
            {
                // Send the server the information of the current active controls
                TransmitOurControls(setValueTo);
            }
        }
    }

    public ActiveControls[] DeactivatedControls
    {
        get
        {
            return hackedControls;
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
                currentState = setValueTo;
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
            if(reflectEnabled != value)
            {
                reflectEnabled = value;
            }
        }
    }
    #endregion

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        ClientSetup();
        SetName();
    }

    // Use this for initialization
    [Client]
    void Start ()
    {
        // Setup what's available
        controller.enabled = isLocalPlayer;
        view.enabled = isLocalPlayer;
        listener.enabled = isLocalPlayer;

        // Reset variables
        Health = MaxHealth;
        lastFramesHealth = Health;
        CurrentActiveControls = ActiveControls.All;
        lastFramesControls = CurrentActiveControls;
    }

    void Update()
    {
        if (isLocalPlayer == true)
        {
            UpdateControlsHud();
            UpdateHealthHud();
            if((CurrentState == State.Invincible) && (Time.time > timeLastInvincible))
            {
                CurrentState = State.Alive;
            }
            // FIXME: not working
            //if(Input.GetKeyDown(KeyCode.E) == true)
            //{
            //    foreach (KeyValuePair<string, PlayerSetup> pair in allPlayersCache)
            //    {
            //        if (pair.Key != name)
            //        {
            //            pair.Value.InflictDamage();
            //        }
            //    }
            //}
        }
        SetName();
    }

    [Client]
    public void Hack(byte index, ActiveControls controlValue)
    {
        if (isLocalPlayer == true)
        {
            hackedControls[index] = controlValue;

            // Update the controls
            ActiveControls disabledControls = ActiveControls.All;
            if (hackedControls[0] != ActiveControls.None)
            {
                disabledControls ^= hackedControls[0];
            }
            if ((hackedControls[1] != ActiveControls.None) && (hackedControls[0] != hackedControls[1]))
            {
                disabledControls ^= hackedControls[1];
            }

            CmdSetOpponentsControls((int)disabledControls);

            // Run event
            if (HackChanged != null)
            {
                HackChanged(this);
            }
        }
    }

    [Client]
    public void InflictDamage(int damage = 1)
    {
        if(CurrentState == State.Alive)
        {
            // FIXME: not working
            Health -= damage;
        }
    }

    #region Commands
    [Command]
    void CmdSubmitName(string name)
    {
        uniquePlayerIdName = name;
    }

    [Command]
    void CmdSetOpponentsControls(int setValueTo)
    {
        foreach (KeyValuePair<string, PlayerSetup> pair in allPlayersCache)
        {
            if (pair.Key != name)
            {
                pair.Value.currentActiveControls = setValueTo;
            }
        }
    }

    [Command]
    void CmdSetOurControls(int setValueTo)
    {
        currentActiveControls = setValueTo;
    }

    [Command]
    void CmdSetStatus(int newHealth, int newState)
    {
        health = newHealth;
        currentState = newState;
    }
    #endregion

    #region Client
    [Client]
    void TransmitOurControls(int setValueTo)
    {
        currentActiveControls = setValueTo;
        CmdSetOurControls(setValueTo);
    }

    [Client]
    void ClientSetup()
    {
        // Indicate this is the local instance
        localInstance = this;

        // Setup ID
        playerId = GetComponent<NetworkIdentity>().netId;
        CmdSubmitName(GenerateName());

        // Disable the camera
        GameSetup startCamera = GameObject.FindObjectOfType<GameSetup>();
        startCamera.GetComponent<Camera>().enabled = false;
        startCamera.GetComponent<AudioListener>().enabled = false;
        SceneManager.CursorMode = CursorLockMode.Locked;

        if (healthIndicators[0] == null)
        {
            SetupHud();
        }
    }

    [Client]
    private void UpdateHealthHud()
    {
        if (lastFramesHealth != Health)
        {
            for (int i = 0; i < MaxHealth; ++i)
            {
                healthIndicators[i].SetActive(i < health);
            }
            lastFramesHealth = Health;
        }
    }

    [Client]
    private void UpdateControlsHud()
    {
        if (lastFramesControls != CurrentActiveControls)
        {
            // Update controls
            foreach (KeyValuePair<ActiveControls, Image> pair in disableGraphics)
            {
                pair.Value.enabled = ((pair.Key & CurrentActiveControls) == 0);
            }
            lastFramesControls = CurrentActiveControls;
        }
    }
    #endregion

    #region Helper Methods
    private void SetName()
    {
        if ((string.IsNullOrEmpty(name) == true) || (name == "Player(Clone)"))
        {
            if (isLocalPlayer == false)
            {
                name = uniquePlayerIdName;
                if (allPlayersCache.ContainsKey(name) == false)
                {
                    allPlayersCache.Add(name, this);
                }
            }
            else
            {
                name = GenerateName();
                if(allPlayersCache.ContainsKey(name) == false)
                {
                    allPlayersCache.Add(name, this);
                }
            }
        }
    }

    string GenerateName()
    {
        return "Player " + playerId.ToString();
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

        hud.gameObject.SetActive(isLocalPlayer);
        hud.transform.SetParent(null, true);

        if(disableGraphics.Count <= 0)
        {
            disableGraphics.Add(ActiveControls.Forward, forwardDisabled);
            disableGraphics.Add(ActiveControls.Back, backDisabled);
            disableGraphics.Add(ActiveControls.Right, rightDisabled);
            disableGraphics.Add(ActiveControls.Left, leftDisabled);
            disableGraphics.Add(ActiveControls.Jump, jumpDisabled);
            disableGraphics.Add(ActiveControls.Run, runDisabled);
            disableGraphics.Add(ActiveControls.Reflect, reflectDisabled);
        }
    }
    #endregion
}
