using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using OmiyaGames;

[RequireComponent(typeof(PlayerStatus))]
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
    int currentActiveControls = (int)ActiveControls.All;
    [SyncVar]
    string uniquePlayerIdName;

    // FIXME: remove frame variable in preference of hooking
    ActiveControls lastFramesControls = ActiveControls.All;
    NetworkInstanceId playerId;
    PlayerStatus playerStatus;

    readonly ActiveControls[] hackedControls = new ActiveControls[] { ActiveControls.None, ActiveControls.None };
    readonly Dictionary<ActiveControls, Image> disableGraphics = new Dictionary<ActiveControls, Image>();

    public static PlayerSetup FindPlayer(string id)
    {
        PlayerSetup returnScript = null;
        if (AllIdentifiedPlayers.TryGetValue(id, out returnScript) == false)
        {
            GameObject copy = GameObject.Find(id);
            if(copy != null)
            {
                returnScript = copy.GetComponent<PlayerSetup>();
                if(returnScript != null)
                {
                    AllIdentifiedPlayers.Add(id, returnScript);
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

    public static Dictionary<string, PlayerSetup> AllIdentifiedPlayers
    {
        get
        {
            return allPlayersCache;
        }
    }
    #endregion

    #region Local Properties
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

    public PlayerStatus Status
    {
        get
        {
            return playerStatus;
        }
    }
    #endregion

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        ClientSetup();
        SetName();

        // Reset control variables
        CurrentActiveControls = ActiveControls.All;
        lastFramesControls = CurrentActiveControls;
    }

    // Use this for initialization
    [Client]
    void Start ()
    {
        // Setup what's available
        controller.enabled = isLocalPlayer;
        view.enabled = isLocalPlayer;
        listener.enabled = isLocalPlayer;

        playerStatus = GetComponent<PlayerStatus>();
    }

    void Update()
    {
        if (isLocalPlayer == true)
        {
            UpdateControlsHud();
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

    #region Commands
    [Command]
    void CmdSubmitName(string name)
    {
        uniquePlayerIdName = name;
    }

    [Command]
    void CmdSetOpponentsControls(int setValueTo)
    {
        foreach (KeyValuePair<string, PlayerSetup> pair in AllIdentifiedPlayers)
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
    #endregion

    #region Helper Methods
    private void SetName()
    {
        if ((string.IsNullOrEmpty(name) == true) || (name == "Player(Clone)"))
        {
            if (isLocalPlayer == false)
            {
                name = uniquePlayerIdName;
                if (AllIdentifiedPlayers.ContainsKey(name) == false)
                {
                    AllIdentifiedPlayers.Add(name, this);
                }
            }
            else
            {
                name = GenerateName();
                if(AllIdentifiedPlayers.ContainsKey(name) == false)
                {
                    AllIdentifiedPlayers.Add(name, this);
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
        hud.gameObject.SetActive(isLocalPlayer);
        if (isLocalPlayer == true)
        {
            hud.transform.SetParent(null, true);

            if (disableGraphics.Count <= 0)
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
    }

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

        SetupHud();
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
}
