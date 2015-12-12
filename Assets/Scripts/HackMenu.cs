using UnityEngine;
using UnityEngine.UI;
using OmiyaGames;
using System.Collections.Generic;
using System;

public class HackMenu : IMenu
{
    GameObject defaultUi = null;
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

    readonly Dictionary<PlayerSetup.ActiveControls, GameObject> buttons = new Dictionary<PlayerSetup.ActiveControls, GameObject>();

    public override GameObject DefaultUi
    {
        get
        {
            return defaultUi;
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
            defaultUi = buttons[PlayerSetup.LocalInstance.DeactivatedControls[index]];
            Manager.SelectGuiGameObject(DefaultUi);
        }
    }

    public override void Show(Action<IMenu> stateChanged)
    {
        base.Show(stateChanged);
        if(buttons.Count <= 0)
        {
            buttons.Add(PlayerSetup.ActiveControls.None, None.gameObject);
            buttons.Add(PlayerSetup.ActiveControls.Forward, Up.gameObject);
            buttons.Add(PlayerSetup.ActiveControls.Back, Down.gameObject);
            buttons.Add(PlayerSetup.ActiveControls.Right, Right.gameObject);
            buttons.Add(PlayerSetup.ActiveControls.Left, Left.gameObject);
            buttons.Add(PlayerSetup.ActiveControls.Jump, Jump.gameObject);
            buttons.Add(PlayerSetup.ActiveControls.Run, Run.gameObject);
            buttons.Add(PlayerSetup.ActiveControls.Reflect, Reflect.gameObject);
        }
    }

    public void OnHackClicked(string hackValue)
    {
        PlayerSetup.ActiveControls controlValue;
        if (PlayerSetup.ControlsDictionary.TryGetValue(hackValue, out controlValue) == true)
        {
            PlayerSetup.LocalInstance.Hack(Index, controlValue);
        }
    }
}
