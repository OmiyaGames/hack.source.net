using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using OmiyaGames;

public class WinMenu : IMenu
{
    [SerializeField]
    Button defaultButton;

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
            return Type.ManagedMenu;
        }
    }

    public void OnQuitClicked()
    {
        NetworkManager.singleton.StopHost();
        Manager.ButtonClick.Play();
    }
}
