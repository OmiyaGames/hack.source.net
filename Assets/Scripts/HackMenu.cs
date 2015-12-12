using UnityEngine;
using UnityEngine.UI;
using OmiyaGames;
using System.Collections.Generic;
using System;

public class HackMenu : IMenu
{
    byte index = 0;

    [SerializeField]
    Button None;
    [SerializeField]
    Button Up;
    [SerializeField]
    Button Down;
    [SerializeField]
    Button Right;
    [SerializeField]
    Button Left;
    [SerializeField]
    Button Jump;
    [SerializeField]
    Button Run;
    [SerializeField]
    Button Reflect;

    readonly Dictionary<PlayerSetup.ActiveControls, Button> buttons = new Dictionary<PlayerSetup.ActiveControls, Button>();
    readonly Dictionary<PlayerSetup.ActiveControls, Text> texts = new Dictionary<PlayerSetup.ActiveControls, Text>();

    public override GameObject DefaultUi
    {
        get
        {
            return null;
        }
    }

    public override Type MenuType
    {
        get
        {
            return Type.ManagedMenu;
        }
    }

    public byte Index
    {
        get
        {
            return index;
        }
        set
        {
            index = value;
            foreach(Text text in texts.Values)
            {
                text.fontStyle = FontStyle.Normal;
            }
            texts[PlayerSetup.LocalInstance.DeactivatedControls[index]].fontStyle = FontStyle.Italic;
        }
    }

    public override void Show(Action<IMenu> stateChanged)
    {
        base.Show(stateChanged);
        if(buttons.Count <= 0)
        {
            buttons.Add(PlayerSetup.ActiveControls.None, None);
            buttons.Add(PlayerSetup.ActiveControls.Forward, Up);
            buttons.Add(PlayerSetup.ActiveControls.Back, Down);
            buttons.Add(PlayerSetup.ActiveControls.Right, Right);
            buttons.Add(PlayerSetup.ActiveControls.Left, Left);
            buttons.Add(PlayerSetup.ActiveControls.Jump, Jump);
            buttons.Add(PlayerSetup.ActiveControls.Run, Run);
            buttons.Add(PlayerSetup.ActiveControls.Reflect, Reflect);
            foreach (KeyValuePair<PlayerSetup.ActiveControls, Button> pair in buttons)
            {
                texts.Add(pair.Key, pair.Value.GetComponentInChildren<Text>(true));
            }
        }
    }

    public void OnHackClicked(string hackValue)
    {
        PlayerSetup.ActiveControls controlValue;
        if (PlayerSetup.ControlsDictionary.TryGetValue(hackValue, out controlValue) == true)
        {
            PlayerSetup.LocalInstance.Hack(Index, controlValue);
        }

        // Indicate button is clicked
        Manager.ButtonClick.Play();
        Hide();
    }
}
