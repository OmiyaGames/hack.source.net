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

    public const string ShootTrigger = "Shoot";
    public const string HackTrigger = "Hack";
    public const string ReflectBool = "Reflect";
    public const string InvincibleBool = "Invincible";
    public const string EnabledBool = "Enabled";
    public const string PressedTriger = "Pressed";

    public event System.Action<PlayerSetup> HackChanged;
    public event System.Action<PlayerSetup, string> NameChanged;
    static PlayerSetup localInstance = null;
    static readonly Dictionary<string, ActiveControls> controlsConversion = new Dictionary<string, ActiveControls>();
    static readonly Dictionary<string, PlayerSetup> allPlayersCache = new Dictionary<string, PlayerSetup>();

    #region Helper Classes
    [System.Serializable]
    public class RigidBodyInfo
    {
        public UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController controller;
        public GameObject[] playerStuff;
        public GameObject[] oppositionStuff;
        [Header("Layers")]
        public GameObject[] updateLayers;

        public void Setup(bool isLocal)
        {
            if(controller != null)
            {
                controller.enabled = isLocal;
            }
            if (playerStuff != null)
            {
                for (int i = 0; i < playerStuff.Length; ++i)
                {
                    if (playerStuff[i] != null)
                    {
                        playerStuff[i].SetActive(isLocal);
                    }
                }
            }
            if (oppositionStuff != null)
            {
                for (int i = 0; i < oppositionStuff.Length; ++i)
                {
                    if (oppositionStuff[i] != null)
                    {
                        oppositionStuff[i].SetActive(!isLocal);
                    }
                }
            }
            if(updateLayers != null)
            {
                for (int i = 0; i < updateLayers.Length; ++i)
                {
                    if (updateLayers[i] != null)
                    {
                        if(isLocal == true)
                        {
                            updateLayers[i].layer = GameSetup.playerAvatarLayerInt;
                        }
                        else
                        {
                            updateLayers[i].layer = GameSetup.oppositionAvatarLayerInt;
                        }
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class CharacterControllerInfo
    {
        public HackableFpsCharacterController controller;
        public GameObject[] playerStuff;
        public GameObject[] oppositionStuff;

        public void Setup(bool isLocal)
        {
            if (controller != null)
            {
                controller.enabled = isLocal;
            }
            if (playerStuff != null)
            {
                for(int i = 0; i < playerStuff.Length; ++i)
                {
                    if(playerStuff[i] != null)
                    {
                        playerStuff[i].SetActive(isLocal);
                    }
                }
            }
            if (oppositionStuff != null)
            {
                for (int i = 0; i < oppositionStuff.Length; ++i)
                {
                    if (oppositionStuff[i] != null)
                    {
                        oppositionStuff[i].SetActive(!isLocal);
                    }
                }
            }
        }
    }
    #endregion

    [SerializeField]
    RigidBodyInfo rigidBodyInfo = new RigidBodyInfo();
    [SerializeField]
    CharacterControllerInfo characterControllerInfo = new CharacterControllerInfo();
    [SerializeField]
    GameState gameInfoPrefab;

    [Header("Animations")]
    [SerializeField]
    public Animator hudAnimations;

    [Header("HUD info")]
    [SerializeField]
    Animator forwardDisabled;
    [SerializeField]
    Animator backDisabled;
    [SerializeField]
    Animator rightDisabled;
    [SerializeField]
    Animator leftDisabled;
    [SerializeField]
    Animator jumpDisabled;
    [SerializeField]
    Animator runDisabled;
    [SerializeField]
    Animator reflectDisabled;

    [Header("HUD info")]
    [SerializeField]
    SoundEffect hackedSound;
    [SerializeField]
    SoundEffect disabledSound;

    [SyncVar]
    int currentActiveControls = (int)ActiveControls.All;
    [SyncVar]
    string uniquePlayerIdName;

    // FIXME: remove frame variable in preference of hooking
    ActiveControls lastFramesControls = ActiveControls.All;
    NetworkInstanceId playerId;
    PlayerStatus playerStatus;

    readonly ActiveControls[] hackedControls = new ActiveControls[] { ActiveControls.None, ActiveControls.None };
    readonly Dictionary<ActiveControls, Animator> disableGraphics = new Dictionary<ActiveControls, Animator>();
    
    #region Static Properties
    public static void Reset()
    {
        allPlayersCache.Clear();
    }

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
            ActiveControls returnControls = ActiveControls.All;
            if ((AllIdentifiedPlayers.Count >= GameSetup.MaxConnections) && (playerStatus != null))
            {
                switch (playerStatus.CurrentState)
                {
                    case PlayerStatus.State.Alive:
                    case PlayerStatus.State.Invincible:
                        returnControls = (ActiveControls)currentActiveControls;
                        break;
                }
            }
            return returnControls;
        }
        //private set
        //{
        //    int setValueTo = (int)value;
        //    if (currentActiveControls != setValueTo)
        //    {
        //        // Send the server the information of the current active controls
        //        TransmitOurControls(setValueTo);
        //    }
        //}
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
        lastFramesControls = CurrentActiveControls;
    }

    // Use this for initialization
    [Client]
    void Start ()
    {
        // Setup what's available
        rigidBodyInfo.Setup(isLocalPlayer);
        characterControllerInfo.Setup(isLocalPlayer);

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

            CmdSetOpponentsControls((int)disabledControls, uniquePlayerIdName);

            // Run event
            if (HackChanged != null)
            {
                HackChanged(this);
            }
        }
    }

    [Command]
    public void CmdSetLosingPlayer()
    {
        //Debug.Log("PlayerSetup.SetLosingPlayer()");
        GameState.Instance.CmdSetLosingPlayer(name);
    }

    #region Commands
    [Command]
    void CmdSubmitName(string name)
    {
        uniquePlayerIdName = name;
    }

    [Command]
    void CmdSetOpponentsControls(int setValueTo, string ignorePlayer)
    {
        //Debug.Log("Hacking: ignore " + uniquePlayerIdName);
        foreach (KeyValuePair<string, PlayerSetup> pair in AllIdentifiedPlayers)
        {
            if (pair.Key != ignorePlayer)
            {
                //Debug.Log("Hacking: affect " + pair.Key);
                pair.Value.currentActiveControls = setValueTo;
            }
        }
    }

    //[Command]
    //void CmdSetOurControls(int setValueTo)
    //{
    //    currentActiveControls = setValueTo;
    //}

    [Command]
    void CmdSetupGameSetup(string name)
    {
        //Debug.Log("Local Name" + name);

        if (GameState.Instance == null)
        {
            // Spawn GameState
            GameObject clone = Instantiate(gameInfoPrefab.gameObject);
            NetworkServer.SpawnWithClientAuthority(clone, connectionToClient);
            //Debug.Log("Clone success!");
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
                AddPlayer(name);
                if (NameChanged != null)
                {
                    NameChanged(this, name);
                }
            }
            else
            {
                name = GenerateName();
                CmdSetupGameSetup(name);
                AddPlayer(name);
                if (NameChanged != null)
                {
                    NameChanged(this, name);
                }
            }
        }
    }

    private void AddPlayer(string name)
    {
        if ((string.IsNullOrEmpty(name) == false) && (AllIdentifiedPlayers.ContainsKey(name) == false) && (name != "Player(Clone)"))
        {
            //Debug.Log("Added player: " + name);
            AllIdentifiedPlayers.Add(name, this);
        }
    }

    string GenerateName()
    {
        return "Player " + playerId.ToString();
    }

    private void SetupHud()
    {
        if ((isLocalPlayer == true) && (disableGraphics.Count <= 0))
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

    //[Client]
    //void TransmitOurControls(int setValueTo)
    //{
    //    currentActiveControls = setValueTo;
    //    CmdSetOurControls(setValueTo);
    //}

    [Client]
    void ClientSetup()
    {
        // Indicate this is the local instance
        localInstance = this;

        // Setup ID
        playerId = GetComponent<NetworkIdentity>().netId;
        CmdSubmitName(GenerateName());

        // Disable the camera
        GameObject startCamera = GameObject.Find("StartCamera");
        startCamera.SetActive(false);
        SceneManager.CursorMode = CursorLockMode.Locked;

        SetupHud();
    }

    [Client]
    private void UpdateControlsHud()
    {
        if (lastFramesControls != CurrentActiveControls)
        {
            // Update controls
            foreach (KeyValuePair<ActiveControls, Animator> pair in disableGraphics)
            {
                pair.Value.SetBool(EnabledBool, ((pair.Key & CurrentActiveControls) != 0));
            }
            hackedSound.Play();
            lastFramesControls = CurrentActiveControls;
        }
    }

    [Client]
    public void PressControls(ActiveControls control, bool pressed)
    {
        if (isLocalPlayer == true)
        {
            Animator temp = null;
            if (disableGraphics.TryGetValue(control, out temp) == true)
            {
                if ((temp.gameObject.activeInHierarchy == true) && (temp.GetBool(PressedTriger) != pressed))
                {
                    // Check if button is disabled
                    if ((pressed == true) && ((control & CurrentActiveControls) == 0))
                    {
                        disabledSound.Play();
                    }
                    temp.SetBool(PressedTriger, pressed);
                }
            }
        }
    }
#endregion
}
