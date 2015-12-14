using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using OmiyaGames;
using System;
using System.Net;

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
        
        // Get this host IP address somehow
        hostIpAddress.text = GetIpAddress();

        // FIXME: start the server
        NetworkManager.singleton.StartHost();
    }

    public void OnBackClicked()
    {
        // FIXME: cancel server
        Hide();
        Manager.ButtonClick.Play();
    }

    public static string GetIpAddress()
    {
        // FIXME: polish this part significantly!
        string strIp = "127.0.0.1";
        IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
        if (IpEntry != null)
        {
            strIp = IpEntry.AddressList[0].ToString();
        }
        return strIp;
    }
}
