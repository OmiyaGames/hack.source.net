using UnityEngine;
using UnityEngine.UI;
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
        // FIXME: start the server
        // FIXME: get this host IP address somehow
        hostIpAddress.text = "127.0.0.1";
        base.Show(stateChanged);
    }

    public void OnBackClicked()
    {
        // FIXME: cancel server
        Hide();
        Manager.ButtonClick.Play();
    }

}
