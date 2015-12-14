using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using OmiyaGames;
using System;

public class HostMenu : IMenu
{
    [SerializeField]
    Button backButton;
    [SerializeField]
    InputField hostIpAddress;

    public override GameObject DefaultUi
    {
        get
        {
            return backButton.gameObject;
        }
    }

    public override Type MenuType
    {
        get
        {
            return Type.ManagedMenu;
        }
    }

    public override void Show(Action<IMenu> stateChanged)
    {
        base.Show(stateChanged);
        
        // FIXME: get this host IP address somehow
        hostIpAddress.text = "127.0.0.1";

        // FIXME: start the server
        NetworkManager.singleton.StartHost();
    }

    public void OnBackClicked()
    {
        // FIXME: cancel server
        Hide();
        Manager.ButtonClick.Play();
    }

}
