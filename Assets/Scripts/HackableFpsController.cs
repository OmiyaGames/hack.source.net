﻿using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(PlayerSetup))]
public class HackableFpsController : RigidbodyFirstPersonController
{
    PlayerSetup player;

    bool lastLeftDisabled = false,
         lastRightDisabled = false,
         lastForwardDisabled = false,
         lastBackDisabled = false,
        lastJumpDisabled = false,
        lastRunDisabled = false;


    protected override void Start()
    {
        base.Start();
        player = GetComponent<PlayerSetup>();
    }

    protected override Vector2 GetMovementInput()
    {
        Vector2 input = new Vector2
        {
            x = Horizontal,
            y = Vertical
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }

    protected override bool GetJumpInput()
    {
        bool returnFlag = base.GetJumpInput();
        player.PressControls(PlayerSetup.ActiveControls.Jump, returnFlag);
        if ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Jump) == 0)
        {
            returnFlag = false;
        }
        return returnFlag;
    }

    protected override bool GetRunInput()
    {
        bool returnFlag = CrossPlatformInputManager.GetButton("Run");
        player.PressControls(PlayerSetup.ActiveControls.Run, returnFlag);
        if ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Run) == 0)
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
            if ((returnValue < 0) && ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Left) == 0))
            {
                returnValue = 0;
            }
            player.PressControls(PlayerSetup.ActiveControls.Left, (returnValue < 0));

            // Check right
            if ((returnValue > 0) && ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Right) == 0))
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
            if ((returnValue < 0) && ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Back) == 0))
            {
                returnValue = 0;
            }
            player.PressControls(PlayerSetup.ActiveControls.Back, (returnValue < 0));

            if ((returnValue > 0) && ((player.CurrentActiveControls & PlayerSetup.ActiveControls.Forward) == 0))
            {
                returnValue = 0;
            }
            player.PressControls(PlayerSetup.ActiveControls.Forward, (returnValue > 0));

            return returnValue;
        }
    }
}
