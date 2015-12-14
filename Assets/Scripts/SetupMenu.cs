using UnityEngine;
using UnityEngine.UI;
using OmiyaGames;

public class SetupMenu : IMenu
{
    [SerializeField]
    Button backButton;

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

    public void OnBackClicked()
    {
        Hide();
        Manager.ButtonClick.Play();
    }
    public void OnHostClicked()
    {
        Manager.Show<HostMenu>();
        Manager.ButtonClick.Play();
    }
    public void OnJoinClicked()
    {
        Manager.Show<JoinMenu>();
        Manager.ButtonClick.Play();
    }
}