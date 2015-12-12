using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour
{
    public static PlayerSetup LocalInstance
    {
        get
        {
            return localInstance;
        }
    }

    [System.Flags]
    public enum ActiveControls : uint
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
    static PlayerSetup localInstance = null;

    [SerializeField]
    UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController controller;
    [SerializeField]
    Camera view;
    [SerializeField]
    AudioListener listener;
    [SerializeField]
    Canvas hud;
    [SerializeField]
    GameObject healthIndicator;

    // FIXME: somehow get a reference to the opposing player
    [SyncVar]
    int health = MaxHealth;
    [SyncVar]
    int currentActiveControls = (int)ActiveControls.All;

    readonly ActiveControls[] deactivatedControls = new ActiveControls[] { ActiveControls.None, ActiveControls.None };
    readonly GameObject[] healthIndicators = new GameObject[MaxHealth];

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

            if(healthIndicators[0] == null)
            {
                healthIndicators[0] = healthIndicator;
                GameObject newIndicator = null;
                for(int i = 1; i < MaxHealth; ++i)
                {
                    newIndicator = Instantiate<GameObject>(healthIndicator);
                    newIndicator.transform.SetParent(healthIndicator.transform.parent, false);
                    newIndicator.transform.SetAsLastSibling();
                    newIndicator.transform.localScale = Vector3.one;
                    healthIndicators[i] = newIndicator;
                }

                hud.gameObject.SetActive(isLocalPlayer);
                hud.transform.SetParent(null, true);
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
}
