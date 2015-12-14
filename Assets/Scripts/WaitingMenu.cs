using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using OmiyaGames;
using System;

public class WaitingMenu : IMenu
{
    [SerializeField]
    Button defaultButton;
    [SerializeField]
    Text ipAddress;

    Action<float> checkPlayerNumber = null;

    public override GameObject DefaultUi
    {
        get
        {
            return defaultButton.gameObject;
        }
    }

    public override Type MenuType
    {
        get
        {
            return Type.DefaultManagedMenu;
        }
    }

    // FIXME: override the show function to indicate this host's IP Address
    public override void Show(Action<IMenu> stateChanged)
    {
        base.Show(stateChanged);

        // Setup cursors
        SceneManager.CursorMode = CursorLockMode.None;
        Singleton.Get<MenuManager>().CursorModeOnPause = CursorLockMode.None;

        // Show IP address
        ipAddress.text = HostMenu.GetIpAddress();

        // Bind to update event
        Cleanup();
        checkPlayerNumber = new Action<float>(CheckPlayerNumber);
        Singleton.Instance.OnUpdate += checkPlayerNumber;
    }

    public override void Hide()
    {
        base.Hide();
        Cleanup();
    }

    public void OnQuitClicked()
    {
        NetworkManager.singleton.StopHost();
        Manager.ButtonClick.Play();
        Hide();
    }

    void OnDestroy()
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (checkPlayerNumber != null)
        {
            Singleton.Instance.OnUpdate -= checkPlayerNumber;
            checkPlayerNumber = null;
        }
    }

    void CheckPlayerNumber(float deltaTime)
    {
        // Check the number of players
        if((checkPlayerNumber != null) && (PlayerSetup.AllIdentifiedPlayers.Count >= GameSetup.MaxConnections))
        {
            // Hide this dialog
            Hide();
            Cleanup();
        }
    }
}
