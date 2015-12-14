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
    public event System.Action<PlayerSetup, string> NameChanged;
    static PlayerSetup localInstance = null;//, onlineInstance = null;
    static readonly Dictionary<string, ActiveControls> controlsConversion = new Dictionary<string, ActiveControls>();

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

    [Header("HUD info")]
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
    int playerBulletLayerId, oppositionBulletLayerId;

    readonly ActiveControls[] hackedControls = new ActiveControls[] { ActiveControls.None, ActiveControls.None };
    readonly Dictionary<ActiveControls, Image> disableGraphics = new Dictionary<ActiveControls, Image>();
    GameSetup gameCache = null;
    
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

    public GameSetup Game
    {
        get
        {
            if(gameCache == null)
            {
                gameCache = Singleton.Get<GameSetup>();
            }
            return gameCache;
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

    void OnDestroy()
    {
        GameState.UpdatePlayerSetup(null, name);
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
        if(Game.Info != null)
        {
            foreach (PlayerSetup opposition in Game.Info.Oppositions())
            {
                opposition.currentActiveControls = setValueTo;
            }
        }
    }

    [Command]
    void CmdSetOurControls(int setValueTo)
    {
        currentActiveControls = setValueTo;
    }

    [Command]
    void CmdSetupGameSetup(string name)
    {
        Debug.Log("Local Name" + name);

        if (Game.Info == null)
        {
            // Spawn GameState
            GameObject clone = Instantiate(gameInfoPrefab.gameObject);
            NetworkServer.Spawn(clone);
            Debug.Log("Clone success!");

            // Update its information
            clone.GetComponent<GameState>().LocalPlayerId = name;
        }
        else
        {
            Game.Info.LocalPlayerId = name;
        }
    }
    #endregion

    #region Helper Methods
    private void SetName()
    {
        if ((string.IsNullOrEmpty(name) == true) || (name == "Player(Clone)"))
        {
            string formerName = name;
            if (isLocalPlayer == false)
            {
                name = uniquePlayerIdName;
                GameState.UpdatePlayerSetup(this, formerName);
                if(NameChanged != null)
                {
                    NameChanged(this, name);
                }
            }
            else
            {
                name = GenerateName();
                GameState.UpdatePlayerSetup(this, formerName);
                Debug.Log("Name changed " + name);
                CmdSetupGameSetup(name);
                if (NameChanged != null)
                {
                    NameChanged(this, name);
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
            foreach (KeyValuePair<ActiveControls, Image> pair in disableGraphics)
            {
                pair.Value.enabled = ((pair.Key & CurrentActiveControls) == 0);
            }
            lastFramesControls = CurrentActiveControls;
        }
    }
    #endregion
}
