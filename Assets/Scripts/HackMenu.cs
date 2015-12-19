using UnityEngine;
using UnityEngine.UI;
using OmiyaGames;
using System.Collections.Generic;
using System;

public class HackMenu : IMenu
{
    byte currentControlIndex = 0;
    byte otherControlIndex = 0;

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
            return currentControlIndex;
        }
    }
	
	public void SetIndexes(byte currentIndex, byte otherIndex)
	{
		// Set member variables
		currentControlIndex = currentIndex;
		otherControlIndex = otherIndex;
		
		// Set fonts
		PlayerSetup.ActiveControls currentControl = PlayerSetup.LocalInstance.DeactivatedControls[currentControlIndex];
		foreach(Text text in texts.Values)
		{
			text.fontStyle = FontStyle.Normal;
		}
		texts[currentControl].fontStyle = FontStyle.Italic;
		
		// Enable all controls
		foreach(Button button in buttons.Values)
		{
			button.gameObject.SetActive(true);
		}
		
		// Disable controls based on other control's values
		PlayerSetup.ActiveControls otherControl = PlayerSetup.LocalInstance.DeactivatedControls[otherControlIndex];
		switch(otherControl)
		{
			case PlayerSetup.ActiveControls.Forward:
			case PlayerSetup.ActiveControls.Back:
			case PlayerSetup.ActiveControls.Left:
			case PlayerSetup.ActiveControls.Right:
				// Disable all directional controls
				buttons[PlayerSetup.ActiveControls.Forward].gameObject.SetActive(false);
				buttons[PlayerSetup.ActiveControls.Back].gameObject.SetActive(false);
				buttons[PlayerSetup.ActiveControls.Left].gameObject.SetActive(false);
				buttons[PlayerSetup.ActiveControls.Right].gameObject.SetActive(false);
				break;
			case PlayerSetup.ActiveControls.None:
				// Do nothing
				break;
			default:
				// Disable the control the other index already disabled
				buttons[otherControl].gameObject.SetActive(false);
				break;
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
            PlayerSetup.LocalInstance.Hack(currentControlIndex, controlValue);
        }

        // Indicate button is clicked
        Manager.ButtonClick.Play();
        Hide();
    }
}
