using UnityEngine;
using UnityEngine.UI;
using OmiyaGames;

[RequireComponent(typeof(PauseMenu))]
public class PauseEnhanced : MonoBehaviour
{
    [SerializeField]
    Text hack1;
    [SerializeField]
    Text hack2;

    MenuManager manager = null;
    string hack1String, hack2String;
    System.Action<PlayerSetup> action = null;

    void Start()
    {
        manager = Singleton.Get<MenuManager>();
        hack1String = hack1.text;
        hack2String = hack2.text;
    }

    void Update()
    {
        if((action == null) && (PlayerSetup.LocalInstance != null))
        {
            action = new System.Action<PlayerSetup>(UpdateText);
            PlayerSetup.LocalInstance.HackChanged += action;
            UpdateText(PlayerSetup.LocalInstance);
        }
    }

    public void OnHackClicked(bool id)
    {
        // Open the options dialog
        HackMenu menu = manager.Show<HackMenu>();
        menu.Index = (byte)(id ? 1 : 0);

        // Indicate button is clicked
        manager.ButtonClick.Play();
    }

    void UpdateText(PlayerSetup instance)
    {
        hack1.text = string.Format(hack1String, instance.DeactivatedControls[0]);
        hack2.text = string.Format(hack2String, instance.DeactivatedControls[1]);
    }
}
