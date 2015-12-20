﻿using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(PlayerSetup))]
public class HackableFpsController : RigidbodyFirstPersonController
{
    PlayerSetup player;

    Vector2 lastInput;

    public Vector2 LastInput
    {
        get
        {
            return lastInput;
        }
    }

    protected override void Start()
    {
        base.Start();
        player = GetComponent<PlayerSetup>();
    }

    protected override Vector2 GetMovementInput()
    {
        lastInput.x = Horizontal;
        lastInput.y = Vertical;
        movementSettings.UpdateDesiredTargetSpeed(lastInput);
        return lastInput;
    }

    protected override bool GetJumpInput()
    {
        bool returnFlag = base.GetJumpInput();
        player.PressControls(PlayerSetup.ActiveControls.Jump, returnFlag);
        if (player.IsControlActive(PlayerSetup.ActiveControls.Jump) == false)
        {
            returnFlag = false;
        }
        return returnFlag;
    }

    protected override bool GetRunInput()
    {
        bool returnFlag = CrossPlatformInputManager.GetButton("Run");
        player.PressControls(PlayerSetup.ActiveControls.Run, returnFlag);
        if (player.IsControlActive(PlayerSetup.ActiveControls.Run) == false)
        {
            returnFlag = false;
        }
        return returnFlag;
    }

    float Horizontal
    {
        get
        {
            float returnValue = CrossPlatformInputManager.GetAxis("Horizontal");

            // Check left
            if ((returnValue < 0) && (player.IsControlActive(PlayerSetup.ActiveControls.Left) == false))
            {
                returnValue = 0;
            }
            player.PressControls(PlayerSetup.ActiveControls.Left, (returnValue < 0));

            // Check right
            if ((returnValue > 0) && (player.IsControlActive(PlayerSetup.ActiveControls.Right) == false))
            {
                returnValue = 0;
            }
            player.PressControls(PlayerSetup.ActiveControls.Right, (returnValue > 0));
            return returnValue;
        }
    }

    float Vertical
    {
        get
        {
            float returnValue = CrossPlatformInputManager.GetAxis("Vertical");

            // Check back
            if ((returnValue < 0) && (player.IsControlActive(PlayerSetup.ActiveControls.Back) == false))
            {
                returnValue = 0;
            }
            player.PressControls(PlayerSetup.ActiveControls.Back, (returnValue < 0));

            if ((returnValue > 0) && (player.IsControlActive(PlayerSetup.ActiveControls.Forward) == false))
            {
                returnValue = 0;
            }
            player.PressControls(PlayerSetup.ActiveControls.Forward, (returnValue > 0));

            return returnValue;
        }
    }
}
