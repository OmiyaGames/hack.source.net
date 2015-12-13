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
        Reflect,
        Dead
    }

    public const int MaxHealth = 4;
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

    // Member variables for updating
    ActiveControls lastFramesControls = ActiveControls.All;
    int lastFramesHealth = MaxHealth;
    NetworkInstanceId playerId;
    string uniquePlayerIdName;

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
        set
        {
            int setValueTo = Mathf.Clamp(value, 0, MaxHealth);
            if(health != setValueTo)
            {
                TransmitOurHealth(setValueTo);
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
        set
        {
            int setValueTo = (int)value;
            if (currentState != setValueTo)
            {
                currentState = setValueTo;
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

    [Client]
    void Update()
    {
        if(lastFramesControls != CurrentActiveControls)
        {
            // Update controls
            foreach (KeyValuePair<ActiveControls, Image> pair in disableGraphics)
            {
                pair.Value.enabled = ((pair.Key & CurrentActiveControls) == 0);
            }
            lastFramesControls = CurrentActiveControls;
        }
        if(lastFramesHealth != Health)
        {
            for (int i = 0; i < MaxHealth; ++i)
            {
                healthIndicators[i].SetActive(i < health);
            }
            lastFramesHealth = Health;
        }
        SetName();


        // FIXME: debugging!
        if((isLocalPlayer == true) && (Input.GetMouseButtonDown(0) == true))
        {
            Hit();
        }
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
    public void Hit(int damage = 1)
    {
        CmdDecreaseOpponentsHealth(damage);
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
    void CmdDecreaseOpponentsHealth(int damage)
    {
        foreach (KeyValuePair<string, PlayerSetup> pair in allPlayersCache)
        {
            if (pair.Key != name)
            {
                pair.Value.health -= damage;
                if(pair.Value.health <= 0)
                {
                    pair.Value.health = 0;
                    pair.Value.currentState = (int)State.Dead;
                }
            }
        }
    }

    [Command]
    void CmdSetOurHealth(int setValueTo)
    {
        health = setValueTo;
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
    void TransmitOurHealth(int setValueTo)
    {
        health = setValueTo;
        CmdSetOurHealth(health);
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
