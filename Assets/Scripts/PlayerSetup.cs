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

    public const int MaxHealth = 4;
    public event System.Action<PlayerSetup> HackChanged;
    static PlayerSetup localInstance = null;
    static readonly Dictionary<string, ActiveControls> controlsConversion = new Dictionary<string, ActiveControls>();

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

    // FIXME: somehow get a reference to the opposing player
    [SyncVar]
    int health = MaxHealth;
    [SyncVar]
    int currentActiveControls = (int)ActiveControls.All;

    readonly ActiveControls[] hackedControls = new ActiveControls[] { ActiveControls.None, ActiveControls.None };
    readonly GameObject[] healthIndicators = new GameObject[MaxHealth];
    readonly Dictionary<ActiveControls, Image> disableGraphics = new Dictionary<ActiveControls, Image>();

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
                health = setValueTo;
                for (int i = 0; i < MaxHealth; ++i)
                {
                    healthIndicators[i].SetActive(i < health);
                }
            }
        }
    }

    public ActiveControls CurrentActiveControls
    {
        get
        {
            return (ActiveControls)currentActiveControls;
        }
        set
        {
            int setValueTo = (int)value;
            if (currentActiveControls != setValueTo)
            {
                currentActiveControls = setValueTo;

                // Update controls
                foreach (KeyValuePair<ActiveControls, Image> pair in disableGraphics)
                {
                    pair.Value.enabled = ((pair.Key & CurrentActiveControls) == 0);
                }
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
    #endregion

    // Use this for initialization
    void Start ()
    {
        if (isLocalPlayer == true)
        {
            // Indicate this is the local instance
            localInstance = this;

            // Disable the camera
            GameObject startCamera = GameObject.Find("StartCamera");
            startCamera.GetComponent<Camera>().enabled = false;
            startCamera.GetComponent<AudioListener>().enabled = false;
            SceneManager.CursorMode = CursorLockMode.Locked;

            if (healthIndicators[0] == null)
            {
                SetuHUD();
            }
        }

        // Setup what's available
        controller.enabled = isLocalPlayer;
        view.enabled = isLocalPlayer;
        listener.enabled = isLocalPlayer;

        // Reset variables
        Health = MaxHealth;
        currentActiveControls = (int)ActiveControls.All;
    }

    public void Hack(byte index, ActiveControls controlValue)
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

        // Run event
        if (HackChanged != null)
        {
            HackChanged(this);
        }

        // FIXME: debugging by setting this instance's controls
        CurrentActiveControls = disabledControls;
    }

    private void SetuHUD()
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

}
